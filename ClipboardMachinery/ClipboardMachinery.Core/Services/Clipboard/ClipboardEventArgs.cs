using System;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public class ClipboardEventArgs : EventArgs {

        public string Source { get; }

        public string Payload { get; }

        public ClipboardEventArgs(string source, string payload) {
            Source = source;
            Payload = payload;
        }

    }

}
