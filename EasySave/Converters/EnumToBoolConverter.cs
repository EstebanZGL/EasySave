using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.Converters
{
     
    /// Converts between enum values and boolean values for use with radio buttons
     
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Check if the enum value equals the parameter
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool valueBool = (bool)value;
            
            // If the radio button is checked, return the enum value
            if (valueBool)
                return parameter;
            
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}