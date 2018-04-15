using System;
using System.Collections.Generic;
using System.IO;

using CsvHelper;

namespace FutureState.Batch
{
    /// <summary>
    ///     Materializes a stream of <see cref="TEntity" /> from a given csv file that has headers.
    /// </summary>
    /// <remarks>
    ///     Csv files must contain a header.
    /// </remarks>
    /// <typeparam name="TEntity"></typeparam>
    public class CsvExtractor<TEntity> : IExtractor<TEntity>
    {
        private readonly string _fileName;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="filePath">The csv file path to load with headers.</param>
        public CsvExtractor(string filePath)
        {
            Guard.ArgumentNotNullOrEmpty(filePath, nameof(filePath));

            _fileName = filePath;
        }

        /// <summary>
        ///     Reads to a given entity type from a csv file.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> Read()
        {
            using (var reader = new StreamReader(File.OpenRead(_fileName)))
            {
                using (var helper = new CsvReader(reader))
                {
                    helper.Read();
                    helper.ReadHeader();

                    try
                    {
                        if (helper.Context.HeaderRecord == null)
                            throw new InvalidOperationException($"Can't read header of file {_fileName}.");
                    }
                    catch (InvalidOperationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Can't read data from file {_fileName}.", ex);
                    }

                    while (helper.Read())
                    {
                        var entityDto = helper.GetRecord<TEntity>();

                        yield return entityDto;
                    }
                }
            }
        }
    }
}
