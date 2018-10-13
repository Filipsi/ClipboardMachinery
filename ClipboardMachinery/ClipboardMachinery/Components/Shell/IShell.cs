using Caliburn.Micro;

namespace ClipboardMachinery.Components.Shell {

    public interface IShell : IConductor {

        bool IsVisible { get; set; }

    }

}
