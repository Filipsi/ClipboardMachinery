using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Core.Data;

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
                    .For<IStorageAdapter>()
                    .ImplementedBy<StorageAdapter>()
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
