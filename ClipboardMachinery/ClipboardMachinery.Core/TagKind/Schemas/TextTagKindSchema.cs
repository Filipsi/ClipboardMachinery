using System;

namespace ClipboardMachinery.Core.TagKind.Schemas {

    public class TextTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Type { get; } = typeof(string);

        public string Name { get; } = "Text";

        public string Description { get; } = "Allows holding a text value and provides text based searching options.";

        public string Icon { get; } = "IconText";

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            result = value;
            return true;
        }

        #endregion

    }

}
