using FutureState.Data.Providers;
using FutureState.Services.Web.Model;
using System;
namespace FutureState.Services.Web.Api
{
    public class MaybeEntityController : FsControllerBase<MaybeEntity,Guid>
    {

        public MaybeEntityController(ProviderLinq<MaybeEntity,Guid> service) : base(service)
        {
        }
    }
}