using System.Windows;
using Caliburn.Micro;
using ClipboardMachinery.ViewModels;

namespace ClipboardMachinery {

    internal class Bootstrapper : BootstrapperBase {

        public Bootstrapper() => Initialize();

        protected override void OnStartup(object sender, StartupEventArgs e) {
            DisplayRootViewFor<AppViewModel>();
        }

    }

}

