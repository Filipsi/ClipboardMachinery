using System;
using System.Globalization;

namespace ClipboardMachinery.Core.TagKind.Schemas {

    public class NumericTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Kind { get; } = typeof(decimal);

        public string Name { get; } = "Numeric";

        public string Description { get; } = "Allows holding a decimal value and provides mathematical search options.";

        public string Icon { get; } = "IconNumeric";

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            bool isSucessfulyParsed = decimal.TryParse(
                s: value,
                style: NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite,
                provider: CultureInfo.InvariantCulture,
                result: out decimal numericValue
            );

            result = numericValue;
            return isSucessfulyParsed;
        }

        public string ToDisplayValue(object value) {
            return value is decimal decimalValue
                ? decimalValue.ToString(CultureInfo.InvariantCulture)
                : value.ToString();
        }

        #endregion

    }

}
