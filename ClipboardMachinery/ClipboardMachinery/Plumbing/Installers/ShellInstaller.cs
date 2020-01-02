using System;
using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Plumbing.Customization;
using ClipboardMachinery.Windows.Shell;
using Onova;
using Onova.Services;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ShellInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container
                .Kernel
                .ComponentModelBuilder
                .AddContributor(new DoNotWireActiveItem());

            container.Register(
                Component
                    .For<IWindsorContainer>()
                    .Instance(container)
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IWindowManager>()
                    .ImplementedBy<WindowManager>()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IEventAggregator>()
                    .ImplementedBy<EventAggregator>()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<UpdateManager>()
                    .Instance(new UpdateManager(
                        new GithubPackageResolver(AppBootstrapper.REPOSITORY_OWNER, AppBootstrapper.REPOSITORY_NAME, "ClipboardMachinery-*.zip"),
                        new ZipPackageExtractor()
                    ))
                    .Named("GithubUpdateManager")
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IShell>()
                    .ImplementedBy<ShellViewModel>()
                    .LifestyleSingleton()
            );
        }

    }

}
