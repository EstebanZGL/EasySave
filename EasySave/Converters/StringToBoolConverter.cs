using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string valueString = value.ToString();
            string parameterString = parameter.ToString();

            return valueString.Equals(parameterString, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool valueBool = (bool)value;
            string parameterString = parameter.ToString();

            return valueBool ? parameterString : null;
        }
    }
}