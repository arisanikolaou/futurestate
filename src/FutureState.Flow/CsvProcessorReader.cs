using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace FutureState.Flow
{
    /// <summary>
    ///     Reads entities from a csv file into a stream.
    /// </summary>
    public class CsvProcessorReader<TEntity> : IReader<TEntity>
    {
        private readonly string _fileName;

        public CsvProcessorReader(string fileName)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(fileName, nameof(fileName));

            _fileName = fileName;
        }

        public IEnumerable<TEntity> Read()
        {
            if (!File.Exists(_fileName))
                throw new InvalidOperationException($"File {_fileName} does not exist.");

            var config = new Configuration {HasHeaderRecord = true};

            using (var reader = new StreamReader(File.OpenRead(_fileName)))
            {
                using (var helper = new CsvReader(reader, config))
                {
                    if (helper.Read())
                        try
                        {
                            if (!helper.ReadHeader())
                                throw new ApplicationException($"Can't read header of file {_fileName}.");
                        }
                        catch (ApplicationException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"Can't read data from file {_fileName}.", ex);
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