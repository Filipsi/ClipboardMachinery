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

            if (!(value is Color color)) {
                throw new InvalidOperationException($"Unsupported type '{value.GetType().Name}'!");
            }

            if (byte.TryParse(parameter as string, out byte alpha)) {
                return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Conversion is supported only one-way!");
        }

        #endregion

    }

}
