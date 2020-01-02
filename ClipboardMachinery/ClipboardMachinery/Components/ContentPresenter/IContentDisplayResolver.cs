using System.Collections.Generic;

namespace ClipboardMachinery.Components.ContentPresenter {

    /// <summary>
    /// A resolver that picks <see cref="IContentPresenter"/> for clip content.
    /// </summary>
    public interface IContentDisplayResolver {

        IEnumerable<IContentPresenter> GetCompatiblePresenters(string content);

        IContentPresenter GetDefaultPresenter(string content);

        IContentPresenter GetPresenter(string id);

    }

}
