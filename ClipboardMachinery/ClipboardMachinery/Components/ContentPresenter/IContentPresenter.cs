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
        bool UsableAsDefault { get; }

        /// <summary>
        /// Determinate whenever the content can be displayed by the presenter or not.
        /// </summary>
        /// <param name="content">Content of a clip.</param>
        /// <returns>True if the presenter can display provided content, false if not.</returns>
        bool CanDisplayContent(string content);

        /// <summary>
        /// Resolves confidence of a presenter in displaying the content correctly.
        /// This is used to resolve conflicts where multiple presenters vote that can display clip content.
        /// Higher number represents bigger continence in displaying content correctly.
        /// </summary>
        /// <param name="content">Content of a clip that will be displayed.</param>
        /// <param name="contender">A colliding provider that also voted that can correctly display the content.</param>
        /// <returns>A value that indicates the relative order of the objects being compared <see cref="https://docs.microsoft.com/en-us/dotnet/api/system.icomparable.compareto?view=netframework-4.8#returns"/>.</returns>
        int GetConfidence(string content, IContentPresenter contender);

        /// <summary>
        /// Creates a screen that will display provided clip.
        /// This screen can accept events from it's clip and is automatically released when deactivated.
        /// </summary>
        /// <param name="clip">Clip which content will be displayed.</param>
        /// <returns>A screen that will display provided content.</returns>
        ContentScreen CreateContentScreen(ClipViewModel clip);

    }

}
