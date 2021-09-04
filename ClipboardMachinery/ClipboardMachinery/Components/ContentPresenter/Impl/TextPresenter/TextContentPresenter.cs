using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.ContentPresenter.Impl.ImagePresenter;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.TextPresenter {

    public class TextContentPresenter : IContentPresenter {

        #region Properties

        public string Id { get; } = "CM:Text";

        public string Name { get; } = "Text";

        public string Description { get; } = "Display content as a raw text with default formatting.";

        public string Icon { get; } = "IconTextFile";

        public bool UsableAsDefault { get; } = true;

        #endregion

        #region Fields

        private readonly IContentScreenFactory contentScreenFactory;

        #endregion

        public TextContentPresenter(IContentScreenFactory contentScreenFactory) {
            this.contentScreenFactory = contentScreenFactory;
        }

        #region Logic

        public bool CanDisplayContent(string content) {
            return !content.StartsWith(ImageContentPresenter.ImageDataPrefix);
        }

        public int GetConfidence(string content, IContentPresenter contender) {
            return -1;
        }

        public ContentScreen CreateContentScreen(ClipViewModel clip) {
            return contentScreenFactory.CreateTextScreen(
                creator: this,
                owner: clip,
                releaseFn: contentScreenFactory.Release
            );
        }

        #endregion

    }

}
