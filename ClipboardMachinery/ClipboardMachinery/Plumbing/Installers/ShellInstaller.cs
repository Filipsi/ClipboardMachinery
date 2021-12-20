using System;
using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Plumbing.Customization;
using ClipboardMachinery.Windows.Shell;
using CommandLine;
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
                        GetPackageResolver(container.Resolve<LaunchOptions>()),
                        new ZipPackageExtractor()
                    ))
                    .Named("ApplicationUpdateManager")
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IShell>()
                    .ImplementedBy<ShellViewModel>()
                    .LifestyleSingleton()
            );
        }

        private IPackageResolver GetPackageResolver(LaunchOptions launchOptions) {
            if (string.IsNullOrWhiteSpace(launchOptions.UpdaterBranch)) {
                return new GithubPackageResolver(AppBootstrapper.REPOSITORY_OWNER, AppBootstrapper.REPOSITORY_NAME, "ClipboardMachinery-*.zip");
            }

            return new AppVeyorPackageResolver(AppBootstrapper.REPOSITORY_OWNER, AppBootstrapper.REPOSITORY_NAME, "ClipboardMachinery%2FClipboardMachinery%2Fbin%2FClipboardMachinery-{0}.zip", launchOptions.UpdaterBranch);
        }

    }

}
