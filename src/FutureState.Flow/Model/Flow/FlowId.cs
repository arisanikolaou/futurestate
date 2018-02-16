using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    /// <summary>
    ///     Service to create/remove flows and flow batches.
    /// </summary>
    public class FlowService
    {
        private readonly FlowIdRepo _repo;

        public FlowService(FlowIdRepo repo)
        {
            _repo = repo;
        }

        /// <summary>
        ///     Creates a new flow flow.
        /// </summary>
        /// <param name="flowCode">The unique flow code.</param>
        /// <returns></returns>
        public FlowId CreateNew(string flowCode)
        {
            var flow = new FlowId()
            {
                Code = flowCode,
                DataDir = Environment.CurrentDirectory,
                DisplayName = flowCode,
                Entities = new List<FlowEntity>(),
                CurrentBatchId = 1
            };

            Save(flow);

            return flow;
        }

        public FlowId Get(string flowCode)
        {
            return _repo.Get(flowCode);
        }

        public void Save(FlowId flow)
        {
            if (_repo.Exists(flow.Code))
                throw new InvalidOperationException($"Another flow with the code {flow.Code} already exists.");

            _repo.Save(flow);
        }

        public FlowEntity RegisterEntity<TEntityType>(string flowCode)
        {
            var flow = Get(flowCode);

            string entityTypeId = typeof(TEntityType).Name;

            if (flow.Entities.Any(m => string.Equals(m.EntityTypeId, entityTypeId)))
                throw new InvalidOperationException($"The entity type has already been registered.");

            var entity = new FlowEntity()
            {
                AssemblyQualifiedTypeName = typeof(TEntityType).AssemblyQualifiedName,
                DateAdded = DateTime.UtcNow,
                EntityTypeId = entityTypeId
            };

            // add to collection
            flow.Entities.Add(entity);

            // update
            _repo.Save(flow);

            //
            return entity;
        }

        public void RegisterEntity(string flowCode, FlowEntity entity)
        {
            var flow = Get(flowCode);

            if (flow.Entities.Any(m => string.Equals(m.EntityTypeId, entity.EntityTypeId)))
                throw new InvalidOperationException($"The entity type has already been registered.");

            flow.Entities.Add(entity);

            // record data added
            entity.DateAdded = DateTime.UtcNow;

            // update
            _repo.Save(flow);
        }

        public FlowBatch GetNewFlowBatch(string flowCode)
        {
            var flow = Get(flowCode);
            if (flow == null)
                flow = new FlowId(flowCode); // just create new flow code

            var newBatchId = flow.CurrentBatchId++;

            var batch = new FlowBatch()
            {
                Flow = flow,
                BatchId = newBatchId,
            };

            // save/update original file - don't call save as it will through error
            _repo.Save(flow);

            // now return to caller
            return batch;
        }
    }
}