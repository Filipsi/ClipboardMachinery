using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.ContentPresenter.Impl.ImagePresenter;
using ClipboardMachinery.Plumbing.Factories;
using System;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.BatchScriptPresenter {

    public class BatchScriptContentPresenter : IContentPresenter {

        #region Properties

        public string Id { get; } = "CM:BatchScript";

        public string Name { get; } = "Script";

        public string Description { get; } = "Execute content as a batch script using the command line.";

        public string Icon { get; } = "IconGears";

        public bool UsableAsDefault { get; } = false;

        #endregion

        #region Fields

        private readonly IContentScreenFactory contentScreenFactory;

        #endregion

        public BatchScriptContentPresenter(IContentScreenFactory contentScreenFactory) {
            this.contentScreenFactory = contentScreenFactory;
        }

        #region Logic

        public bool CanDisplayContent(string content) {
            return !content.StartsWith(ImageContentPresenter.ImageDataPrefix)
                && !Uri.IsWellFormedUriString(content, UriKind.Absolute);
        }

        public int GetConfidence(string content, IContentPresenter contender) {
            return 0;
        }

        public ContentScreen CreateContentScreen(ClipViewModel clip) {
            return contentScreenFactory.CreateBatchScriptScreen(
                creator: this,
                owner: clip,
                releaseFn: contentScreenFactory.Release
            );
        }

        #endregion

    }

}
