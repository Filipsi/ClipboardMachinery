using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ClipboardMachinery.Common.Converters {

    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class ColorToSolidBrushConverter : MarkupExtension, IValueConverter {

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        #endregion

        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return null;
            }

            if (value is Color color) {
                return new SolidColorBrush(color);
            }

            throw new InvalidOperationException($"Unsupported type '{value.GetType().Name}'!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Conversion is supported only one-way!");
        }

        #endregion

    }

}
