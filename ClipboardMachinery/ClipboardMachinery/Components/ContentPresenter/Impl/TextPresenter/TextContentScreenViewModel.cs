using System;
using ClipboardMachinery.Components.Clip;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.TextPresenter {

    // TODO: Implement max text length and show more button
    public class TextContentScreenViewModel : ContentScreen {

        public TextContentScreenViewModel(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn) : base(creator, owner, releaseFn) {
        }

        public override string GetClipboardString() {
            return Clip.Model.Content;
        }

    }

}
