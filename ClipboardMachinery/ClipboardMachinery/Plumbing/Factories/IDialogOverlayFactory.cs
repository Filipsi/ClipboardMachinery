using System;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.OverlayDialogs.TagEditor;
using ClipboardMachinery.OverlayDialogs.TagOverview;
using ClipboardMachinery.OverlayDialogs.TagTypeEditor;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IDialogOverlayFactory {

        TagEditorViewModel CreateTagEditor(ClipModel clipModel);

        TagEditorViewModel CreateTagEditor(TagModel tagModel);

        TagTypeEditorViewModel CreateTagTypeEditor();

        TagTypeEditorViewModel CreateTagTypeEditor(TagTypeModel tagTypeModel);

        TagOverviewViewModel CreateTagOverview(ClipModel clip);

        void Release(TagEditorViewModel tagEditor);

        void Release(TagTypeEditorViewModel tagTypeEditor);

        void Release(TagOverviewViewModel tagOverview);

    }

}
