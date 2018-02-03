using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace FutureState.Flow
{
    /// <summary>
    ///     Reads entities from a csv file into a stream.
    /// </summary>
    public class CsvProcessorReader<TEntity> : IReader<TEntity>
    {
        public IEnumerable<TEntity> Read(string fileName)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(fileName, nameof(fileName));

            if (!File.Exists(fileName))
                throw new InvalidOperationException($"File {fileName} does not exist.");

            var config = new Configuration { HasHeaderRecord = true };

            using (var reader = new StreamReader(File.OpenRead(fileName)))
            {
                using (var helper = new CsvReader(reader, config))
                {
                    if (helper.Read())
                        try
                        {
                            if (!helper.ReadHeader())
                                throw new ApplicationException($"Can't read header of file {fileName}.");
                        }
                        catch (ApplicationException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"Can't read data from file {fileName}.", ex);
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