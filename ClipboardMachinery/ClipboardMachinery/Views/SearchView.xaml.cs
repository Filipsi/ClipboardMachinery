using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ClipboardMachinery.Views {

    public partial class SearchView : UserControl {

        public SearchView() {
            InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

    }

}
