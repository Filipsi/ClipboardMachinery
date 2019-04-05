using ClipboardMachinery.Components.Tag;

namespace ClipboardMachinery.Common.Events {

    public class TagEvent {

        public enum TagEventType {
            Add,
            Remove,
            ColorChange,
            ValueChange
        }

        public TagModel Source { get; }

        public TagEventType EventType { get; }

        public TagEvent(TagModel source, TagEventType eventType) {
            Source = source;
            EventType = eventType;
        }

    }

}
