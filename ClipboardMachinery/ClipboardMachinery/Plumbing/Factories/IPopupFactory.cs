using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Popup.TagEditor;
using ClipboardMachinery.Popup.TagTypeEditor;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IPopupFactory {

        TagEditorViewModel CreateTagEditor(TagModel tagModel);

        TagTypeEditorViewModel CreateTagTypeEditor(TagTypeModel tagTypeModel);

        void Release(TagEditorViewModel tagEditor);

        void Release(TagTypeEditorViewModel tagTypeEditor);

    }

}
