using Caliburn.Micro;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.Components.Pages.Search {

    public class SearchViewModel : Screen, IScreenPage {

        #region IPage

        public string Title { get; } = "Search";

        public string Icon { get; } = "IconSearch";

        #endregion

    }

}
