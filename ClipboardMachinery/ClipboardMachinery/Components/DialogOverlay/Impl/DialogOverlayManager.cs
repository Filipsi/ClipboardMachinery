using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Components.DialogOverlay.Impl.Portal;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Components.DialogOverlay.Impl {

    public class DialogOverlayManager : IDialogOverlayManager {

        #region Properties

        public IScreen Portal
            => dialogOverlayPortal;

        public IDialogOverlayFactory Factory {
            get;
        }

        #endregion

        #region Fields

        private readonly DialogOverlayPortalViewModel dialogOverlayPortal;

        #endregion

        public DialogOverlayManager(IDialogOverlayFactory dialogOverlayFactory, DialogOverlayPortalViewModel dialogOverlayPortal) {
            Factory = dialogOverlayFactory;
            this.dialogOverlayPortal = dialogOverlayPortal;
        }

        #region Logic

        public Task OpenDialog(IOverlayDialog dialog) {
            return dialogOverlayPortal.OpenDialog(dialog);
        }

        public Task OpenDialog<T>(Func<T> createFn, Action<T> releaseFn) where T : IOverlayDialog {
            return dialogOverlayPortal.OpenDialog(createFn, releaseFn);
        }

        public Task CloseDialog() {
            return dialogOverlayPortal.CloseDialog();
        }

        #endregion

    }

}
