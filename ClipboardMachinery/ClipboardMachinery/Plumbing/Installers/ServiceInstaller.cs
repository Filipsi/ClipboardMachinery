using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Core.Repositories.Lazy;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ServiceInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Classes
                    .FromAssemblyNamed("ClipboardMachinery.Core")
                    .InNamespace("ClipboardMachinery.Core", includeSubnamespaces: true)
                    .Unless(type => type is ILazyDataProvider)
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton()
            );
        }

    }

}
