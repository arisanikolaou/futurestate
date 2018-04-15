using FutureState.Data;
using FutureState.Data.Providers;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Batch.Tests
{
    public class MaybeLoader : LoaderBase<MaybeLoaderDto, MaybeEntity>
    {
        private readonly ProviderLinq<MaybeEntity, int> _provider;

        public MaybeLoader(
            ProviderLinq<MaybeEntity,int> provider, 
            IProvideSpecifications<MaybeLoaderDto> rulesProvider,
            IExtractor<MaybeLoaderDto> extractor) 
            : base(
                  extractor ?? new CsvExtractor<MaybeLoaderDto>(), 
                  rulesProvider, 
                  "MaybeEntity")
        {
            _provider = provider;
        }

        protected override void Process(MaybeLoaderDto dtoIn, LoaderState<List<MaybeEntity>> loadState, MaybeEntity dtoOutDefaultMapped)
        {
            // dto should be validated and mapped to target entity by default
            // any custom mapping/translation logic should be implemented here

            // add to valid state
            loadState.Valid.Add(dtoOutDefaultMapped);
        }

        // commit to the underling data store
        protected override void Commit(LoaderState<List<MaybeEntity>> loadState)
        {
            _provider.Add(loadState.Valid);
        }
    }


    public class MaybeLoaderDto
    {
        [Required]
        [NotEmpty("First name cannot be null or empty.")]
        public string FirstName { get; set; }

        [StringLength(10)]
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
    }


    public class MaybeEntity : IEntityMutableKey<int>
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}
