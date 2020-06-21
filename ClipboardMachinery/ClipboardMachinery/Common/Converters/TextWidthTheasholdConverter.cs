using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ClipboardMachinery.Common.Converters {

    public class TextWidthTheasholdConverter : MarkupExtension, IMultiValueConverter {

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        #endregion

        #region IValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length != 2) {
                throw new ArgumentException("Converter expected exactly two input values!");
            }

            if (!(values[0] is TextBlock textBlock)) {
                throw new ArgumentException($"Converter expected {nameof(TextBlock)} as first value type!", nameof(values));
            }

            if (!int.TryParse(parameter as string, out int theashold)) {
                throw new ArgumentException($"Converter expected {nameof(String)} representing a numeric value as parameter!", nameof(parameter));
            }

            return MeasureTextSize(textBlock).Width >= theashold;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Conversion is supported only one-way!");
        }

        #endregion

        #region Logic

        private Size MeasureTextSize(TextBlock textBlock) {
            if (string.IsNullOrEmpty(textBlock.Text)) {
                return Size.Empty;
            }

            FormattedText formattedText = new FormattedText(
                textToFormat: textBlock.Text,
                culture: CultureInfo.CurrentCulture,
                flowDirection: textBlock.FlowDirection,
                typeface: new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                emSize: textBlock.FontSize,
                foreground: textBlock.Foreground,
                pixelsPerDip: VisualTreeHelper.GetDpi(textBlock).PixelsPerDip
            );

            return new Size(formattedText.Width, formattedText.Height);
        }

        #endregion

    }

}
