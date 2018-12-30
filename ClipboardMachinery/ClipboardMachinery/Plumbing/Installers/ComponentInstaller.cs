using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Windows.Shell;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ComponentInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Classes
                    .FromThisAssembly()
                    .BasedOn<IScreenPage>()
                    .WithServiceBase()
                    .LifestyleSingleton()
            );

            container.Register(
                Classes
                    .FromThisAssembly()
                    .InNamespace("ClipboardMachinery", includeSubnamespaces: true)
                    .If(type => !string.IsNullOrEmpty(type.Name) && type.Name.EndsWith("ViewModel"))
                    .Unless(type => typeof(IShell).IsAssignableFrom(type))
                    .LifestyleTransient()
            );

            container.Register(
                Component
                    .For<IClipViewModelFactory>()
                    .AsFactory()
            );
        }

    }

}
