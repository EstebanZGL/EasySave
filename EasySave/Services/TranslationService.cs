using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace EasySave.Services
{
    // Service for handling translations
    public class TranslationService : INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> _translations;
        private string _currentLanguage;
        private readonly string _languageFilePath;
        
        // Event pour notifier les changements de propriété
        public event PropertyChangedEventHandler? PropertyChanged;

        // Constructor with optional default language parameter
        public TranslationService(string defaultLanguage = "en")
        {
            _languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.txt");
            _currentLanguage = LoadLanguagePreference() ?? (defaultLanguage.ToLower() == "fr" ? "fr" : "en");
            _translations = InitializeTranslations();
        }

        // Get the current language
        public string CurrentLanguage => _currentLanguage;

        // Change the current language
        public void ChangeLanguage(string language)
        {
            _currentLanguage = language.ToLower() == "fr" ? "fr" : "en";
            SaveLanguagePreference();
            // Notifier que toutes les traductions ont potentiellement changé
            OnPropertyChanged(nameof(CurrentLanguage));
            NotifyTranslationsChanged();
        }

        // Toggle between available languages
        public void ToggleLanguage()
        {
            _currentLanguage = _currentLanguage == "en" ? "fr" : "en";
            SaveLanguagePreference();
            // Notifier que toutes les traductions ont potentiellement changé
            OnPropertyChanged(nameof(CurrentLanguage));
            NotifyTranslationsChanged();
        }
        
        // Sauvegarder la préférence de langue
        private void SaveLanguagePreference()
        {
            try
            {
                File.WriteAllText(_languageFilePath, _currentLanguage);
            }
            catch (Exception ex)
            {
                // Log l'erreur mais continue l'exécution
                System.Diagnostics.Debug.WriteLine($"Error saving language preference: {ex.Message}");
            }
        }
        
        // Charger la préférence de langue
        private string LoadLanguagePreference()
        {
            try
            {
                if (File.Exists(_languageFilePath))
                {
                    string language = File.ReadAllText(_languageFilePath).Trim().ToLower();
                    if (language == "fr" || language == "en")
                    {
                        return language;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur mais continue l'exécution
                System.Diagnostics.Debug.WriteLine($"Error loading language preference: {ex.Message}");
            }
            
            return null;
        }
        
        // Notifier que toutes les traductions ont changé
        private void NotifyTranslationsChanged()
        {
            // Déclencher un événement spécial pour indiquer que toutes les traductions ont changé
            OnPropertyChanged("AllTranslations");
        }

        // Get translation for a key
        public string GetTranslation(string key)
        {
            string fullKey = $"{_currentLanguage}_{key}";
            // Use TryGetValue for better performance than ContainsKey + indexer
            return _translations.TryGetValue(fullKey, out string translation) ? translation : $"[{key}]";
        }

        // Initialize translations dictionary
        private Dictionary<string, string> InitializeTranslations()
        {
            // Use initial capacity to avoid resizing
            var translations = new Dictionary<string, string>(100);

            // English translations
            translations["en_app_title"] = "EasySave 2.0 - Backup Software";
            translations["en_menu_title"] = "Main Menu";
            translations["en_menu_create"] = "Create New Backup Job";
            translations["en_menu_execute"] = "Execute Backup";
            translations["en_menu_edit"] = "Edit";
            translations["en_menu_delete"] = "Delete";
            translations["en_menu_settings"] = "Settings";
            translations["en_backup_jobs"] = "Backup Jobs";
            translations["en_backup_job_details"] = "Backup Job Details";
            translations["en_name"] = "Name:";
            translations["en_source_path"] = "Source Path:";
            translations["en_target_path"] = "Target Path:";
            translations["en_type"] = "Type:";
            translations["en_job_status"] = "Job Status";
            translations["en_not_running"] = "Not running";
            translations["en_log_format"] = "Log Format:";
            translations["en_status_ready"] = "Ready";
            translations["en_last_backup"] = "Last Backup:";
            
            // Ajout des traductions pour BackupJobDialog
            translations["en_create_backup_job"] = "Create Backup Job";
            translations["en_edit_backup_job"] = "Edit Backup Job";
            translations["en_browse"] = "Browse";
            translations["en_complete"] = "Complete";
            translations["en_differential"] = "Differential";
            translations["en_save"] = "Save";
            translations["en_cancel"] = "Cancel";
            
            // Ajout des traductions pour les messages de validation
            translations["en_name_required"] = "Name is required.";
            translations["en_source_required"] = "Source path is required.";
            translations["en_source_not_exist"] = "Source directory does not exist.";
            translations["en_target_required"] = "Target path is required.";
            translations["en_target_error"] = "Error creating target directory: {0}";
            
            // Ajout des traductions pour la sélection multiple
            translations["en_select_all"] = "Select All";
            translations["en_execute_selected"] = "Execute Selected Jobs";
            
            // French translations
            translations["fr_app_title"] = "EasySave 2.0 - Logiciel de Sauvegarde";
            translations["fr_menu_title"] = "Menu Principal";
            translations["fr_menu_create"] = "Créer un Nouveau Travail";
            translations["fr_menu_execute"] = "Exécuter la Sauvegarde";
            translations["fr_menu_edit"] = "Modifier";
            translations["fr_menu_delete"] = "Supprimer";
            translations["fr_menu_settings"] = "Paramètres";
            translations["fr_backup_jobs"] = "Travaux de Sauvegarde";
            translations["fr_backup_job_details"] = "Détails du Travail";
            translations["fr_name"] = "Nom:";
            translations["fr_source_path"] = "Chemin Source:";
            translations["fr_target_path"] = "Chemin Cible:";
            translations["fr_type"] = "Type:";
            translations["fr_job_status"] = "État du Travail";
            translations["fr_not_running"] = "Non en cours";
            translations["fr_log_format"] = "Format de Log:";
            translations["fr_status_ready"] = "Prêt";
            translations["fr_last_backup"] = "Dernière Sauvegarde:";
            
            // Ajout des traductions pour BackupJobDialog
            translations["fr_create_backup_job"] = "Créer un Travail de Sauvegarde";
            translations["fr_edit_backup_job"] = "Modifier un Travail de Sauvegarde";
            translations["fr_browse"] = "Parcourir";
            translations["fr_complete"] = "Complète";
            translations["fr_differential"] = "Différentielle";
            translations["fr_save"] = "Enregistrer";
            translations["fr_cancel"] = "Annuler";
            
            // Ajout des traductions pour les messages de validation
            translations["fr_name_required"] = "Le nom est requis.";
            translations["fr_source_required"] = "Le chemin source est requis.";
            translations["fr_source_not_exist"] = "Le répertoire source n'existe pas.";
            translations["fr_target_required"] = "Le chemin cible est requis.";
            translations["fr_target_error"] = "Erreur lors de la création du répertoire cible: {0}";
            
            // Ajout des traductions pour la sélection multiple
            translations["fr_select_all"] = "Tout Sélectionner";
            translations["fr_execute_selected"] = "Exécuter les Sélectionnés";

            return translations;
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}