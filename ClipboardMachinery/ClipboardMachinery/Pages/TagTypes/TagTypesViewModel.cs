using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Components.TagTypeLister;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.TagTypes {

    public class TagTypesViewModel : TagTypeListerViewModel, IScreenPage {

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

        private bool canCreateNew = true;

        #endregion

        public TagTypesViewModel(IDialogOverlayManager dialogOverlayManager, IDataRepository dataRepository, IClipFactory vmFactory)
            : base(dataRepository, vmFactory) {

            this.dialogOverlayManager = dialogOverlayManager;
        }

        #region Handlers

        protected override void OnItemSelected(TagTypeViewModel item) {
            base.OnItemSelected(item);
            EditTagType(item.Model);
        }

        #endregion

        #region Actions

        public void CreateNew() {
            dialogOverlayManager.OpenDialog(
                () => {
                    CanCreateNew = false;
                    return dialogOverlayManager.Factory.CreateTagTypeEditor();
                },
                editor => {
                    dialogOverlayManager.Factory.Release(editor);
                    CanCreateNew = true;
                }
            );
        }

        public void EditTagType(TagTypeModel tagType) {
            dialogOverlayManager.OpenDialog(
                () => dialogOverlayManager.Factory.CreateTagTypeEditor(tagType),
                editor => dialogOverlayManager.Factory.Release(editor)
            );
        }

        #endregion

    }

}
