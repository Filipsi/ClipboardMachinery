using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Components.DialogOverlay {

    public interface IOverlayDialog : IScreen {

        bool IsOpen { get; set; }

        bool AreControlsVisible { get; set; }

        BindableCollection<ActionButtonViewModel> DialogControls { get; }

    }

}

