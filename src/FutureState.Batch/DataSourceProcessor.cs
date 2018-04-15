using System;
using System.Collections.Generic;
using System.Linq;

using FutureState.Specifications;
using NLog;

namespace FutureState.Batch
{
    /// <summary>
    ///     Controls how data is loaded from an incoming stream and validated.
    /// </summary>
    /// <typeparam name="TEntityDtoIn">The data type of the entity to read in.</typeparam>
    public class DataSourceProcessor<TEntityDtoIn, TLoadStateData>
        where TLoadStateData : new()
    {
        /// <summary>
        ///     Gets the default max batch size;
        /// </summary>
        public const int DefaultMaxBatchSize = 10000;

        private readonly IProvideSpecifications<TEntityDtoIn> _validator;

        private bool _isInitialized;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="validator">
        ///     Optional validator to use. Null will use 'SpecProvider'
        /// </param>
        /// <param name="log">The log to post event to.</param>
        public DataSourceProcessor(IProvideSpecifications<TEntityDtoIn> validator = null, ILoaderLogWriter log = null)
        {
            _validator = validator ?? new SpecProvider<TEntityDtoIn>();

            Log = Log ?? new LoaderLogWriter(LogManager.GetLogger(GetLogSystemName()));
        }

        public static string GetLogSystemName()
        {
            return $"Loader-{typeof(TEntityDtoIn).Name}";
        }

        /// <summary>
        ///     Gets the function to get the entities to read in and process.
        /// </summary>
        public IEnumerable<TEntityDtoIn> EntitiesGet { get; set; }

        /// <summary>
        ///     Gets the function to map and process entities to load state from the incoming datastore. 
        /// </summary>
        public Action<LoaderState<TLoadStateData>, TEntityDtoIn> Processor { get; set; }

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
        ///     Get/sets the log to write errors and warnings to.
        /// </summary>
        public ILoaderLogWriter Log { get; }

        /// <summary>
        ///     Loads data from the incoming data stream, controls validating of incoming objects and calls commit when all valid
        ///     objects have been loaded.
        /// </summary>
        /// <returns>The load process result state.</returns>
        public ILoaderState Process()
        {
            if (Processor == null)
                throw new InvalidOperationException("Mapper action has not been assigned.");

            if (Commit == null)
                throw new InvalidOperationException("Commit action has not been assigned.");

            // start time will be assigned
            var loaderState = new LoaderState<TLoadStateData>();

            CurrentLoaderState = loaderState;

            // ReSharper disable once SuggestVarOrType_Elsewhere
            var exceptions = loaderState.ErrorsCount;

            try
            {
                loaderState.CurrentRow = 0;

                // the rules to validate incoming data
                // ReSharper disable once SuggestVarOrType_Elsewhere
                ISpecification<TEntityDtoIn>[] rules = _validator.GetSpecifications().ToArray();

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
                    Log.Warn(
                            $"Fixing up max batch size as the supplied value is less than 1. Current value is {MaxBatchSize} which is being set to {DefaultMaxBatchSize}.");

                    maxBatchSize = DefaultMaxBatchSize;
                }

                Log.Info($"Processing {typeof(TEntityDtoIn).Name} in batches of {maxBatchSize}.");

                // split stream into batches of 4
                foreach (IEnumerable<TEntityDtoIn> batch in dataSource.BatchEx(maxBatchSize))
                {
                    currentBatch++;

                    // log the number of batches to enhance testability
                    loaderState.Batches = currentBatch;

                    Log.Info($"Starting to process batch {currentBatch}.");

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
                            {
                                // map + load valid state
                                Processor(loaderState, dto);
                            }
                            else
                            {
                                this.Log.Error(new RuleException(
                                    $"Can't process row {loaderState.CurrentRow} due to one or more validation errors. Please see the log for more details.",
                                    errors));

                                loaderState.ErrorsCount++;
                            }
                        }
                        catch (RuleException rex)
                        {
                            Log.Error(
                                $"Can't process row {loaderState.CurrentRow} due to one or more errors. Please see the log for more details.");

                            foreach (Error error in rex.Errors)
                                Log.Error(error.Message);

                            loaderState.ErrorsCount++;
                        }
                        catch (ApplicationException apex)
                        {
                            Log.Error(apex, $"Can't process row {loaderState.CurrentRow} due to error: {apex.Message}");

                            loaderState.ErrorsCount++;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Can't process row {loaderState.CurrentRow} due to an unexpected error.");

                            loaderState.ErrorsCount++;
                        }
                    }

                    Log.Info($"Batch {currentBatch} completed.");

                    Log.Info("Committing changes.");

                    // update target
                    Commit(loaderState);

                    Log.Info("Committed changes.");
                }
            }
            finally
            {
                loaderState.EndTime = DateTime.UtcNow;

                Log.Info(loaderState.ToString());
            }

            return loaderState;
        }
    }
}
