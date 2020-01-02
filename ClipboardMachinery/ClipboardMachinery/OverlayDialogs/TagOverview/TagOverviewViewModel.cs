using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.TagLister;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.OverlayDialogs.TagOverview {

    public class TagOverviewViewModel : TagListerViewModel, IOverlayDialog {

        #region Properties

        public BindableCollection<ActionButtonViewModel> DialogControls {
            get;
        }

        public bool IsOpen {
            get => isOpen;
            set {
                if (isOpen == value) {
                    return;
                }

                isOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public bool AreControlsVisible {
            get => areControlsVisible;
            set {
                if (areControlsVisible == value) {
                    return;
                }

                areControlsVisible = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private bool isOpen;
        private bool areControlsVisible;

        #endregion

        public TagOverviewViewModel(ClipModel clip, IClipFactory clipFactory) : base(clip, clipFactory) {
            DialogControls = new BindableCollection<ActionButtonViewModel>();
        }

    }

}
