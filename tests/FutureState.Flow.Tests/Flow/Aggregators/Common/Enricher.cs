using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using EmitMapper;
using NLog;

namespace FutureState.Flow.Tests.Aggregators
{
    public class CsvEnricherBuilder<TPart, TComposite>
        where TPart : IEquatable<TComposite>
    {
        public string FileName { get; set; }

        public Enricher<TPart, TComposite> Get()
        {
            return new Enricher<TPart, TComposite>(Read);
        }

        // read from a csv file
        public IEnumerable<TPart> Read()
        {
            var config = new Configuration { HasHeaderRecord = true };

            using (var sr = new StreamReader(FileName))
            {
                using (var csvHelper = new CsvReader(sr, config))
                {
                    if (csvHelper.Read())
                    {
                        try
                        {
                            if (!csvHelper.ReadHeader())
                                throw new ApplicationException($"Can't read header of file {FileName}.");
                        }
                        catch (ApplicationException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"Can't read data from file {FileName}.", ex);
                        }
                    }

                    while (csvHelper.Read())
                    {
                        yield return csvHelper.GetRecord<TPart>();
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Enriches data from a given part entity type to a whole (target) entity tpe.
    /// </summary>
    /// <typeparam name="TPart"></typeparam>
    /// <typeparam name="TComposite"></typeparam>
    public class Enricher<TPart, TComposite> : IEnricher<TComposite>
        where TPart : IEquatable<TComposite>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly ObjectsMapper<TPart, TComposite> _mapper;
        private readonly Func<IEnumerable<TPart>> _source;

        /// <summary>
        ///     Action to use to enrich data from a part to a whole.
        /// </summary>
        public Action<TPart, TComposite> EnrichAction { get; set; }

        /// <summary>
        ///     Gets the unique id for the enricher.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        ///     Geta a handle to the default mapper.
        /// </summary>
        static Enricher()
        {
            _mapper = ObjectMapperManager
                .DefaultInstance
                .GetMapper<TPart, TComposite>();
        }

        public Enricher(Func<IEnumerable<TPart>> sourceGet)
        {
            Guard.ArgumentNotNull(sourceGet, nameof(sourceGet));

            // source data to enrich from
            this._source = sourceGet;

            this.UniqueId = GetType().Name;
        }

        public virtual TComposite Enrich(TPart part, TComposite whole)
        {
            // copy objects from source to targe
            var map = _mapper.Map(part, whole);

            EnrichAction?.Invoke(part, map);

            return map;
        }

        public virtual TComposite Enrich(IEquatable<TComposite> part, TComposite whole)
        {
            return _mapper.Map((TPart)part, whole);
        }

        public IEnumerable<IEquatable<TComposite>> Get()
        {
            // gets all the items in the source
            foreach (var item in _source())
                yield return item;
        }

        public virtual IEnumerable<IEquatable<TComposite>> Find(TComposite composite)
        {
            foreach (var equatable in Get())
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (equatable.Equals(composite))
                    yield return equatable;
        }
    }
}