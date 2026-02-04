using System;
using System.Collections.Generic;

namespace EasySave.Services
{
    /// <summary>
    /// Service for handling translations
    /// </summary>
    public class TranslationService
    {
        private readonly Dictionary<string, string> _translations;
        private string _currentLanguage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultLanguage">Default language code (en or fr)</param>
        public TranslationService(string defaultLanguage = "en")
        {
            _currentLanguage = defaultLanguage.ToLower() == "fr" ? "fr" : "en";
            _translations = InitializeTranslations();
        }

        /// <summary>
        /// Get the current language
        /// </summary>
        public string CurrentLanguage => _currentLanguage;

        /// <summary>
        /// Change the current language
        /// </summary>
        /// <param name="language">Language code (en or fr)</param>
        public void ChangeLanguage(string language)
        {
            _currentLanguage = language.ToLower() == "fr" ? "fr" : "en";
        }

        /// <summary>
        /// Toggle between available languages
        /// </summary>
        public void ToggleLanguage()
        {
            _currentLanguage = _currentLanguage == "en" ? "fr" : "en";
        }

        /// <summary>
        /// Get translation for a key
        /// </summary>
        /// <param name="key">Translation key without language prefix</param>
        /// <returns>Translated text</returns>
        public string GetTranslation(string key)
        {
            string fullKey = $"{_currentLanguage}_{key}";
            return _translations.ContainsKey(fullKey) ? _translations[fullKey] : $"[{key}]";
        }

        /// <summary>
        /// Initialize translations
        /// </summary>
        /// <returns>Dictionary containing all translations</returns>
        private Dictionary<string, string> InitializeTranslations()
        {
            var translations = new Dictionary<string, string>();

            // English translations
            translations["en_app_title"] = "EasySave 1.0 - Backup Software";
            translations["en_menu_title"] = "Main Menu";
            translations["en_menu_create"] = "1. Create a new backup job";
            translations["en_menu_execute"] = "2. Execute backup job(s)";
            translations["en_menu_list"] = "3. List backup jobs";
            translations["en_menu_language"] = "4. Change language (English/Français)";
            translations["en_menu_exit"] = "5. Exit";
            translations["en_menu_choice"] = "Enter your choice: ";
            
            translations["en_create_title"] = "Create a new backup job";
            translations["en_create_name"] = "Enter a name for the backup job: ";
            translations["en_create_source"] = "Enter the source directory path: ";
            translations["en_create_target"] = "Enter the target directory path: ";
            translations["en_create_type"] = "Enter the backup type (1 for Complete, 2 for Differential): ";
            translations["en_create_success"] = "Backup job created successfully.";
            translations["en_create_error"] = "Error creating backup job. Please check your inputs.";
            translations["en_create_max_reached"] = "Maximum number of backup jobs (5) reached.";
            
            translations["en_execute_title"] = "Execute backup job(s)";
            translations["en_execute_select"] = "Enter the number(s) of the backup job(s) to execute (e.g., '1', '1-3', or '1;3'): ";
            translations["en_execute_success"] = "Backup job(s) executed successfully.";
            translations["en_execute_no_jobs"] = "No backup jobs available.";
            
            translations["en_list_title"] = "List of backup jobs";
            translations["en_list_no_jobs"] = "No backup jobs available.";
            translations["en_list_source"] = "Source: ";
            translations["en_list_target"] = "Target: ";
            translations["en_list_type"] = "Type: ";
            translations["en_list_last_backup"] = "Last backup: ";
            translations["en_list_never"] = "Never";
            translations["en_list_delete_option"] = "Type 'D' or 'delete' to delete a backup job";
            translations["en_list_back_option"] = "Press any other key to return to the main menu";
            translations["en_list_choice"] = "Enter your choice: ";
            
            translations["en_delete_select"] = "Enter the number of the backup job to delete: ";
            translations["en_delete_confirm"] = "Are you sure you want to delete the backup job '{0}'? (Y/N): ";
            translations["en_delete_success"] = "Backup job deleted successfully.";
            translations["en_delete_error"] = "Error deleting backup job.";
            translations["en_delete_cancelled"] = "Deletion cancelled.";
            translations["en_delete_invalid"] = "Invalid job number.";
            
            translations["en_language_changed"] = "Language changed to English";
            
            translations["en_press_any_key"] = "Press any key to continue...";

            // French translations
            translations["fr_app_title"] = "EasySave 1.0 - Logiciel de Sauvegarde";
            translations["fr_menu_title"] = "Menu Principal";
            translations["fr_menu_create"] = "1. Créer un nouveau travail de sauvegarde";
            translations["fr_menu_execute"] = "2. Exécuter un/des travail(aux) de sauvegarde";
            translations["fr_menu_list"] = "3. Lister les travaux de sauvegarde";
            translations["fr_menu_language"] = "4. Changer de langue (English/Français)";
            translations["fr_menu_exit"] = "5. Quitter";
            translations["fr_menu_choice"] = "Entrez votre choix : ";
            
            translations["fr_create_title"] = "Créer un nouveau travail de sauvegarde";
            translations["fr_create_name"] = "Entrez un nom pour le travail de sauvegarde : ";
            translations["fr_create_source"] = "Entrez le chemin du répertoire source : ";
            translations["fr_create_target"] = "Entrez le chemin du répertoire cible : ";
            translations["fr_create_type"] = "Entrez le type de sauvegarde (1 pour Complète, 2 pour Différentielle) : ";
            translations["fr_create_success"] = "Travail de sauvegarde créé avec succès.";
            translations["fr_create_error"] = "Erreur lors de la création du travail de sauvegarde. Veuillez vérifier vos entrées.";
            translations["fr_create_max_reached"] = "Nombre maximum de travaux de sauvegarde (5) atteint.";
            
            translations["fr_execute_title"] = "Exécuter un/des travail(aux) de sauvegarde";
            translations["fr_execute_select"] = "Entrez le(s) numéro(s) du/des travail(aux) de sauvegarde à exécuter (ex : '1', '1-3', ou '1;3') : ";
            translations["fr_execute_success"] = "Travail(aux) de sauvegarde exécuté(s) avec succès.";
            translations["fr_execute_no_jobs"] = "Aucun travail de sauvegarde disponible.";
            
            translations["fr_list_title"] = "Liste des travaux de sauvegarde";
            translations["fr_list_no_jobs"] = "Aucun travail de sauvegarde disponible.";
            translations["fr_list_source"] = "Source : ";
            translations["fr_list_target"] = "Cible : ";
            translations["fr_list_type"] = "Type : ";
            translations["fr_list_last_backup"] = "Dernière sauvegarde : ";
            translations["fr_list_never"] = "Jamais";
            translations["fr_list_delete_option"] = "Tapez 'D' ou 'delete' pour supprimer un travail de sauvegarde";
            translations["fr_list_back_option"] = "Appuyez sur une autre touche pour revenir au menu principal";
            translations["fr_list_choice"] = "Entrez votre choix : ";
            
            translations["fr_delete_select"] = "Entrez le numéro du travail de sauvegarde à supprimer : ";
            translations["fr_delete_confirm"] = "Êtes-vous sûr de vouloir supprimer le travail de sauvegarde '{0}' ? (O/N) : ";
            translations["fr_delete_success"] = "Travail de sauvegarde supprimé avec succès.";
            translations["fr_delete_error"] = "Erreur lors de la suppression du travail de sauvegarde.";
            translations["fr_delete_cancelled"] = "Suppression annulée.";
            translations["fr_delete_invalid"] = "Numéro de travail invalide.";
            
            translations["fr_language_changed"] = "Langue changée en Français";
            
            translations["fr_press_any_key"] = "Appuyez sur une touche pour continuer...";

            return translations;
        }
    }
}