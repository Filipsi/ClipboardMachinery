using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;

namespace ClipboardMachinery.Common.Events {

    public class TagEvent {

        public enum TagEventType {
            TagAdded,
            TagRemoved,
            TagValueChanged,
            TypeAdded,
            TypeColorChanged,
            TypeDescriptionChanged
        }

        #region Properties

        public TagEventType EventType { get; }

        public int? TagId { get; private set; }

        public string TagTypeName { get; private set; }

        public object Argument { get; private set; }

        #endregion

        private TagEvent(TagEventType eventType) {
            EventType = eventType;
        }

        #region Factories

        public static TagEvent CreateTagRemovedEvent(TagModel tag) {
            return new TagEvent(TagEventType.TagRemoved) {
                TagId = tag.Id,
                TagTypeName = tag.TypeName
            };
        }

        public static TagEvent CreateTagValueChangedEvent(TagModel tag) {
            return new TagEvent(TagEventType.TagValueChanged) {
                TagId = tag.Id,
                TagTypeName = tag.TypeName,
                Argument = tag.Value
            };
        }

        public static TagEvent CreateTypeAddedEvent(TagTypeModel tagType) {
            return new TagEvent(TagEventType.TypeAdded) {
                TagTypeName = tagType.Name,
                Argument = tagType
            };
        }

        public static TagEvent CreateTypeDescriptionChangedEvent(TagTypeModel tagType) {
            return new TagEvent(TagEventType.TypeDescriptionChanged) {
                TagTypeName = tagType.Name,
                Argument = tagType.Description
            };
        }

        public static TagEvent CreateTypeColorChangedEvent(TagTypeModel tagType) {
            return new TagEvent(TagEventType.TypeColorChanged) {
                TagTypeName = tagType.Name,
                Argument = tagType.Color
            };
        }

        #endregion

    }

}
