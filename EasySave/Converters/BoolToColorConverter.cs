using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EasySave.Converters
{
    /// <summary>
    /// Convertit une valeur booléenne en une couleur
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        // Brosses statiques pour éviter de créer de nouvelles instances à chaque conversion
        private static readonly SolidColorBrush TrueBrush = new SolidColorBrush(Colors.LightCoral);
        private static readonly SolidColorBrush FalseBrush = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// Convertit une valeur booléenne en couleur
        /// </summary>
        /// <param name="value">La valeur booléenne à convertir</param>
        /// <param name="targetType">Le type cible</param>
        /// <param name="parameter">Un paramètre optionnel pour inverser la conversion</param>
        /// <param name="culture">La culture à utiliser</param>
        /// <returns>Rouge si true, Transparent si false (ou inversé si le paramètre est "invert")</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            
            // Si le paramètre est "invert", inverser la valeur
            if (parameter is string param && param.Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }
            
            // Rouge si true, Transparent si false
            return boolValue ? TrueBrush : FalseBrush;
        }

        /// <summary>
        /// Convertit une couleur en valeur booléenne
        /// </summary>
        /// <param name="value">La couleur à convertir</param>
        /// <param name="targetType">Le type cible</param>
        /// <param name="parameter">Un paramètre optionnel pour inverser la conversion</param>
        /// <param name="culture">La culture à utiliser</param>
        /// <returns>true si la couleur est rouge, false sinon (ou inversé si le paramètre est "invert")</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cette méthode n'est généralement pas utilisée pour ce convertisseur
            return null; // Retourner null au lieu de Binding.DoNothing qui n'existe pas
        }
    }
}