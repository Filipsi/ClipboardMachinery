using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.Favorites {

    public class FavoritesViewModel : LazyClipPage, IScreenPage {

        #region IPage

        public string Title
            => "Favorites";

        public string Icon
            => "IconStarFull";

        public byte Order
            => 2;

        #endregion

        public FavoritesViewModel(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) : base(15, dataRepository, clipVmFactory) {
            AllowAddingClipsFromKeyboard = false;
            ClearAllItemsOnDeactivate = true;
        }

    }

}
