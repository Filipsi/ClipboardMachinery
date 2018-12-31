using Caliburn.Micro;
using ClipboardMachinery.Components.ActionButton;

namespace ClipboardMachinery.Popup.Manager.Interfaces {

    internal interface IExtensionControlsProvider {

        BindableCollection<ActionButtonViewModel> ExtensionControls { get; }

    }

}
