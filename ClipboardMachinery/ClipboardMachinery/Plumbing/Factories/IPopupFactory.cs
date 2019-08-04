using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Popups.TagEditor;
using ClipboardMachinery.Popups.TagTypeEditor;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IPopupFactory {

        TagEditorViewModel CreateTagEditor(TagModel tagModel);

        TagTypeEditorViewModel CreateTagTypeEditor(TagTypeModel tagTypeModel, bool isCreatingNew = false);

        void Release(TagEditorViewModel tagEditor);

        void Release(TagTypeEditorViewModel tagTypeEditor);

    }

}
