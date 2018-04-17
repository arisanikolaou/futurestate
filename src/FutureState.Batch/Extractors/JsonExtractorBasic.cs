using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FutureState.Batch
{
    public class JsonExtractor<TDtoIn> : IExtractor<TDtoIn>
    {
        public string Uri { get; set; }

        public IEnumerable<TDtoIn> Read()
        {
            var serializer = new JsonSerializer();

            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(Uri))
                return (TDtoIn[])serializer.Deserialize(file, typeof(TDtoIn[]));
            
        }
    }
}
