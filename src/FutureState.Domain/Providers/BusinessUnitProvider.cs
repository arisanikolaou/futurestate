using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Providers
{
    public class BusinessUnitProvider : ProviderLinq<BusinessUnit, Guid>
    {
        public BusinessUnitProvider(
            IUnitOfWorkLinq<BusinessUnit,Guid> db,
            IEntityIdProvider<BusinessUnit, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<BusinessUnit> specProvider = null,
            EntityHandler<BusinessUnit,Guid> entityHandler = null) 
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
        }

        /// <summary>
        ///     Gets all business units contained in a given instance.
        /// </summary>
        public IList<BusinessUnit> GetBusinessUnits(BusinessUnit unit)
        {
            using (this.Db.Open())
                return Db.EntitySet.LinqReader.Where(m => m.ParentId.HasValue && m.ParentId.Value == unit.Id).ToList();
        }

        /// <summary>
        ///     Gets a business unit by its external key.
        /// </summary>
        public BusinessUnit GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId).FirstOrDefault();
        }

        /// <summary>
        ///     Ensures that a valid user for the business unit exists.
        /// </summary>
        /// <param name="entity"></param>
        protected override void OnBeforeAdd(BusinessUnit entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);
        }
    }
}