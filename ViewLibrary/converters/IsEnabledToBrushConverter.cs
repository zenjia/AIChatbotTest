using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AiTest.converters
{
    public class LogTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            switch (((LogItem)value).LogType)
            {
                case LogType.Info:
                    return Brushes.Black;

                case LogType.Warn:
                    return Brushes.DarkOrange;
                case LogType.Error:
                    return Brushes.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsEnabledToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            return (bool)value ? parameter : Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}