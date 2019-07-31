using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ClipboardMachinery.Common.Converters {

    public class ColorToSolidBrushConverter : MarkupExtension, IValueConverter {

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;

        #endregion

        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch (value) {
                case Color color:
                    return new SolidColorBrush(color);

                case null:
                    return null;
            }

            throw new InvalidOperationException($"Unsupported type '{value.GetType().Name}'!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        #endregion

    }

}
