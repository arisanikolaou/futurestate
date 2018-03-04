using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace FutureState.Flow
{
    /// <summary>
    ///     A flow is a well known data flow from a given distinct primary source to a set of extensible target
    ///     data stores (flow files).
    /// </summary>
    public class FlowId
    {
        /// <summary>
        ///     Creates a new flow id.
        /// </summary>
        public FlowId()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="code">Thee code.</param>
        public FlowId(string code)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(code, nameof(code));

            this.Entities = new List<FlowEntity>();
            this.Code = code;
            this.CurrentBatchId = 1;
        }

        /// <summary>
        ///     Gets the base data directory for related data sets.
        /// </summary>
        public string DataDir { get; set; }

        /// <summary>
        ///     Gets the flow code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        ///     Gets the flow's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets the last assigned batch id.
        /// </summary>
        public long CurrentBatchId { get; set; }

        /// <summary>
        ///     Gets the list of registered entities in the flow.
        /// </summary>
        public List<FlowEntity> Entities { get; set; }

        /// <summary>
        ///     Gets the flow code.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Code;
        }

        /// <summary>
        ///     Creates a new flow id from a given flow code.
        /// </summary>
        public static implicit operator FlowId(string code)
        {
            return new FlowId(code);
        }
    }

    /// <summary>
    ///     Gets the repository to load/save flow files.
    /// </summary>
    public class FlowIdRepo
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets the data directory to load from.
        /// </summary>
        public string DataDir { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowIdRepo()
        {
            this.DataDir = Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets the flow code to load from the file system.
        /// </summary>
        /// <param name="flowCode">
        ///     The flow code to load.
        /// </param>
        /// <returns>
        ///     The flow.
        /// </returns>
        public FlowId Get(string flowCode)
        {
            // source id would be the name of a processor

            var fileName =
                $@"{DataDir}\Flow-{flowCode}.json";

            if (!File.Exists(fileName))
                return null;

            var content = File.ReadAllText(fileName);

            var log = JsonConvert.DeserializeObject<FlowId>(
                content,
                new JsonSerializerSettings());

            return log;
        }

        /// <summary>
        ///     Saves/updates a given flow.
        /// </summary>
        /// <param name="flow">The flow to save.</param>
        public void Save(FlowId flow)
        {
            Guard.ArgumentNotNull(flow, nameof(flow));

            CreateDirIfNotExists();

            // source log
            var fileName =
                $@"{DataDir}\Flow-{flow.Code}.json";

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving flow output to {fileName}.");

            // save the data in the data directory
            var body = JsonConvert.SerializeObject(flow, new JsonSerializerSettings());

            if (File.Exists(fileName))
            {
                // back up older file, don't delete
                string backFile = fileName + ".bak";
                if (File.Exists(backFile))
                    File.Delete(backFile);

                File.Move(fileName, backFile);
            }

            // wrap in transaction
            File.WriteAllText(fileName, body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved flow file to {fileName}.");
        }

        private void CreateDirIfNotExists()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
        }

        /// <summary>
        ///     Gets whether a flow with a given code already exists.
        /// </summary>
        /// <param name="code">The flow code.</param>
        /// <returns></returns>
        public bool Exists(string code)
        {
            // source log
            var fileName =
                $@"{DataDir}\Flow-{code}.json";

            return File.Exists(fileName);
        }
    }
}