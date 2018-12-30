using System;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public interface IClipboardService {

        event EventHandler<ClipboardEventArgs> ClipboardChanged;

        void IgnoreNextChange(string value);

        void NotifyOfClipboardChange();

        void SetClipboardContent(object content);

    }

}
