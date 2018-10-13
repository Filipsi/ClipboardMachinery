using System.Collections.Specialized;
using Caliburn.Micro;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Plumbing;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.Components.Pages.History {

    public class HistoryViewModel : Screen, IScreenPage {

        #region IPage

        public string Title => "History";

        public string Icon => "IconHistory";

        #endregion

    }

}
