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
    public class Flow
    {
        public Flow()
        {
        }

        public Flow(string code)
        {
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
    public class FlowRepo
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string DataDir { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowRepo()
        {
            this.DataDir = Environment.CurrentDirectory;
        }

        public Flow Get(string flowCode)
        {
            // source id would be the name of a processor

            var fileName =
                $@"{DataDir}\Flow-{flowCode}.json";

            if (!File.Exists(fileName))
                return null;

            var content = File.ReadAllText(fileName);

            var log = JsonConvert.DeserializeObject<Flow>(
                content,
                new JsonSerializerSettings());

            return log;
        }

        public void Save(Flow flow)
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
    }

    /// <summary>
    ///     Service to create/remove flows and flow batches.
    /// </summary>
    public class FlowService
    {
        private readonly FlowRepo _repo;

        public FlowService(FlowRepo repo)
        {
            _repo = repo;
        }

        public Flow CreateNew(string flowCode)
        {
            var flow = new Flow()
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

        public Flow Get(string flowCode)
        {
            return _repo.Get(flowCode);
        }

        public void Save(Flow flow)
        {
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
            Save(flow);

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
            Save(flow);
        }

        public FlowBatch GetNewBatchProcess(string flowCode)
        {
            var flow = Get(flowCode);
            if (flow == null)
                throw new InvalidOperationException($"Can't find flow with the code {flowCode}.");

            var newBatchId = flow.CurrentBatchId++;

            var batch = new FlowBatch()
            {
                Flow = flow,
                BatchId = newBatchId,
            };

            // save/update original file
            Save(flow);

            // now return to caller
            return batch;
        }
    }

    /// <summary>
    ///     A well known entity processed within a given flow.
    /// </summary>
    public class FlowEntity
    {
        public FlowEntity(Type type)
        {
            this.AssemblyQualifiedTypeName = type.AssemblyQualifiedName;
            this.DateAdded = DateTime.UtcNow;
            this.EntityTypeId = type.Name;
        }

        public FlowEntity()
        {
        }

        /// <summary>
        ///     Gets the assembly qualified flow entity. This is the type name of the material form of the entity.
        /// </summary>
        public string AssemblyQualifiedTypeName { get; set; }

        /// <summary>
        ///     Gets the date, in UTC, the flow entity was added.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the entity type id. This id must be unique within a given flow.
        /// </summary>
        public string EntityTypeId { get; set; }
    }
}