using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FutureState.Batch.Tests
{
    public class Class1
    {
        [Fact]
        public void BuildTest()
        {
            string dataSource = @"C:\Src\futurestate\tests\FutureState.Batch.Tests\Input\Datasource.json";

            // movies
            var data = new List<Contact>();

            for (int i = 0; i < 8; i++)
            {
                data.Add(new Contact()
                {
                    FirstName = "firstname " + i,
                    LastName = "lastname " + i,
                    DateOfBirth = DateTime.UtcNow
                });
            }

            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(dataSource))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, data.ToArray());
            }
        }

        // contact
        public class Contact
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime DateOfBirth { get; set; }
        }
    }
}
