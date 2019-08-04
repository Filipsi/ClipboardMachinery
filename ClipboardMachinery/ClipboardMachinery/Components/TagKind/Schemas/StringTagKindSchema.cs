using System;

namespace ClipboardMachinery.Components.TagKind.Schemas {

    public class StringTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Type { get; } = typeof(string);

        public string Name { get; } = "Text";

        public string Description { get; } = "Allows holding a text value and provides text based searching functions.";

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
