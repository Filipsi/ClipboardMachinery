using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Pages.History;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.Components.Pages.Favorites {

    public class FavoritesViewModel : Screen, IScreenPage {

        #region IPage

        public string Title { get; } = "Favorites";

        public string Icon { get; } = "IconStarFull";

        #endregion

    }

}
