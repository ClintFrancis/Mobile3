using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CustomerRecognition.Forms.Converters
{
    class DateToElapsedTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var date = (DateTime)value;

                var ts = DateTime.Now.Subtract(date);
                string result = "";
                if (ts.Hours > 0)
                    result = String.Format("{0:00}h {1:00}m {2:00}s", ts.Hours, ts.Minutes, ts.Seconds);
                else
                    result = String.Format("{0:00}m {1:00}s", ts.Minutes, ts.Seconds);

                return result;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
