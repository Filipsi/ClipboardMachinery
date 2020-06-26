using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Plumbing.Factories;
using System;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.LinkPresenter {

    public class LinkContentPresenter : IContentPresenter {

        #region Properties

        public string Id { get; } = "CM:Link";

        public string Name { get; } = "Link";

        public string Description { get; } = "Displays content as a web link with meta-data preview.";

        public string Icon { get; } = "IconLink";

        public bool UsableAsDefault { get; } = true;

        #endregion

        #region Fields

        private readonly IContentScreenFactory contentScreenFactory;

        #endregion

        public LinkContentPresenter(IContentScreenFactory contentScreenFactory) {
            this.contentScreenFactory = contentScreenFactory;
        }

        #region Logic

        public bool CanDisplayContent(string content) {
            return Uri.IsWellFormedUriString(content, UriKind.Absolute);
        }

        public int GetConfidence(string content, IContentPresenter contender) {
            return 20;
        }

        public ContentScreen CreateContentScreen(ClipViewModel clip) {
            return contentScreenFactory.CreateLinkScreen(
                creator: this,
                owner: clip,
                releaseFn: (screen) => contentScreenFactory.Release(screen)
            );
        }

        #endregion

    }

}
