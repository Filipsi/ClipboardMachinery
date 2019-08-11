using Caliburn.Micro;
using ClipboardMachinery.Components.DialogOverlay.Impl.Portal;

namespace ClipboardMachinery.Components.DialogOverlay.Impl {

    public class DialogOverlayManager : IDialogOverlayManager {

        #region Properties

        public IScreen DialogOverlay
            => dialogOverlayPortal;

        #endregion

        #region Fields

        private readonly DialogOverlayPortalViewModel dialogOverlayPortal;

        #endregion

        public DialogOverlayManager(DialogOverlayPortalViewModel dialogOverlayPortal) {
            this.dialogOverlayPortal = dialogOverlayPortal;
        }

    }

}
