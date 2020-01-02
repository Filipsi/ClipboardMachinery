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
            return contentPresenters
                .Where(cp => cp.CanDisplayContent(content))
                .ToArray();
        }

        public IContentPresenter GetDefaultPresenter(string content) {
            IContentPresenter[] candidates = GetCompatiblePresenters(content).ToArray();

            return candidates
               .Where(cp => cp.CanBeDefault)
               .Select(cp => new { Presenter = cp, Confidence = cp.GetConfidence(content, candidates) })
               .Where(info => info.Confidence > 0)
               .OrderByDescending(info => info.Confidence)
               .Select(info => info.Presenter)
               .FirstOrDefault();
        }

        public IContentPresenter GetPresenter(string id) {
            return idMap.ContainsKey(id) ? idMap[id] : null;
        }

        #endregion

    }

}
