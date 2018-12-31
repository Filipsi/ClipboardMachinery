using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Popup.TagEditor;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IPopupFactory {

        TagEditorViewModel CreateTagEditor(TagModel tagModel);

        void Release(TagEditorViewModel clipVm);

    }
}
