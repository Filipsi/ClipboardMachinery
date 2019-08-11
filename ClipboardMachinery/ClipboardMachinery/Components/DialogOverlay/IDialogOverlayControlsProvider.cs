using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Components.DialogOverlay {

    internal interface IDialogOverlayControlsProvider {

        BindableCollection<ActionButtonViewModel> DialogControls { get; }

    }

}

