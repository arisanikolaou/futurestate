using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow
{
    /// <summary>
    ///     Extracts entities from a given data sources in a managed way to use in
    ///     data processing.
    /// </summary>
    /// <typeparam name="TEntityDto">The type of entity to process.</typeparam>
    public class ProcessorEngine<TEntityDto> : IProcessorEngine
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorEngine()
        {
            Warnings = new List<string>();
        }

        /// <summary>
        ///     Initializes a new procesor.
        /// </summary>
        public Action Initialize { get; set; }

        /// <summary>
        ///     Gets the action to read results from.
        /// </summary>
        public IEnumerable<TEntityDto> EntitiesReader { get; set; }

        /// <summary>
        ///     Gets the action to process one item.
        /// </summary>
        public Func<TEntityDto, IEnumerable<ErrorEvent>> ProcessItem { get; set; }

        /// <summary>
        ///     Gets the error handler.
        /// </summary>
        public Action<TEntityDto, Exception> OnError { get; set; }

        /// <summary>
        ///     Gets the list of warnings accumulated.
        /// </summary>
        public List<string> Warnings { get; }

        /// <summary>
        ///     Action to execute when finished processing.
        /// </summary>
        public Action Commit { get; set; }

        /// <summary>
        ///     Gets the current entity or row count being procesed.
        /// </summary>
        public int Current { get; private set; }

        /// <summary>
        ///     Gets the date the process started.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        ///     Gets the default processor name.
        /// </summary>
        /// <returns></returns>
        private string GetDefaultProcessName()
        {
            return $"{GetType().Name.Replace("`1", "")}-{typeof(TEntityDto).Name}";
        }

        /// <summary>
        ///     Processes all  data from the incoming source and records. Will record to file and memory the entities that were and
        ///     were not processed and returns
        ///     a summary of the processes' execution status.
        /// </summary>
        /// <returns></returns>
        public FlowSnapshot Process<TOut>(FlowBatch process, FlowSnapShot<TOut> result = null)
        {
            Guard.ArgumentNotNull(process, nameof(process));

            // record processor time
            StartTime = DateTime.UtcNow;

            Current = 0;

            if (EntitiesReader == null)
                throw new InvalidOperationException("EntitiesReader has not been assigned.");

            if (result == null)
                result = new FlowSnapShot<TOut>
                {
                    ProcessName = GetDefaultProcessName()
                };

            result.Batch = process;

            Initialize?.Invoke();

            var onError = OnError ?? ((_, ___) => { });

            var processed = new List<TEntityDto>();
            var errors = new List<ErrorEvent>();
            var exceptions = new List<Exception>();

            foreach (var dto in EntitiesReader)
            {
                Current++;

                try
                {
                    var errorsEvents = ProcessItem(dto);

                    var errorEvents = errorsEvents as ErrorEvent[] ?? errorsEvents.ToArray();
                    if (!errorEvents.Any())
                        processed.Add(dto);
                    else
                        foreach (var error in errorEvents)
                            errors.Add(error);
                }
                catch (ApplicationException apex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(apex, $"Can't process row {Current} due to error: {apex.Message}");

                    onError(dto, apex);

                    exceptions.Add(apex);

                    errors.Add(new ErrorEvent { Type = "Exception", Message = apex.Message });
                }
                catch (Exception ex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(ex, $"Can't process entity at index {Current} due to an unexpected error.");

                    onError(dto, ex);

                    exceptions.Add(ex);

                    errors.Add(new ErrorEvent { Type = "Exception", Message = ex.Message });
                }
            }

            // update target
            try
            {
                Commit();
            }
            catch (Exception ex)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(ex, "An unexpected error occurred commiting the processed result.");

                // roll back items into an error state
                foreach (var entityDto in processed)
                    errors.Add(new ErrorEvent { Type = "Exception", Message = $"Failed to commit changes: {ex.Message}" });

                //reset items
                processed.Clear();
            }

            // added/updated
            if (Logger.IsInfoEnabled)
                Logger.Info($"Finised processing.");

            result.ProcessedCount = processed.Count;
            result.Errors = errors;
#if DEBUG
            result.Exceptions = exceptions;
#endif
            result.Warnings = Warnings;
            result.ProcessTime = DateTime.UtcNow - StartTime;

            return result;
        }
    }
}