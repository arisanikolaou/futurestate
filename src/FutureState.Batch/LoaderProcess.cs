using System;
using System.Collections.Generic;
using System.Linq;

using FutureState.Specifications;
using NLog;

namespace FutureState.Batch
{
    /// <summary>
    ///     Controls how data is loaded from an incoming stream to a target data store.
    /// </summary>
    /// <typeparam name="TEntityDtoIn">The data type of the entity to read in.</typeparam>
    public class LoaderProcess<TEntityDtoIn, TLoadStateData>
        where TLoadStateData : new()
    {
        /// <summary>
        ///     Gets the default max batch size;
        /// </summary>
        public const int DefaultMaxBatchSize = 10000;

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IProvideSpecifications<TEntityDtoIn> _validator;

        private bool _isInitialized;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="validator">
        ///     Optional validator to use. Null will use 'SpecProvider'
        /// </param>
        public LoaderProcess(IProvideSpecifications<TEntityDtoIn> validator = null)
        {
            _validator = validator ?? new SpecProvider<TEntityDtoIn>();
        }

        /// <summary>
        ///     Gets the function to get the entities to read in and process.
        /// </summary>
        public IEnumerable<TEntityDtoIn> EntitiesGet { get; set; }

        /// <summary>
        ///     Gets the function to map incoming entities to outgoing entities. By default matching properties will be mapped.
        /// </summary>
        public Action<LoaderState<TLoadStateData>, TEntityDtoIn> Mapper { get; set; }

        /// <summary>
        ///     Gets the function to call to commit changes to the target data store.
        /// </summary>
        public Action<LoaderState<TLoadStateData>> Commit { get; set; }

        /// <summary>
        ///     Initializes the loader process.
        /// </summary>
        public Action Initialize { get; set; }

        /// <summary>
        ///     Gets the current loader state being updated.
        /// </summary>
        public LoaderState<TLoadStateData> CurrentLoaderState { get; private set; }

        /// <summary>
        ///     Gets the maximum batch size to process entities.
        /// </summary>
        public int? MaxBatchSize { get; set; }

        /// <summary>
        ///     Loads data from the incoming data stream, controls validating of incoming objects and calls commit when all valid
        ///     objects have been loaded.
        /// </summary>
        /// <returns>The load process result state.</returns>
        public ILoaderState Process()
        {
            if (Mapper == null)
                throw new InvalidOperationException("Mapper action has not been assigned.");

            if (Commit == null)
                throw new InvalidOperationException("Commit action has not been assigned.");

            // start time will be assigned
            var loaderState = new LoaderState<TLoadStateData>();

            CurrentLoaderState = loaderState;

            // ReSharper disable once SuggestVarOrType_Elsewhere
            var exceptions = loaderState.Errors;

            try
            {
                loaderState.CurrentRow = 0;

                // the rules to validate incoming data
                // ReSharper disable once SuggestVarOrType_Elsewhere
                var rules = _validator.GetSpecifications().ToArray();

                // initialize lookups only once
                if (!_isInitialized)
                {
                    Initialize?.Invoke();

                    _isInitialized = true;
                }

                IEnumerable<TEntityDtoIn> dataSource = EntitiesGet;
                if (dataSource == null)
                    throw new InvalidOperationException("'EntitiesGet' is null.");

                var currentBatch = 0;

                int maxBatchSize = MaxBatchSize ?? DefaultMaxBatchSize;
                if (maxBatchSize < 1)
                {
                    if (_logger.IsWarnEnabled)
                        _logger.Warn(
                            $"Fixing up max batch size as the supplied value is less than 1. Current value is {MaxBatchSize} which is being set to {DefaultMaxBatchSize}.");

                    maxBatchSize = DefaultMaxBatchSize;
                }

                if (_logger.IsDebugEnabled)
                    _logger.Debug($"Processing {typeof(TEntityDtoIn).Name} in batches of {maxBatchSize}.");

                foreach (IEnumerable<TEntityDtoIn> batch in dataSource.BatchEx(maxBatchSize))
                {
                    currentBatch++;

                    if (_logger.IsDebugEnabled)
                        _logger.Debug($"Starting to process batch {currentBatch}.");

                    // create new state for each batch
                    loaderState.Valid = new TLoadStateData();

                    foreach (TEntityDtoIn dto in batch)
                    {
                        loaderState.CurrentRow++;

                        try
                        {
                            // always validate incoming data
                            // ReSharper disable once SuggestVarOrType_Elsewhere
                            var errors = rules.ToErrors(dto).ToCollection();
                            if (!errors.Any())
                                Mapper(loaderState, dto);
                            else
                                loaderState.Errors.Add(new RuleException(
                                    $"Can't process row {loaderState.CurrentRow} due to one or more validation errors. Please see the log for more details.",
                                    errors));
                        }
                        catch (RuleException rex)
                        {
                            if (_logger.IsErrorEnabled)
                                _logger.Error(
                                    $"Can't process row {loaderState.CurrentRow} due to one or more errors. Please see the log for more details.");

                            foreach (Error error in rex.Errors)
                                _logger.Error(error.Message);

                            exceptions.Add(rex);
                        }
                        catch (ApplicationException apex)
                        {
                            if (_logger.IsErrorEnabled)
                                _logger.Error(apex, $"Can't process row {loaderState.CurrentRow} due to error: {apex.Message}");

                            exceptions.Add(apex);
                        }
                        catch (Exception ex)
                        {
                            if (_logger.IsErrorEnabled)
                                _logger.Error(ex, $"Can't process row {loaderState.CurrentRow} due to an unexpected error.");

                            exceptions.Add(ex);
                        }
                    }

                    if (_logger.IsDebugEnabled)
                        _logger.Debug($"Batch {currentBatch} completed.");

                    if (_logger.IsDebugEnabled)
                        _logger.Debug("Committing changes.");

                    // update target
                    Commit(loaderState);

                    if (_logger.IsDebugEnabled)
                        _logger.Debug("Committed changes.");
                }
            }
            finally
            {
                loaderState.EndTime = DateTime.UtcNow;

                // remove progress
                if (_logger.IsInfoEnabled)
                    _logger.Info(loaderState);
            }

            return loaderState;
        }
    }
}
