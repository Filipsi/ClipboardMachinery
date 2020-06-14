using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace ClipboardMachinery.Common.Converters {

    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class Base64ToImageConverter : MarkupExtension, IValueConverter {

        #region Fields

        private static readonly Regex imageDataPattern = new Regex(
            pattern: @"^data\:(?<visiblityChangeType>image\/(png|tiff|jpg|gif|bmp|webp));base64,(?<data>[A-Z0-9\+\/\=]+)$",
            options: RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );

        #endregion

        #region MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        #endregion

        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is string text)) {
                throw new NotSupportedException("Expected string as value type!");
            }

            Match result = imageDataPattern.Match(text);

            if (!result.Success) {
                return null;
            }

            string base64data = result.Groups[2].Value;
            byte[] bytes = System.Convert.FromBase64String(base64data);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.StreamSource = new MemoryStream(bytes);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Conversion is supported only one-way!");
        }

        #endregion

    }
}
