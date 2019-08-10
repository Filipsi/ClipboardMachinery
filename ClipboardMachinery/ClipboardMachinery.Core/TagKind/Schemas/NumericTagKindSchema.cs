using System;
using System.Globalization;
using ClipboardMachinery.Core.TagKind;

namespace ClipboardMachinery.Components.TagKind.Schemas {

    public class NumericTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Type { get; } = typeof(decimal);

        public string Name { get; } = "Numeric";

        public string Description { get; } = "Allows holding a decimal value and provides mathematical search options.";

        public string Icon { get; } = "IconNumeric";

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            bool isSucessfulyParsed =  decimal.TryParse(
                s: value,
                style: NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite,
                provider: CultureInfo.CurrentUICulture,
                result: out decimal numericValue
            );

            result = numericValue;
            return isSucessfulyParsed;
        }

        #endregion

    }

}
