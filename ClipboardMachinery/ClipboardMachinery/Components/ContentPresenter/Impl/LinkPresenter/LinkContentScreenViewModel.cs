using ClipboardMachinery.Components.Clip;
using System;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.LinkPresenter {

    public class LinkContentScreenViewModel : ContentScreen {

        public LinkContentScreenViewModel(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn) : base(creator, owner, releaseFn) {
        }

        public override string GetClipboardString() {
            return Clip.Model.Content;
        }

    }

}
