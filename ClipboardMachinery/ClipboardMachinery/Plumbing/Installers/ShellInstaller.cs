using Caliburn.Micro;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Plumbing.Facilities;
using ClipboardMachinery.Windows.Shell;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ShellInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container
                .AddFacility<TypedFactoryFacility>()
                .AddFacility<EventAggregatorFacility>();

            container
                .Register(Component.For<IWindsorContainer>().Instance(container).LifestyleSingleton())
                .Register(Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton())
                .Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton())
                .Register(Component.For<IShell>().ImplementedBy<ShellViewModel>().LifestyleSingleton());
        }

    }

}
