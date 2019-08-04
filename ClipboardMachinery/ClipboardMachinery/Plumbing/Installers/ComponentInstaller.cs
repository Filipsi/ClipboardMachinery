using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.ColorGallery;
using ClipboardMachinery.Components.ColorGallery.Presets;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Plumbing.Factories;

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
                Classes
                    .FromThisAssembly()
                    .BasedOn<IColorPalette>()
                    .WithServiceBase()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IColorGalleryFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<ITagKindHandler>()
                    .ImplementedBy<TagKindHandler>()
                    .LifestyleSingleton()
            );

            container.Register(
                Classes
                    .FromThisAssembly()
                    .BasedOn<ITagKindSchema>()
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<ITagKindFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<IViewModelFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<IPopupFactory>()
                    .AsFactory()
            );
        }

    }

}
