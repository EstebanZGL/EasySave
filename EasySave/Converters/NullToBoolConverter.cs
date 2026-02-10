using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.Converters
{
    /// <summary>
    /// Convertit une valeur null en false et une valeur non-null en true
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Convertit une valeur null en false et une valeur non-null en true
        /// </summary>
        /// <param name="value">La valeur à convertir</param>
        /// <param name="targetType">Le type cible</param>
        /// <param name="parameter">Un paramètre optionnel pour inverser la conversion</param>
        /// <param name="culture">La culture à utiliser</param>
        /// <returns>true si la valeur n'est pas null, false sinon (ou inversé si le paramètre est "invert")</returns>
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

        /// <summary>
        /// Convertit une valeur booléenne en null ou en un objet non-null
        /// </summary>
        /// <param name="value">La valeur booléenne à convertir</param>
        /// <param name="targetType">Le type cible</param>
        /// <param name="parameter">Un paramètre optionnel pour inverser la conversion</param>
        /// <param name="culture">La culture à utiliser</param>
        /// <returns>null si la valeur est false, un objet non-null sinon (ou inversé si le paramètre est "invert")</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cette méthode n'est généralement pas utilisée pour ce convertisseur
            return null;
        }
    }
}