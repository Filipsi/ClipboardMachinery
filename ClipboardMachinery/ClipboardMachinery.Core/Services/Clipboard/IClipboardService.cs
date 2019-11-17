using System;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public interface IClipboardService {

        bool IsRunning { get; }

        event EventHandler<ClipboardEventArgs> ClipboardChanged;

        void Start(IntPtr handle);

        void Stop();

        void SetClipboardContent(string content);

    }

}
