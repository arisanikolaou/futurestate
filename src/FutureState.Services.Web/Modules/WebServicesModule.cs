using System.Configuration;
using Autofac;
using FutureState.Autofac;
using FutureState.Autofac.Modules;
using FutureState.Reflection;

namespace FutureState.Services.Web.Modules
{

    public class WebServicesModule : Module
    {
        protected override void Load(ContainerBuilder cb)
        {
            base.Load(cb);

            // register sql repository etc
            cb.RegisterModule(new GenericDataAccessModule());

            // connect to future state db by default
            cb.RegisterModule(new InMemoryDataAccessModule());
        }
    }
}
