using Caliburn.Micro;

namespace ClipboardMachinery.Common.Events {

    public class DialogOverlayEvent {

        public enum PopupEventType {
            Open,
            Close
        }

        #region Properties

        public IScreen Popup { get; private set; }

        public PopupEventType EventType { get; private set; }

        #endregion

        private DialogOverlayEvent() {
        }

        #region Factory

        public static DialogOverlayEvent Open(IScreen popup) {
            return new DialogOverlayEvent {
                EventType = PopupEventType.Open,
                Popup = popup
            };
        }

        public static DialogOverlayEvent Close() {
            return new DialogOverlayEvent {
                EventType = PopupEventType.Close
            };
        }

        #endregion

    }

}
