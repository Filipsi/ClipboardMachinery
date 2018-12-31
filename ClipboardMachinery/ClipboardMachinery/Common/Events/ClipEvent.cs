using ClipboardMachinery.Components.Clip;

namespace ClipboardMachinery.Common.Events {

    public class ClipEvent {

        public enum ClipEventType {
            Created,
            Remove,
            Select
        }

        public ClipModel Source { get; }

        public ClipEventType EventType { get; }

        public ClipEvent(ClipModel source, ClipEventType eventType) {
            Source = source;
            EventType = eventType;
        }

    }

}
