using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Components.Popup {

    internal interface IPopupControlsProvider {

        BindableCollection<ActionButtonViewModel> PopupControls { get; }

    }

}
