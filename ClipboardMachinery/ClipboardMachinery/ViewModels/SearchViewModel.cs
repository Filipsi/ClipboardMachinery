using Caliburn.Micro;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.ViewModels {

    public class SearchViewModel : Screen, IPage {

        #region IPage

        public string Title { get; } = "Search";

        public string Icon { get; } = "IconSearch";

        #endregion

    }

}
