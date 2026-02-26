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
                // Access the translation service via the DI container
                _translationService = ((App)App.Current).ServiceProvider.GetService<TranslationService>();
            }
            return _translationService;
        }
        
        // This method is used by XAML bindings to get translations
        public string GetTranslation(string key)
        {
            return GetTranslationService().GetTranslation(key);
        }
    }
}