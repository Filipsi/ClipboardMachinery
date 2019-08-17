using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.TagTypes {

    public class TagTypesViewModel : LazyPageBase<TagTypeViewModel, TagTypeModel>, IScreenPage, IHandle<TagEvent> {

        #region IScreenPage

        public string Title
            => "Tags";

        public string Icon
            => "IconTag";

        public byte Order
            => 3;

        #endregion

        #region Properties

        public bool CanCreateNew {
            get => canCreateNew;
            private set {
                if (canCreateNew == value) {
                    return;
                }

                canCreateNew = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly IDialogOverlayManager dialogOverlayManager;
        private readonly IViewModelFactory vmFactory;

        private bool canCreateNew = true;

        #endregion

        public TagTypesViewModel(IDialogOverlayManager dialogOverlayManager, IDataRepository dataRepository, IViewModelFactory vmFactory)
            : base(dataRepository.CreateLazyTagTypeProvider(15)) {

            this.dialogOverlayManager = dialogOverlayManager;
            this.vmFactory = vmFactory;
        }

        #region Logic

        protected override bool IsClearingItemsWhenDeactivating() {
            return true;
        }

        protected override TagTypeViewModel CreateItem(TagTypeModel model) {
            return vmFactory.CreateTagType(model);
        }

        protected override void ReleaseItem(TagTypeViewModel instance) {
            vmFactory.Release(instance);
        }

        #endregion

        #region Actions

        public void CreateNew() {
            dialogOverlayManager.OpenDialog(
                () => {
                    CanCreateNew = false;
                    TagTypeModel newTagType = new TagTypeModel { Kind = typeof(string) };
                    return dialogOverlayManager.Factory.CreateTagTypeEditor(newTagType, isCreatingNew: true);
                },
                (editor) => {
                    dialogOverlayManager.Factory.Release(editor);
                    CanCreateNew = true;
                }
            );
        }

        #endregion

        #region Handlers

        public async Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            switch (message.EventType) {
                case TagEvent.TagEventType.TypeAdded:
                case TagEvent.TagEventType.TypeRemoved:
                    await Reset();
                    break;
            }
        }

        #endregion

    }

}
