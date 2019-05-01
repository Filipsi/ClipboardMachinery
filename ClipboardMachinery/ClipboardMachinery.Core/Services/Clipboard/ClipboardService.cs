using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Forms = System.Windows.Forms;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public class ClipboardService : IClipboardService {

        #region Events

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        #endregion

        #region Fields

        private static readonly NotificationForm notificationHandler = new NotificationForm();
        private const string pngImageHeader = "data:image/png;base64,";

        private string ignoreValue;

        #endregion

        public ClipboardService() {
            notificationHandler.ClipboardChanged += OnClipboardChanged;
        }

        #region IClipboardService

        private void OnClipboardChanged(object sender, EventArgs e) {
            string content = string.Empty;

            if (Forms.Clipboard.ContainsText()) {
                content = Forms.Clipboard.GetText();

            } else if (Forms.Clipboard.ContainsImage()) {
                using (Image image = Forms.Clipboard.GetImage()) {
                    if (image != null) {
                        using (MemoryStream rawImage = new MemoryStream()) {
                            image.Save(rawImage, ImageFormat.Png);
                            content = $"{pngImageHeader}{Convert.ToBase64String(rawImage.ToArray())}";
                        }
                    }
                }
            }

            if (ignoreValue == content) {
                return;
            }

            ClipboardChanged?.Invoke(
                sender: this,
                e: new ClipboardEventArgs(
                    source: WindowHelper.GetActiveProcessName(),
                    payload: content
                )
            );
        }

        public void IgnoreNextChange(string value) {
            ignoreValue = value;
        }

        public void SetClipboardContent(string content) {
            if (content.StartsWith(pngImageHeader)) {
                try {
                    byte[] rawImage = Convert.FromBase64String(content.Remove(0, pngImageHeader.Length));
                    using (MemoryStream imageStream = new MemoryStream(rawImage, 0, rawImage.Length)) {
                        Forms.Clipboard.SetImage(Image.FromStream(imageStream));
                    }
                } catch (FormatException) {
                    // NO-OP
                }

                return;
            }

            Forms.Clipboard.SetText(content);
        }

        #endregion

    }

}
