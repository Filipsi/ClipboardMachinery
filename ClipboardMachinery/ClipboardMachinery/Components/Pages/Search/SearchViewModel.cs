using Caliburn.Micro;
using ClipboardMachinery.Components.Navigator;

namespace ClipboardMachinery.Components.Pages.Search {

    public class SearchViewModel : Screen, IScreenPage {

        #region IPage

        public string Title
            => "Search";

        public string Icon
            => "IconSearch";

        public byte Order
            => 3;

        #endregion

    }

}
