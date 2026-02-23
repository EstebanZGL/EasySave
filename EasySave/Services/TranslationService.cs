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

        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            private set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged();
                    SaveLanguagePreference();
                }
            }
        }

        // Constructor with optional default language parameter
        public TranslationService(string defaultLanguage = "en")
        {
            _languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.txt");
            _currentLanguage = LoadLanguagePreference() ?? (defaultLanguage.ToLower() == "fr" ? "fr" : "en");
            _translations = InitializeTranslations();
        }

        // Toggle between English and French
        public void ToggleLanguage()
        {
            CurrentLanguage = _currentLanguage == "en" ? "fr" : "en";
            // Notifier que toutes les traductions ont potentiellement changÃ©
            OnPropertyChanged(nameof(CurrentLanguage));
            NotifyTranslationsChanged();
        }

        // Set language explicitly
        public void SetLanguage(string language)
        {
            if (language != "en" && language != "fr")
                throw new ArgumentException("Language must be 'en' or 'fr'", nameof(language));

            CurrentLanguage = language;
            // Notifier que toutes les traductions ont potentiellement changÃ©
            OnPropertyChanged(nameof(CurrentLanguage));
            NotifyTranslationsChanged();
        }
        
        // Notifier que toutes les traductions ont changÃ©
        private void NotifyTranslationsChanged()
        {
            // DÃ©clencher un Ã©vÃ©nement spÃ©cial pour indiquer que toutes les traductions ont changÃ©
            OnPropertyChanged("AllTranslations");
        }

        // Get translation for a key
        public string GetTranslation(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

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
            translations["en_app_title"] = "EasySave 3.0 - Backup Software";
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
            translations["en_description"] = "Description:";
            translations["en_job_status"] = "Job Status";
            translations["en_not_running"] = "Not running";
            translations["en_log_format"] = "Log Format:";
            translations["en_status_ready"] = "Ready";
            translations["en_last_backup"] = "Last Backup:";
            translations["en_created"] = "Created:";
            
            // Ajout des traductions pour BackupJobDialog
            translations["en_create_backup_job"] = "Create Backup Job";
            translations["en_edit_backup_job"] = "Edit Backup Job";
            translations["en_browse"] = "Browse";
            translations["en_language"] = "Language:";
            translations["en_language_english"] = "English";
            translations["en_language_french"] = "French";
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
            
            // Ajout des traductions pour la sÃ©lection multiple
            translations["en_select_all"] = "Select All";
            translations["en_execute_selected"] = "Execute Selected Jobs";
            
            // Ajout des traductions pour les paramÃ¨tres
            translations["en_business_software"] = "Business Software:";
            translations["en_cryptosoft_path"] = "CryptoSoft Path:";
            translations["en_encrypt_extensions"] = "Encrypt Extensions:";
            translations["en_max_parallel_jobs"] = "Max Parallel Jobs:";
            translations["en_log_format_setting"] = "Log Format:";
            translations["en_restart_required"] = "* Requires application restart";
            translations["en_settings_notes"] = "Notes:";
            translations["en_settings_business_note"] = "- Business Software: EasySave will pause backups when this application is running";
            translations["en_settings_encrypt_note"] = "- Encrypt Extensions: File types that will be encrypted using CryptoSoft";
            translations["en_settings_log_note"] = "- Log Format: Changes to log format require application restart to take effect";
            translations["en_settings_changes_note"] = "- Changes are saved automatically";
            translations["en_close"] = "Close";
            
            // Ajout des traductions pour les boutons de contrÃ´le des travaux
            translations["en_pause"] = "Pause";
            translations["en_resume"] = "Resume";
            translations["en_stop"] = "Stop";
            translations["en_priority_extensions"] = "Priority Extensions:";
            translations["en_large_file_threshold"] = "Large File Threshold (Megabytes):";
            translations["en_business_software_running"] = "Business software is running. Backups are paused.";
            
            // Ajout des traductions pour les sections des paramÃ¨tres
            translations["en_general_settings"] = "General Settings";
            translations["en_parallel_backup_settings"] = "Parallel Backup Settings";
            translations["en_encryption_settings"] = "Encryption Settings";
            translations["en_log_centralization_settings"] = "Log Centralization Settings";
            
            // Ajout des traductions pour les paramÃ¨tres de centralisation des logs
            translations["en_enable_centralization"] = "Enable Centralization:";
            translations["en_log_server_url"] = "Log Server URL:";
            translations["en_log_destination"] = "Log Destination:";
            translations["en_local_only"] = "Local Only";
            translations["en_remote_only"] = "Remote Only";
            translations["en_both_local_remote"] = "Both Local and Remote";
            
            // Ajout des traductions pour les notes des paramÃ¨tres
            translations["en_max_parallel_jobs_note"] = "- Max Parallel Jobs: Maximum number of backup jobs that can run simultaneously";
            translations["en_large_file_threshold_note"] = "- Large File Threshold: Files larger than this size cannot be transferred simultaneously";
            translations["en_priority_extensions_note"] = "- Priority Extensions: File types that will be processed before any other files";
            translations["en_log_centralization_note"] = "- Log Centralization: Configure where log files are stored (local, remote server, or both)";
            
            // Ajout des traductions pour la barre d'avancement
            translations["en_current_file"] = "Current File:";
            translations["en_progress"] = "Progress:";

            // French translations
            translations["fr_app_title"] = "EasySave 3.0 - Logiciel de Sauvegarde";
            translations["fr_menu_title"] = "Menu Principal";
            translations["fr_menu_create"] = "Créer un Nouveau Travail";
            translations["fr_menu_execute"] = "Exécuter la Sauvegarde";
            translations["fr_menu_edit"] = "Modifier";
            translations["fr_menu_delete"] = "Supprimer";
            translations["fr_menu_settings"] = "Paramètres";
            translations["fr_backup_jobs"] = "Travaux de sauvegarde";
            translations["fr_backup_job_details"] = "Détails du travail";
            translations["fr_name"] = "Nom:";
            translations["fr_source_path"] = "Chemin source:";
            translations["fr_target_path"] = "Chemin cible:";
            translations["fr_type"] = "Type:";
            translations["fr_description"] = "Description:";
            translations["fr_job_status"] = "État du travail";
            translations["fr_not_running"] = "Non en cours";
            translations["fr_log_format"] = "Format de log:";
            translations["fr_status_ready"] = "Prêt";
            translations["fr_last_backup"] = "Dernière sauvegarde:";
            translations["fr_created"] = "Créé le:";

            // BackupJobDialog
            translations["fr_create_backup_job"] = "Créer un travail de sauvegarde";
            translations["fr_edit_backup_job"] = "Modifier un travail de sauvegarde";
            translations["fr_browse"] = "Parcourir";
            translations["fr_language"] = "Langue:";
            translations["fr_language_english"] = "Anglais";
            translations["fr_language_french"] = "Français";
            translations["fr_complete"] = "Complète";
            translations["fr_differential"] = "Différentielle";
            translations["fr_save"] = "Enregistrer";
            translations["fr_cancel"] = "Annuler";

            // Validation messages
            translations["fr_name_required"] = "Le nom est requis.";
            translations["fr_source_required"] = "Le chemin source est requis.";
            translations["fr_source_not_exist"] = "Le répertoire source n'existe pas.";
            translations["fr_target_required"] = "Le chemin cible est requis.";
            translations["fr_target_error"] = "Erreur lors de la création du répertoire cible: {0}";

            // Multi-selection
            translations["fr_select_all"] = "Tout sélectionner";
            translations["fr_execute_selected"] = "Exécuter les sélectionnés";

            // Settings
            translations["fr_business_software"] = "Logiciel métier:";
            translations["fr_cryptosoft_path"] = "Chemin CryptoSoft:";
            translations["fr_encrypt_extensions"] = "Extensions à chiffrer:";
            translations["fr_max_parallel_jobs"] = "Travaux parallèles max:";
            translations["fr_log_format_setting"] = "Format des logs:";
            translations["fr_restart_required"] = "* Nécessite un redémarrage de l'application";
            translations["fr_settings_notes"] = "Notes:";
            translations["fr_settings_business_note"] = "- Logiciel métier: EasySave mettra en pause les sauvegardes quand cette application est en cours d'exécution";
            translations["fr_settings_encrypt_note"] = "- Extensions à chiffrer: types de fichiers qui seront chiffrés avec CryptoSoft";
            translations["fr_settings_log_note"] = "- Format des logs: les changements de format nécessitent un redémarrage pour prendre effet";
            translations["fr_settings_changes_note"] = "- Les changements sont sauvegardés automatiquement";
            translations["fr_close"] = "Fermer";

            // Job control buttons
            translations["fr_pause"] = "Pause";
            translations["fr_resume"] = "Reprendre";
            translations["fr_stop"] = "Arrêter";
            translations["fr_priority_extensions"] = "Extensions prioritaires:";
            translations["fr_large_file_threshold"] = "Seuil fichier volumineux (octets):";
            translations["fr_business_software_running"] = "Le logiciel métier est en cours d'exécution. Les sauvegardes sont en pause.";

            // Settings sections
            translations["fr_general_settings"] = "Paramètres généraux";
            translations["fr_parallel_backup_settings"] = "Paramètres de sauvegarde parallèle";
            translations["fr_encryption_settings"] = "Paramètres de chiffrement";
            translations["fr_log_centralization_settings"] = "Paramètres de centralisation des logs";

            // Log centralization settings
            translations["fr_enable_centralization"] = "Activer la centralisation:";
            translations["fr_log_server_url"] = "URL du serveur de logs:";
            translations["fr_log_destination"] = "Destination des logs:";
            translations["fr_local_only"] = "Local uniquement";
            translations["fr_remote_only"] = "Distant uniquement";
            translations["fr_both_local_remote"] = "Local et distant";

            // Settings notes
            translations["fr_max_parallel_jobs_note"] = "- Travaux parallèles max: nombre maximum de travaux de sauvegarde pouvant s'exécuter simultanément";
            translations["fr_large_file_threshold_note"] = "- Seuil fichier volumineux: les fichiers plus grands que cette taille ne peuvent pas être transférés simultanément";
            translations["fr_priority_extensions_note"] = "- Extensions prioritaires: types de fichiers traités avant tous les autres fichiers";
            translations["fr_log_centralization_note"] = "- Centralisation des logs: configurer où les fichiers de log sont stockés (local, serveur distant, ou les deux)";

            // Progress bar
            translations["fr_current_file"] = "Fichier actuel:";
            translations["fr_progress"] = "Progression:";

            return translations;
        }
        
        // Save language preference to file
        private void SaveLanguagePreference()
        {
            try
            {
                File.WriteAllText(_languageFilePath, _currentLanguage);
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"Error saving language preference: {ex.Message}");
            }
        }
        
        // Load language preference from file
        private string LoadLanguagePreference()
        {
            try
            {
                if (File.Exists(_languageFilePath))
                {
                    string language = File.ReadAllText(_languageFilePath).Trim().ToLower();
                    if (language == "en" || language == "fr")
                    {
                        return language;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default
                Console.WriteLine($"Error loading language preference: {ex.Message}");
            }
            
            return null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

