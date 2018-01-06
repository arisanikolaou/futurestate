using Autofac;
using FutureState.Reflection;

namespace FutureState.Autofac
{
    public static class ApplicationContainerBuilderEx
    {
        public static ApplicationContainerBuilder RegisterAll(this ContainerBuilder cb, AppTypeScanner scanner)
        {
            return new ApplicationContainerBuilder(cb, scanner).RegisterAll();
        }
    }
}