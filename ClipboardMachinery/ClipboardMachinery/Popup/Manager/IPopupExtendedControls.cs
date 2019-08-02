using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Popup.Manager {

    internal interface IPopupExtendedControls {

        BindableCollection<ActionButtonViewModel> PopupControls { get; }

    }

}
