using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace AiTest.converters
{
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Trace.WriteLine($"DebugConverter: value={value}, parameter={parameter}"); 
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}