using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Plumbing.Factories;
using System.Text.RegularExpressions;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.ImagePresenter {

    public class ImageContentPresenter : IContentPresenter {

        #region Properties

        public string Id { get; } = "CM:Image";

        public string Name { get; } = "Image";

        public string Description { get; } = "Displays content as bitmap image.";

        public string Icon { get; } = "IconPicture";

        public bool UsableAsDefault { get; } = true;

        #endregion

        #region Fields

        public static readonly string ImageDataPrefix = "data:image";

        public static readonly Regex Base64DataPattern = new Regex(
            pattern: @"^data\:(?<visiblityChangeType>image\/(png|tiff|jpg|gif|bmp|webp));base64,(?<data>[A-Z0-9\+\/\=]+)$",
            options: RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );

        private readonly IContentScreenFactory contentScreenFactory;

        #endregion

        public ImageContentPresenter(IContentScreenFactory contentScreenFactory) {
            this.contentScreenFactory = contentScreenFactory;
        }

        #region Logic

        public bool CanDisplayContent(string content) {
            return Base64DataPattern.IsMatch(content);
        }

        public int GetConfidence(string content, IContentPresenter contender) {
            return 100;
        }

        public ContentScreen CreateContentScreen(ClipViewModel clip) {
            return contentScreenFactory.CreateImageScreen(
                creator: this,
                owner: clip,
                releaseFn: contentScreenFactory.Release
            );
        }

        #endregion

    }

}
