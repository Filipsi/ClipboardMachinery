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

        #region Properties

        private static readonly NotificationForm notificationHandler = new NotificationForm();
        private static readonly string imageHeader = "data:image/png;base64,";

        private string ignoreValue;

        #endregion

        public ClipboardService() {
            notificationHandler.ClipboardChanged += OnClipboardChanged;
        }

        #region IClipboardService

        private void OnClipboardChanged(object sender, EventArgs e) {
            string content = string.Empty;

            if (System.Windows.Forms.Clipboard.ContainsText()) {
                content = System.Windows.Forms.Clipboard.GetText();

            } else if (System.Windows.Forms.Clipboard.ContainsImage()) {
                using (Image image = System.Windows.Forms.Clipboard.GetImage()) {
                    if (image != null) {
                        using (MemoryStream ms = new MemoryStream()) {
                            image.Save(ms, ImageFormat.Png);
                            content = $"{imageHeader}{Convert.ToBase64String(ms.ToArray())}";
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
            if (content.StartsWith(imageHeader)) {
                try {
                    byte[] rawImage = Convert.FromBase64String(content.Remove(0, imageHeader.Length));
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
