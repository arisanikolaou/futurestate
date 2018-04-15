using System;
using System.Collections.Generic;
using System.IO;

using CsvHelper;
using NLog;

namespace FutureState.Batch
{
    /// <summary>
    ///     Materializes a stream of <see cref="TDtoIn" /> from a given csv file that has headers.
    /// </summary>
    /// <remarks>
    ///     Csv files must contain a header.
    /// </remarks>
    /// <typeparam name="TDtoIn">
    ///     The entity to read in from the underlying data store.
    /// </typeparam>
    public class CsvExtractor<TDtoIn> : IExtractor<TDtoIn>
    {
        public ILoaderLogWriter Logger { get; set; }

        /// <summary>
        ///     Gets/sets the file path to read the csv file from.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        ///     Reads to a given entity type from a csv file.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TDtoIn> Read()
        {
            var log = this.Logger ?? new LoaderLogWriter(LogManager.GetCurrentClassLogger());

            using (var reader = new StreamReader(File.OpenRead(Uri)))
            {
                using (var helper = new CsvReader(reader))
                {
                    helper.Read();
                    helper.ReadHeader();

                    try
                    {
                        if (helper.Context.HeaderRecord == null)
                            throw new InvalidOperationException($"Can't read header of file {this.Uri}.");
                    }
                    catch (InvalidOperationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Can't read data from file {this.Uri}.", ex);
                    }

                    long rowNumber = 0;

                    // read entities
                    while (helper.Read())
                    {
                        rowNumber++;

                        TDtoIn entityDto = default(TDtoIn);
                        try
                        {
                            // may contain unmappable objects
                            entityDto = helper.GetRecord<TDtoIn>();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, $"Error parsing row {rowNumber}.");

                            continue;
                        }

                        yield return entityDto;
                    }
                }
            }
        }
    }
}
