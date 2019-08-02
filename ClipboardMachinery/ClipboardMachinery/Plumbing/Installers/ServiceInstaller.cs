using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.DataStorage.Impl;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ServiceInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Component
                    .For<IDataRepository>()
                    .ImplementedBy<DataRepository>()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IDatabaseAdapter>()
                    .ImplementedBy<DatabaseAdapter>()
                    .DependsOn(Dependency.OnConfigValue("dataSourcePath", "storage.sqlite"))
                    .LifestyleBoundToNearest<IDataRepository>()
            );

            container.Register(
                Classes
                    .FromAssemblyNamed("ClipboardMachinery.Core")
                    .InNamespace("ClipboardMachinery.Core.Services", includeSubnamespaces: true)
                    .If(type => type.Name.EndsWith("Service"))
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton()
            );
        }

    }

}
