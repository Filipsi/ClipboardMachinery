using System;
using System.Globalization;

namespace ClipboardMachinery.Core.TagKind.Impl.Schemas {

    public class DateTimeTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Kind { get; } = typeof(DateTime);

        public string Name { get; } = "Date and time";

        public string Description => $"Information about date and time in '{TargetCultureInfo.DateTimeFormat.ShortDatePattern} {TargetCultureInfo.DateTimeFormat.ShortTimePattern}' format.";

        public string Icon { get; } = "IconDateTime";

        #endregion

        #region Fields

        private readonly CultureInfo TargetCultureInfo = CultureInfo.CreateSpecificCulture("cs-CZ");

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            if (DateTime.TryParse(value, TargetCultureInfo, DateTimeStyles.AllowWhiteSpaces, out DateTime parsedDateTime)) {
                result = parsedDateTime;
                return true;
            }

            result = null;
            return false;
        }

        public string ToPersistentValue(object value) {
            switch (value) {
                case DateTime dateTime:
                    return dateTime.ToString(TargetCultureInfo);

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
