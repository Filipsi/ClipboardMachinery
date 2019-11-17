using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.DataStorage.Impl;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;

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
                    .DependsOn(
                        Dependency.OnConfigValue("databasePath", "storage.sqlite"),
                        Dependency.OnConfigValue("databaseVersion", "3")
                    )
                    .LifestyleBoundToNearest<IDataRepository>()
            );

            container.Register(
                Component
                    .For<IClipboardService>()
                    .ImplementedBy<ClipboardService>()
                    .StopUsingMethod(c => c.Stop)
                    .LifeStyle.Singleton
            );

            container.Register(
                Component
                    .For<IHotKeyService>()
                    .ImplementedBy<HotKeyService>()
                    .LifeStyle.Singleton
            );
        }

    }

}
