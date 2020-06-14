using Caliburn.Micro;
using ClipboardMachinery.Common.Converters;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Clip;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.ImagePresenter {

    public class ImageContentScreenViewModel : ContentScreen {

        #region Fields

        private readonly ActionButtonViewModel exportImageButton;

        #endregion

        public ImageContentScreenViewModel(
            IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn,
            ActionButtonViewModel actionButton) : base(creator, owner, releaseFn) {

            // Create export image button
            exportImageButton = actionButton;
            exportImageButton.ToolTip = "Save as file";
            exportImageButton.Icon = (Geometry)Application.Current.FindResource("IconExport");
            exportImageButton.ClickAction = ExportImage;
            exportImageButton.ConductWith(this);

            // Add export button to side controls of the clip
            Clip.SideControls.Add(exportImageButton);
        }

        #region Handlers

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            // Remove export button from side controls of the clip
            if (close && Clip.SideControls.Contains(exportImageButton)) {
                Clip.SideControls.Remove(exportImageButton);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Logic

        public override string GetClipboardString() {
            return Clip.Model.Content;
        }

        private Task ExportImage(ActionButtonViewModel source) {
            // Create dialog for user to pick save location
            SaveFileDialog dialog = new SaveFileDialog {
                FileName = $"{Clip.Model.Id}-{DateTime.UtcNow.ToFileTimeUtc()}",
                Filter = "Portable Network Graphics (*.png)|*.png|All files (*.*)|*.*"
            };

            // Make sure that the dialog was accepted
            if (dialog.ShowDialog() != true) {
                return Task.CompletedTask;
            }

            // Convert base64 string to an image
            IValueConverter converter = new Base64ToImageConverter();
            if (!(converter.Convert(Clip.Model.Content, typeof(string), null, CultureInfo.CurrentCulture) is BitmapImage bitmapImage)) {
                return Task.CompletedTask;
            }

            // Encode image as bitmap frame
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            // Save the frame to file
            using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create)) {
                encoder.Save(fileStream);
            }

            return Task.CompletedTask;
        }


        #endregion

    }

}
