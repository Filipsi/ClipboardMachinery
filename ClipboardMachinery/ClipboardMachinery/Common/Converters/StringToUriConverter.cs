using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClipboardMachinery.Common.Converters {

    [ValueConversion(typeof(string), typeof(Uri))]
    public class StringToUriConverter : MarkupExtension, IValueConverter {

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        #endregion

        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string link) {
                return new Uri(link);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Uri uri) {
                return uri.AbsoluteUri;
            }

            return null;
        }

        #endregion

    }

}
