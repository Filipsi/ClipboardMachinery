using Caliburn.Micro;
using ClipboardMachinery.Components.Navigator;

namespace ClipboardMachinery.Components.Pages.Favorites {

    public class FavoritesViewModel : Screen, IScreenPage {

        #region IPage

        public string Title
            => "Favorites";

        public string Icon
            => "IconStarFull";

        public byte Order
            => 2;

        #endregion

    }

}
