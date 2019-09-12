using System;

namespace ClipboardMachinery.Core.TagKind.Impl.Schemas {

    public class TextTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Kind { get; } = typeof(string);

        public string Name { get; } = "Text";

        public string Description { get; } = "A value stored as a plain text without any restrictions.";

        public string Icon { get; } = "IconText";

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            result = value;
            return true;
        }

        public string ToPersistentValue(object value) {
            return value.ToString();
        }

        #endregion

    }

}
