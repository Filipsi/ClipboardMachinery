using System;
using ClipboardMachinery.Components.Clip;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.TextPresenter {

    public class TextContentScreenViewModel : ContentScreen {

        public TextContentScreenViewModel(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn) : base(creator, owner, releaseFn) {
        }

    }

}
