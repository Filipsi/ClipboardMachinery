using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.FileSystem;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ServiceInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Component.For<ClipFile>().LifestyleSingleton()
            );

            container.Register(
                Classes
                    .FromThisAssembly()
                    .InNamespace("ClipboardMachinery.Logic", includeSubnamespaces: true)
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton()
            );
        }

    }

}
