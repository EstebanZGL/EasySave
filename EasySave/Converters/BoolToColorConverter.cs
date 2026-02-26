using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EasySave.Converters
{
     
    /// Convertit une valeur booléenne en une couleur
     
    public class BoolToColorConverter : IValueConverter
    {
        // Brosses statiques pour éviter de créer de nouvelles instances à chaque conversion
        private static readonly SolidColorBrush TrueBrush = new SolidColorBrush(Colors.LightCoral);
        private static readonly SolidColorBrush FalseBrush = new SolidColorBrush(Colors.Transparent);

         
        /// Convertit une valeur booléenne en couleur
         
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

         
        /// Convertit une couleur en valeur booléenne
         
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cette méthode n'est généralement pas utilisée pour ce convertisseur
            return null; // Retourner null au lieu de Binding.DoNothing qui n'existe pas
        }
    }
}