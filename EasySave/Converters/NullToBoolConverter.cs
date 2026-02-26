using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.Converters
{
     
    /// Convertit une valeur null en false et une valeur non-null en true
     
    public class NullToBoolConverter : IValueConverter
    {
         
        /// Convertit une valeur null en false et une valeur non-null en true
         
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value != null;
            
            // Si le paramètre est "invert", inverser le résultat
            if (parameter is string param && param.Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                result = !result;
            }
            
            return result;
        }

         
        /// Convertit une valeur booléenne en null ou en un objet non-null
         
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cette méthode n'est généralement pas utilisée pour ce convertisseur
            return null;
        }
    }
}