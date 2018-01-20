using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace FutureState.Flow.Core
{
    public class CsvProcesorReader<TEntity> : IReader<TEntity>
    {
        private readonly string _fileName;

        public CsvProcesorReader(string fileName)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(fileName, nameof(fileName));

            _fileName = fileName;
        }

        public IEnumerable<TEntity> Read()
        {
            using (var reader = new StreamReader(File.OpenRead(_fileName)))
            {
                using (var helper = new CsvReader(reader))
                {
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
