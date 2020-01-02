using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ClipboardMachinery.Components.ColorGallery;
using ClipboardMachinery.Components.ContentPresenter;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.DialogOverlay.Impl;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.TagKind;
using ClipboardMachinery.Core.TagKind.Impl;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Windows.UpdateNotes;

namespace ClipboardMachinery.Plumbing.Installers {

    public class ComponentInstaller : IWindsorInstaller {

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Classes
                    .FromThisAssembly()
                    .BasedOn<IScreenPage>()
                    .WithServiceBase()
                    .WithServiceSelf()
            );

            container.Register(
                Component
                    .For<IDialogOverlayManager>()
                    .ImplementedBy<DialogOverlayManager>()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<UpdateNotesViewModel>()
                    .DependsOn(
                        Dependency.OnValue("repositoryOwner", AppBootstrapper.REPOSITORY_OWNER),
                        Dependency.OnValue("repositoryName", AppBootstrapper.REPOSITORY_NAME))
                    .LifestyleTransient()
            );

            container.Register(
                Classes
                    .FromThisAssembly()
                    .InNamespace("ClipboardMachinery", includeSubnamespaces: true)
                    .If(type => !string.IsNullOrEmpty(type.Name) && type.Name.EndsWith("ViewModel"))
                    .Unless(type => typeof(IShell).IsAssignableFrom(type))
                    .Unless(type => type == typeof(UpdateNotesViewModel))
                    .LifestyleTransient()
            );

            container.Register(
                Classes
                    .FromThisAssembly()
                    .BasedOn<IColorPalette>()
                    .WithService.Base()
                    .LifestyleSingleton()
            );

            container.Register(
                Classes
                    .FromThisAssembly()
                    .BasedOn<IContentPresenter>()
                    .WithService.FromInterface()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<ITagKindManager>()
                    .ImplementedBy<TagKindManager>()
                    .LifestyleSingleton()
            );

            container.Register(
                Classes
                    .FromAssemblyContaining(typeof(ITagKindSchema))
                    .BasedOn<ITagKindSchema>()
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IContentDisplayResolver>()
                    .ImplementedBy<ContentDisplayResolver>()
                    .LifestyleSingleton()
            );

            container.Register(
                Component
                    .For<IContentScreenFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<ITagKindFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<ITagKindSchemaFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<IClipFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<IDialogOverlayFactory>()
                    .AsFactory()
            );

            container.Register(
                Component
                    .For<IWindowFactory>()
                    .AsFactory()
            );
        }

    }

}
