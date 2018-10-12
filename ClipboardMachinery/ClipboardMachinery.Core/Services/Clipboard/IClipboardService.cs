using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public interface IClipboardService {

        event EventHandler<ClipboardEventArgs> ClipboardChanged;

        void IgnoreNextChange(string value);

        void NotifyOfClipboardChange();

        void SetClipboardContent(object content);

    }

}
