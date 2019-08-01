using Caliburn.Micro;
using ClipboardMachinery.Components.TagType;

namespace ClipboardMachinery.Popup.TagTypeEditor {

    public class TagTypeEditorViewModel : Screen {

        #region Properties

        public TagTypeModel Model {
            get;
        }

        #endregion

        public TagTypeEditorViewModel(TagTypeModel tagTypeModel) {
            Model = tagTypeModel;
        }

    }

}
