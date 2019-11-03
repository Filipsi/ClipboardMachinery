using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ClipboardMachinery.Windows.UpdateNotes {

    public partial class UpdateNotesView : Window {

        public UpdateNotesView() {
            InitializeComponent();
        }

        private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) {
            Process.Start(e.Parameter.ToString());
        }

    }

}
