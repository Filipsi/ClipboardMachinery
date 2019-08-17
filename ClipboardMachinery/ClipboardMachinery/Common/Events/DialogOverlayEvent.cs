using ClipboardMachinery.Components.DialogOverlay;

namespace ClipboardMachinery.Common.Events {

    public class DialogOverlayEvent {

        public enum PopupEventType {
            Opened,
            Closed
        }

        #region Properties

        public IOverlayDialog OverlayDialog { get; private set; }

        public PopupEventType EventType { get; private set; }

        #endregion

        private DialogOverlayEvent() {
        }

        #region Factory

        public static DialogOverlayEvent CreateOpenedEvent(IOverlayDialog overlayDialog) {
            return new DialogOverlayEvent {
                EventType = PopupEventType.Opened,
                OverlayDialog = overlayDialog
            };
        }

        public static DialogOverlayEvent CreateClosedEvent(IOverlayDialog overlayDialog) {
            return new DialogOverlayEvent {
                EventType = PopupEventType.Closed,
                OverlayDialog = overlayDialog
            };
        }

        #endregion

    }

}
