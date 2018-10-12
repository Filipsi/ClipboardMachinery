using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public class ClipboardEventArgs : EventArgs {

        public string Payload { get; }

        public ClipboardEventArgs(string payload) {
            Payload = payload;
        }

    }

}
