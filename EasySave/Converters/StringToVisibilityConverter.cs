using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasySave.Converters
{
    /// <summary>
    /// Converts a string value to a Visibility value
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string valueString = value.ToString();
            string parameterString = parameter.ToString();

            return valueString.Equals(parameterString, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}