using Autofac;

namespace FutureState.App
{
    public static class ApplicationContainerBuilderEx
    {
        public static ApplicationContainerBuilder BuildApp(this ContainerBuilder cb)
        {
            return new ApplicationContainerBuilder(cb);
        }
    }
}
