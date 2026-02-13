using EasySave.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasySave.Views
{
    public class TranslationServiceProvider
    {
        private static TranslationService _translationService;

        public static TranslationService GetTranslationService()
        {
            if (_translationService == null)
            {
                // Accéder au service de traduction via le conteneur DI
                _translationService = ((App)App.Current).ServiceProvider.GetService<TranslationService>();
            }
            return _translationService;
        }
        
        // Cette méthode est utilisée par les bindings XAML pour obtenir les traductions
        public string GetTranslation(string key)
        {
            return GetTranslationService().GetTranslation(key);
        }
    }
}