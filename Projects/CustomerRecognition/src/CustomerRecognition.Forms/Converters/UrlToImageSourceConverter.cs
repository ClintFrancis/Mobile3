using System;
using Xamarin.Forms;

namespace CustomerRecognition.Forms.Converters
{
    public class UrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var url = (string)value;
                ImageSource retImageSource = ImageSource.FromUri(new Uri(url));
                return retImageSource;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
