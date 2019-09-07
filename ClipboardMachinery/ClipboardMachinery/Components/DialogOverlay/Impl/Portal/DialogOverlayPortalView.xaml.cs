using System.Windows.Controls;
using System.Windows.Input;

namespace ClipboardMachinery.Components.DialogOverlay.Impl.Portal {

    public partial class DialogOverlayPortalView : UserControl {

        public DialogOverlayPortalView() {
            InitializeComponent();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Focus();
        }

    }

}
