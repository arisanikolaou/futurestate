using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace FutureState.Flow.Core
{
    public class ProcessorHandler<TEntityDto> : IProcessorHandler
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorHandler(
            string processorName = null,
            Guid? correlationId = null,
            int batchId = 1)
        {
            StartTime = DateTime.UtcNow;

            CorrelationId = correlationId ?? SeqGuid.Create();
            BatchId = batchId;
            Warnings = new List<string>();
            Errors = new List<ProcessError<TEntityDto>>();
            Processed = new List<TEntityDto>();
            ProcessName = processorName ?? GetType().Name;
            WorkingFolder = Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string WorkingFolder { get; set; }

        /// <summary>
        ///     Gets/set the processor name.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        ///     Gets the process correlation id.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     Gets the job/batch number.
        /// </summary>
        public int BatchId { get; set; }

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
        public Func<TEntityDto, ErrorEvent> ProcessItem { get; set; }

        /// <summary>
        ///     Gets the error handler.
        /// </summary>
        public Action<TEntityDto, Exception> OnError { get; set; }

        /// <summary>
        ///     Gets the items that were successfully processed.
        /// </summary>
        public List<TEntityDto> Processed { get; private set; }

        /// <summary>
        ///     Gets the errors that were encountered processing the incoming entities.
        /// </summary>
        public List<ProcessError<TEntityDto>> Errors { get; private set; }

        /// <summary>
        ///     Gets the file containing the entities that were processed to file.
        /// </summary>
        public string ProcessedSnapshotFile { get; private set; }

        /// <summary>
        ///     Gets the file path to the entities that were not processed.
        /// </summary>
        public string ErrorSnapshotFile { get; private set; }

        /// <summary>
        ///     Gets the list of warnings accumulated.
        /// </summary>
        public List<string> Warnings { get; }

        /// <summary>
        ///     Gets the logger to use.
        /// </summary>
        public Logger Logger { get; set; }

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
        public DateTime StartTime { get; }

        /// <summary>
        ///     Processes all  data from the incoming source and records. Will record to file and memory the entities that were and
        ///     were not processed and returns
        ///     a summary of the processes' execution status.
        /// </summary>
        /// <returns></returns>
        public ProcessResult Process()
        {
            var loaderErrors = new List<Exception>();

            if (!Directory.Exists(WorkingFolder))
            {
                try
                {
                    Directory.CreateDirectory(WorkingFolder);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working exception {WorkingFolder}.", ex);
                }
                
            }

            Current = 0;

            Initialize?.Invoke();

            if (EntitiesReader == null)
                throw new InvalidOperationException("EntitiesReader has not been assigned.");

            var onError = OnError ?? ((_, ___) => { });

            var processed = new List<TEntityDto>();
            var errors = new List<ProcessError<TEntityDto>>();

            foreach (var dto in EntitiesReader)
            {
                Current++;

                try
                {
                    var error = ProcessItem(dto);

                    if (error == null)
                        processed.Add(dto);
                    else
                        errors.Add(new ProcessError<TEntityDto> {Error = error, Item = dto});
                }
                catch (ApplicationException apex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(apex, $"Can't process row {Current} due to error: {apex.Message}");

                    onError(dto, apex);

                    loaderErrors.Add(apex);

                    errors.Add(new ProcessError<TEntityDto>
                    {
                        Error = new ErrorEvent {Type = "Exception", Message = apex.Message},
                        Item = dto
                    });
                }
                catch (Exception ex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(ex, $"Can't process row {Current} due to an unexpected error.");

                    onError(dto, ex);

                    loaderErrors.Add(ex);

                    errors.Add(new ProcessError<TEntityDto>
                    {
                        Error = new ErrorEvent {Type = "Exception", Message = ex.Message},
                        Item = dto
                    });
                }
            }

            // update target
            try
            {
                Commit();
            }
            catch (Exception ex)
            {
                if(Logger.IsErrorEnabled)
                    Logger.Error(ex);

                // roll back
                foreach (var entityDto in processed)
                {
                    errors.Add(new ProcessError<TEntityDto>
                    {
                        Error = new ErrorEvent { Type = "Exception", Message = $"Failed to commit changes: {ex.Message}" },
                        Item = entityDto
                    });
                }

                processed.Clear();
            }


            // log errors to file system
            {
                var i = 1;
                var fileName = $@"{WorkingFolder}\{ProcessName}-Errors-{CorrelationId}-{BatchId}.json";
                while (File.Exists(fileName))
                    fileName = $@"{WorkingFolder}\{ProcessName}-Errors-{CorrelationId}-{BatchId}-{i++}.json";
                SaveSnapShot(fileName, errors);

                ErrorSnapshotFile = fileName;
                Errors = errors;
            }

            // log processed items to be able to roll back
            {
                var i = 1;
                var fileName = $@"{WorkingFolder}\{ProcessName}-OnFinishedProcessing-{CorrelationId}-{BatchId}.json";
                while (File.Exists(fileName))
                    fileName = $@"{WorkingFolder}\{ProcessName}-OnFinishedProcessing-{CorrelationId}-{BatchId}-{i++}.json";

                SaveSnapShot(fileName, processed);

                ProcessedSnapshotFile = fileName;
                Processed = processed;
            }

            // added/updated
            if (Logger.IsInfoEnabled)
                Logger.Info($"Finised processing.");

            return new ProcessResult
            {
                CorrelationId = CorrelationId,
                BatchId = BatchId,
                ProcessedCount = Current,
                Errors = loaderErrors,
                Warnings = Warnings,
                LoadTime = DateTime.UtcNow - StartTime
            };
        }

        // keep a log of the entities which errored out or were processed
        private void SaveSnapShot<T>(string fileName, List<T> data)
        {
            var log = new ProcessSnapshot<T>
            {
                CorrelationId = CorrelationId,
                Data = data
            };

            var body = JsonConvert.SerializeObject(log, new JsonSerializerSettings());
            File.WriteAllText(fileName, body);
        }

        public ProcessSnapshot<T> LoadSnapShot<T>(string fileName)
        {
            var body = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<ProcessSnapshot<T>>(body);
        }
    }

    public class ProcessError<TEntityDto>
    {
        public ErrorEvent Error { get; set; }

        public TEntityDto Item { get; set; }
    }

    public class ProcessSnapshot<TEntityDto>
    {
        public Guid CorrelationId { get; set; }

        public List<TEntityDto> Data { get; set; }
    }
}