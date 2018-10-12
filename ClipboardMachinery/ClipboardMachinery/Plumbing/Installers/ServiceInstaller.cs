using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ServiceInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Classes
                    .FromAssemblyNamed("ClipboardMachinery.Core")
                    .InNamespace("ClipboardMachinery.Core.Services", includeSubnamespaces: true)
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton()
            );
        }

    }

}
