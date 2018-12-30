using Caliburn.Micro;

namespace ClipboardMachinery.Plumbing {

    public interface IShell : IConductor {

        bool IsVisible { get; set; }

    }

}
