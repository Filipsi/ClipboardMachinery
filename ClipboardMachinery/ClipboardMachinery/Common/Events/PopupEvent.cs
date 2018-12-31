using Caliburn.Micro;

namespace ClipboardMachinery.Common.Events {

    public class PopupEvent {

        public enum PopupEventType {
            Show,
            Close
        }

        #region Properties

        public IScreen Popup { get; private set; }

        public PopupEventType EventType { get; private set; }

        #endregion

        private PopupEvent() {
        }

        #region Factory

        public static PopupEvent Show(IScreen popup) {
            return new PopupEvent {
                EventType = PopupEventType.Show,
                Popup = popup
            };
        }

        public static PopupEvent Close() {
            return new PopupEvent {
                EventType = PopupEventType.Close
            };
        }

        #endregion

    }

}
