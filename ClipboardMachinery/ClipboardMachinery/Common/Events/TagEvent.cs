namespace ClipboardMachinery.Common.Events {

    public class TagEvent {

        public enum TagEventType {
            Add,
            Remove,
            ColorChange,
            ValueChange
        }

        public TagEventType EventType { get; }

        public int? TagId { get; }

        public string TagTypeName { get; }

        public object Argument { get; }

        public TagEvent(TagEventType eventType, int tagId, string tagTypeName, object argument = null) {
            EventType = eventType;
            TagId = tagId;
            TagTypeName = tagTypeName;
            Argument = argument;
        }

        public TagEvent(TagEventType eventType, string tagTypeName, object argument = null) {
            EventType = eventType;
            TagTypeName = tagTypeName;
            Argument = argument;
        }

    }

}
