using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.Favorites {

    public class FavoritesViewModel : ClipPageBase, IScreenPage {

        #region IScreenPage

        public string Title
            => "Favorites";

        public string Icon
            => "IconStarFull";

        public byte Order
            => 2;

        #endregion

        public FavoritesViewModel(IDataRepository dataRepository, IViewModelFactory vmFactory) : base(15, dataRepository, vmFactory) {
            DataProvider.ApplyTagFilter("category", "favorite");
        }

        #region Logic

        protected override bool IsAllowedAddClipsFromKeyboard(ClipEvent message) {
            return false;
        }

        protected override bool IsClearingItemsWhenDeactivating() {
            return true;
        }

        #endregion

    }

}
