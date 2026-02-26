using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.Converters
{
     
    /// Converts a boolean value to a string based on the provided parameter
    /// Format: "TrueValue|FalseValue"
     
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return "Pause"; // Default to Pause if not a boolean

            if (!(parameter is string paramString))
                return boolValue ? "Resume" : "Pause";

            string[] parts = paramString.Split('|');
            if (parts.Length != 2)
                return boolValue ? "Resume" : "Pause";

            return boolValue ? parts[0] : parts[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}