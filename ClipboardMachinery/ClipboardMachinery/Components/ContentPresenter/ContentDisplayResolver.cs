using Castle.Core.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ClipboardMachinery.Components.ContentPresenter {

    public class ContentDisplayResolver : IContentDisplayResolver {

        #region Properties

        public ILogger Logger { get; set; }

        #endregion

        #region Fields

        private readonly IEnumerable<IContentPresenter> contentPresenters;
        private readonly IReadOnlyDictionary<string, IContentPresenter> idMap;

        #endregion

        public ContentDisplayResolver(IEnumerable<IContentPresenter> contentPresenters, ILogger logger) {
            this.contentPresenters = contentPresenters;
            Logger = logger ?? NullLogger.Instance;

            Logger.Info("Listing available content preseters...");
            foreach (IContentPresenter presenter in contentPresenters) {
                Logger.Info($" - Id={presenter.Id}, Name={presenter.Name}");
            }

            idMap = new ReadOnlyDictionary<string, IContentPresenter>(
                contentPresenters.ToDictionary(
                    keySelector: cp => cp.Id,
                    elementSelector: cp => cp
                )
            );
        }

        #region IClipContentResolver

        public IEnumerable<IContentPresenter> GetCompatiblePresenters(string content) {
            // Filter out preseters that can display the content
            List<IContentPresenter> compatiblePresenters = contentPresenters
                .Where(cp => cp.CanDisplayContent(content))
                .ToList();

            // Sort presenters in palce by ther confidence values
            compatiblePresenters.Sort(
                (x, y) => x.GetConfidence(content, y).CompareTo(y.GetConfidence(content, x))
            );

            return compatiblePresenters;
        }

        public IContentPresenter GetDefaultPresenter(string content) {
            // Filter out compatible presenters that can be used as defaults
            return GetCompatiblePresenters(content)
                .Where(cp => cp.UsableAsDefault)
                .FirstOrDefault();
        }

        public bool TryGetPresenter(string id, out IContentPresenter contentPresenter) {
            bool hasPresenter = idMap.ContainsKey(id);
            contentPresenter = hasPresenter ? idMap[id] : null;
            return hasPresenter;
        }

        #endregion

    }

}
