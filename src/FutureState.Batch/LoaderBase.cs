#region

// Copyright 2007-2010 The Apache Software Foundation.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.

// ported from FutureState.Batch library
// https://github.com/arisanikolaou/FutureState.Batch

#endregion

#region

using System;
using System.Collections.Generic;

using EmitMapper;
using FutureState.Specifications;
using NLog;

#endregion

namespace FutureState.Batch
{
    /// <summary>
    ///     Base class for all loaders processing incoming entities or dtos to outgoing entities.
    /// </summary>
    public class LoaderBase
    {
        /// <summary>
        ///     Gets the unique schema type code of the data being read in.
        /// </summary>
        public string LoaderTypeCode { get; protected set; }

        /// <summary>
        ///     Gets/sets the data source (e.g. a file to load from).
        /// </summary>
        public string DataSource { get; set; }
    }

    /// <summary>
    ///     Loads a given <see cref="TDtoOut" /> entity by mapping from a given <see cref="TDtoIn" /> data source read
    ///     from a given data stream.
    /// </summary>
    /// <remarks>
    ///     Uses a csv loader to read entities from a given data source by default.
    /// </remarks>
    public abstract class LoaderBase<TDtoIn, TDtoOut>
        : LoaderBaseWithState<TDtoIn, List<TDtoOut>>
        where TDtoOut : class, new()
    {
        private static readonly ObjectsMapper<TDtoIn, TDtoOut> _defaultMapper;

        // static constructor
        static LoaderBase()
        {
            _defaultMapper = ObjectMapperManager.DefaultInstance.GetMapper<TDtoIn, TDtoOut>();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="loaderTypeCode">The unique schema or loader identifier.</param>
        /// <param name="validator">Provides the rules to validate incoming dtos.</param>
        /// <param name="extractor">The data source reader.</param>
        protected LoaderBase(
            IExtractor<TDtoIn> extractor = null,
            IProvideSpecifications<TDtoIn> validator = null,
            string loaderTypeCode = null)
            : base(extractor, validator, loaderTypeCode ?? GetLoaderTypeCode())
        {

        }

        public static string GetLoaderTypeCode()
        {
            return $"Loader-{typeof(TDtoIn).Name}-{typeof(TDtoOut).Name}";
        }

        /// <summary>
        ///     Map data from an incoming entity to an outgoing entity and ensure the validated outgoing entity is
        ///     added to the loadstate's valid item collection.
        /// </summary>
        /// <remarks>
        ///     By default will use an object mapping manager to map dto in to and outgoing dto.
        /// </remarks>
        /// <param name="dtoIn">The source entity to map to the target entity.</param>
        /// <param name="loadState">The state to load validated target entities.</param>
        protected override void Process(TDtoIn dtoIn, LoaderState<List<TDtoOut>> loadState)
        {
            var outDto = new TDtoOut();

            try
            {
                _defaultMapper.Map(dtoIn, outDto);
            }
            catch (Exception ex)
            {
                // don't fail the loader process
                if (_logger.IsErrorEnabled)
                    _logger.Error(ex, $"Failed to map {dtoIn} to {outDto} due to an unexpected error.");
            }

            // maps
            Process(dtoIn, loadState, outDto);
        }

        /// <summary>
        ///     Map data from an incoming entity to an outoing entity and ensure the validated outgoing entity is
        ///     added to the loadstate's valid item collection.
        /// </summary>
        /// <remarks>
        ///     By default will use an object mapping manager to map dto in to and outgoing dto.
        /// </remarks>
        /// <param name="dtoIn">The source entity to map to the target entity.</param>
        /// <param name="loadState">The state to load validated target entities.</param>
        /// <param name="dtoOutDefaultMapped">
        ///     The pre-mapped target entity to complete mapping, validating and adding to the target
        ///     load state.
        /// </param>
        protected abstract void Process(TDtoIn dtoIn, LoaderState<List<TDtoOut>> loadState, TDtoOut dtoOutDefaultMapped);
    }

    /// <summary>
    ///     Loads data from a given <see cref="TDtoIn" /> data source from a given stream.
    /// </summary>
    /// <remarks>
    ///     Uses a csv reader to stream in data by default.
    /// </remarks>
    public abstract class LoaderBaseWithState<TDtoIn, TLoadStateData> : LoaderBase, ILoader
        where TLoadStateData : new()
    {
        private readonly IProvideSpecifications<TDtoIn> _validator;
        private readonly IExtractor<TDtoIn> _extractor;
        protected readonly Logger _logger;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="loaderTypeCode">The type of entity that is being loaded into the system.</param>
        /// <param name="validator">Validates the incoming dtos.</param>
        /// <param name="extractor">The underlying extrator for the incoming dtos.</param>
        protected LoaderBaseWithState(
            IExtractor<TDtoIn> extractor = null,
            IProvideSpecifications<TDtoIn> validator = null,
            string loaderTypeCode = null)
        {
            LoaderTypeCode = loaderTypeCode ?? typeof(TLoadStateData).Name;

            _validator = validator;
            _logger = LogManager.GetLogger(loaderTypeCode);

            // create default extractor function
            if (extractor == null)
                extractor = new CsvExtractor<TDtoIn>();

            _extractor = extractor;
        }

        /// <summary>
        ///     Gets the maximum batch size to read/write data sets.
        /// </summary>
        public int MaxBatchSize { get; set; } = 1000;

        /// <summary>
        ///     Get/sets the log to write errors and warnings to.
        /// </summary>
        public ILoaderLogWriter Log { get; set; }

        /// <summary>
        ///     Start the load process.
        /// </summary>
        ILoaderState ILoader.Load()
        {
            return Load();
        }

        /// <summary>
        ///     Gets the underlying extractor to use.
        /// </summary>
        /// <remarks>
        ///     CsvExtractor is used by default.
        /// </remarks>
        /// <returns></returns>
        protected virtual IEnumerable<TDtoIn> GetDtos()
        {
            // default extractor
            _extractor.Uri = DataSource;

            return _extractor.Read();
        }

        /// <summary>
        ///     Loads the entities and produces a load result.
        /// </summary>
        public ILoaderState Load()
        {
            ILoaderLogWriter log = Log ?? new LoaderLogWriter(_logger);

            try
            {
                var dataSourceProcessor = new DataSourceProcessor<TDtoIn, TLoadStateData>(_validator, log)
                {
                    EntitiesGet = GetDtos(),
                    Initialize = Initialize,
                    MaxBatchSize = MaxBatchSize,
                    Processor = (loaderState, dtoIn) => Process(dtoIn, loaderState),
                    Commit = Commit
                };

                ILoaderState result = dataSourceProcessor.Process();

                return result;
            }
            finally
            {
                OnLoaded();
            }
        }

        /// <summary>
        ///     Map data from an incoming entity to an outgoing entity and ensure the validated outgoing entity is
        ///     added to the loadstate's valid item collection.
        /// </summary>
        /// <remarks>
        ///     By default will use an object mapping manager to map dto in to and outgoing dto.
        /// </remarks>
        /// <param name="dtoIn">The source entity to map to the target entity.</param>
        /// <param name="loadState">The state to load validated target entities.</param>
        protected abstract void Process(TDtoIn dtoIn, LoaderState<TLoadStateData> loadState);

        /// <summary>
        ///     Called after Load regardless of any errors processing incoming data.
        /// </summary>
        protected virtual void OnLoaded()
        {
        }

        /// <summary>
        ///     Initialize any caches or lookup tables.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        ///     Commit valid entities to the underlying data source and update the added,updated and removed count.
        /// </summary>
        /// <param name="loadState"></param>
        protected abstract void Commit(LoaderState<TLoadStateData> loadState);
    }
}
