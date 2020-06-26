using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.ContentPresenter.Impl.ImagePresenter;
using ClipboardMachinery.Plumbing.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.BatchScriptPresenter {

    public class BatchScriptContentPresenter : IContentPresenter {

        #region Properties

        public string Id { get; } = "CM:BatchScript";

        public string Name { get; } = "Batch script";

        public string Description { get; } = "Allows to run content as a batch script via the command line.";

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
            return !ImageContentPresenter.Base64DataPattern.IsMatch(content)
                && !Uri.IsWellFormedUriString(content, UriKind.Absolute);
        }

        public int GetConfidence(string content, IContentPresenter contender) {
            return 0;
        }

        public ContentScreen CreateContentScreen(ClipViewModel clip) {
            return contentScreenFactory.CreateBatchScriptScreen(
                creator: this,
                owner: clip,
                releaseFn: (screen) => contentScreenFactory.Release(screen)
            );
        }

        #endregion

    }

}
