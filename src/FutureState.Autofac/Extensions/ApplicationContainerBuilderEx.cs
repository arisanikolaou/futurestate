using Autofac;
using FutureState.Reflection;

namespace FutureState.Autofac.Extensions
{
    public static class ApplicationContainerBuilderEx
    {
        public static ApplicationContainerBuilder BuildApp(this ContainerBuilder cb, AppTypeScanner scanner)
        {
            return new ApplicationContainerBuilder(cb, scanner);
        }
    }
}