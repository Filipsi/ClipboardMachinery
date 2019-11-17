using System.Windows;
using System.Windows.Interop;
using Caliburn.Micro;
using ClipboardMachinery.Core.Services.Clipboard;

namespace ClipboardMachinery.Windows.Shell {

    public partial class ShellView : Window {

        public ShellView() {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            // Clipboard initialization is done here instead of ViewModel, because we need direct reference to the Window.
            // Doing it this way removes the need for ViewModel to provide reference to the window using IViewAware.
            // The operation also has to be performed when the window if fully loaded and ready or we will get invalid handle pointer.
            IoC.Get<IClipboardService>().Start(new WindowInteropHelper(this).Handle);
        }

    }

}
