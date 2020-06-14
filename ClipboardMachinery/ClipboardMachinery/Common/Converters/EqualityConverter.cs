using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClipboardMachinery.Common.Converters {

    public class EqualityConverter : MarkupExtension, IMultiValueConverter {

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        #endregion

        #region IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Length >= 2 && values[0].Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Conversion is supported only one-way!");
        }

        #endregion

    }

}
