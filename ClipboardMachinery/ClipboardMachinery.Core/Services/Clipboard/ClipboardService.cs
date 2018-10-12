using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public class ClipboardService : IClipboardService {

        #region Events

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        #endregion

        #region Properties

        private readonly NotificationForm notificationHandler;
        private string ignoreValue;

        #endregion

        public ClipboardService() {
            notificationHandler = new NotificationForm(this);
        }

        #region IClipboardService

        public void NotifyOfClipboardChange() {
            string payload = string.Empty;

            if (System.Windows.Forms.Clipboard.ContainsText()) {
                payload = System.Windows.Forms.Clipboard.GetText();

            } else if (System.Windows.Forms.Clipboard.ContainsImage()) {
                using (Image image = System.Windows.Forms.Clipboard.GetImage()) {
                    if (image != null) {
                        using (MemoryStream ms = new MemoryStream()) {
                            image.Save(ms, ImageFormat.Png);
                            payload = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
                        }
                    }
                }
            }

            if (ignoreValue == payload) {
                return;
            }

            ClipboardChanged?.Invoke(null, new ClipboardEventArgs(payload));
        }

        public void IgnoreNextChange(string value) {
            ignoreValue = value;
        }

        public void SetClipboardContent(object content) {
            if (typeof(Image).IsAssignableFrom(content.GetType())) {
                System.Windows.Forms.Clipboard.SetImage((Image)content);
                return;
            }

            System.Windows.Forms.Clipboard.SetText((string)content);
        }

        #endregion

    }

}
