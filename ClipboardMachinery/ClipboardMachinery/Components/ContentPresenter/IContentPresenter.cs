using System.Collections.Generic;
using System.Windows.Media;
using ClipboardMachinery.Components.Clip;

namespace ClipboardMachinery.Components.ContentPresenter {

    /// <summary>
    /// Represents an option in presenting clip content.
    /// </summary>
    public interface IContentPresenter {

        /// <summary>
        /// Identification of the presenter that will be used in persistent storage.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name of content presenter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of how the presenter will display the content.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Path to a <see cref="Geometry"/> resource to use as an icon.
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// A flag indicating whatever the content provider can precipitate in selection to display data when new clip is created.
        /// When set to false, the provided can only be manually selected by the user.
        /// </summary>
        bool CanBeDefault { get; }

        /// <summary>
        /// Determinants whenever the content can be displayed by the presenter or not.
        /// </summary>
        /// <param name="content">Content of a clip.</param>
        /// <returns>True if the presenter can display provided content, false if not.</returns>
        bool CanDisplayContent(string content);

        /// <summary>
        /// Resolves confidence of a presenter in displaying the content correctly.
        /// This is used to resolve conflicts where multiple presenters vote that can display clip content.
        /// Higher number represents bigger continence in displaying content correctly.
        /// Presenters that have zero or negative continence numbers are not used in case of collision.
        /// </summary>
        /// <param name="content">Content of a clip.</param>
        /// <param name="voters">A list of colliding provides that voted that can display the content.</param>
        /// <returns>Numerical continence in displaying provided content correctly.</returns>
        int GetConfidence(string content, IEnumerable<IContentPresenter> voters);

        /// <summary>
        /// Creates a screen that will display provided clip.
        /// This screen can accept events from it's clip and is automatically released when deactivated.
        /// </summary>
        /// <param name="clip">Clip which content will be displayed.</param>
        /// <returns>A screen that will display provided content.</returns>
        ContentScreen CreateContentScreen(ClipViewModel clip);

    }

}
