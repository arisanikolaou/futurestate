using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using EmitMapper;
using NLog;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     Enrich a given target type from a Csv source file.
    /// </summary>
    /// <typeparam name="TPart"></typeparam>
    /// <typeparam name="TComposite"></typeparam>
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
    ///     Enriches data from a given 'part' entity type to a whole (target) entity tpe.
    /// </summary>
    /// <typeparam name="TPart">The entity type used to enrich the target composite type.</typeparam>
    /// <typeparam name="TComposite">The composite type being enriched.</typeparam>
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
        public string OutputTypeId { get; set; }

        /// <summary>
        ///     Geta a handle to the default mapper.
        /// </summary>
        static Enricher()
        {
            _mapper = ObjectMapperManager
                .DefaultInstance
                .GetMapper<TPart, TComposite>();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="sourceGet"></param>
        public Enricher(Func<IEnumerable<TPart>> sourceGet)
        {
            Guard.ArgumentNotNull(sourceGet, nameof(sourceGet));

            // source data to enrich from
            this._source = sourceGet;

            // the unique id of the enricher
            this.OutputTypeId = typeof(TComposite).Name;
        }
        
        /// <summary>
        ///     Enriches the whole from a given part.
        /// </summary>
        /// <param name="part">The data source used to enrich the whole composite data type.</param>
        /// <param name="whole">The whole object to enrich.</param>
        /// <returns></returns>
        public virtual TComposite Enrich(IEquatable<TComposite> part, TComposite whole)
        {
            return _mapper.Map((TPart)part, whole);
        }

        /// <summary>
        ///     Gets a set of composite objects.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IEquatable<TComposite>> Get()
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