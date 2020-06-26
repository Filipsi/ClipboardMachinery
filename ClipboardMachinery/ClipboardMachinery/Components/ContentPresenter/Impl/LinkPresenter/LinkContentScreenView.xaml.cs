using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.LinkPresenter {

    public partial class LinkContentScreenView : UserControl {

        public LinkContentScreenView() {
            InitializeComponent();
        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }

    }

}
