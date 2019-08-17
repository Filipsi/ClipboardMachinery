using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.OverlayDialogs.TagEditor;
using ClipboardMachinery.OverlayDialogs.TagTypeEditor;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IDialogOverlayFactory {

        TagEditorViewModel CreateTagEditor(TagModel tagModel);

        TagTypeEditorViewModel CreateTagTypeEditor(TagTypeModel tagTypeModel, bool isCreatingNew = false);

        void Release(TagEditorViewModel tagEditor);

        void Release(TagTypeEditorViewModel tagTypeEditor);

    }

}
