using Caliburn.Micro;
using ClipboardMachinery.Components.Navigator;

namespace ClipboardMachinery.Pages.TagTypes {

    public class TagTypesViewModel : Screen, IScreenPage {

        #region IScreenPage

        public string Title
            => "Tag types";

        public string Icon
            => "IconTag";

        public byte Order
            => 3;

        #endregion

    }

}
