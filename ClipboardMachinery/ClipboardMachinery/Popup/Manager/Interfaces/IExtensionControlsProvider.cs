using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;

namespace ClipboardMachinery.Popup.Manager.Interfaces {

    internal interface IExtensionControlsProvider {

        BindableCollection<ActionButtonViewModel> ExtensionControls { get; }

    }

}
