using Caliburn.Micro;

namespace ClipboardMachinery.Windows.Shell {

    public interface IShell : IConductor {

        bool IsVisible { get; set; }

    }

}
