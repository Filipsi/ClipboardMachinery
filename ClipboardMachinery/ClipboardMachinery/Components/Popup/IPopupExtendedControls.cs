using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Components.Popup {

    internal interface IPopupExtendedControls {

        BindableCollection<ActionButtonViewModel> PopupControls { get; }

    }

}
