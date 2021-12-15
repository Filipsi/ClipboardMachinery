using System;
using System.Globalization;

namespace ClipboardMachinery.Core.TagKind.Impl.Schemas {

    public class NumericTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Kind { get; } = typeof(decimal);

        public string Name { get; } = "Numeric";

        public string Description { get; } = "A value stored as number allowing for decimal places if desired.";

        public string Icon { get; } = "IconNumeric";

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            bool isSuccessfullyParsed = decimal.TryParse(
                s: value,
                style: NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite,
                provider: CultureInfo.InvariantCulture,
                result: out decimal numericValue
            );

            result = numericValue;
            return isSuccessfullyParsed;
        }

        public string ToPersistentValue(object value) {
            switch (value) {
                case decimal decimalValue:
                    return decimalValue.ToString(CultureInfo.InvariantCulture);

                case string textValue:
                    if (TryParse(textValue, out object result)) {
                        // ReSharper disable once TailRecursiveCall
                        return ToPersistentValue(result);
                    }
                    break;
            }

            return string.Empty;
        }

        #endregion

    }

}
