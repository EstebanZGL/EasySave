# Prompt

Analyse tout les fichiers du projet, surtout les documentations et le fichier suivi projet

Je veux que tu aies une vision complète du projet afin que l'on puisse travailler

Quand on codera, ce sera toi qui devra écrire les fichiers pour moi avec le builtin_write. Aussi, suis/met à jour toujours le fichier suivi_projet

J'utilise visual studio pour compiler et vscode pour coder

# 📝 Suivi du Projet EasySave (ProSoft)

## 📌 Vision Globale
Développement d'un logiciel de sauvegarde robuste pour la Suite ProSoft, évoluant de la version 1.0 (Console) à la version 3.0 (Graphique, Parallèle, Docker).
- **Client :** ProSoft (Usage interne et revente).
- **Prix unitaire :** 200 €HT
- **Contrat de maintenance annuel :** 12% prix d'achat (5/7 8-17h, mises à jour incluses, tacite reconduction, indice SYNTEC).
- **Contrainte Critique :** Code, commentaires et logs 100% en ANGLAIS.
- **Architecture :** Modulaire, maintenable, pas de duplication de code.

## 🚀 État Actuel du Développement
- **Version en cours de développement :** v3.0
- **Dernier Livrable Validé :** Version 2.0
- **Prochaine Échéance :** Livrable 3 - Version 3.0
- **Focus Actuel :** Développement des fonctionnalités avancées (sauvegardes parallèles, priorités, Docker)

## 🛠️ Stack Technique & Contraintes
- **Langage :** C# (.NET 8.0)
- **IDE :** Visual Studio 2022
- **UML :** ArgoUML
- **Versioning :** GitHub (respect du flux de travail d'équipe)
- **Architecture :** 
  - MVVM (obligatoire à partir de la v2.0)
  - DLL externe `EasyLog.dll` pour la gestion des logs.
- **Formats de données :** JSON (v1.0), puis JSON/XML (v1.1+).

## 🏗️ Design Patterns Utilisés

### EasyLog.dll
- **Strategy Pattern** - Interface `ILogger` avec implémentations spécifiques (`JsonLogger`, `XmlLogger`)
- **Factory Pattern** - `LoggerFactory` pour la création des différents types de loggers
- **Decorator Pattern** - `PerformanceLogger` pour enrichir les logs avec des métriques de performance

### BackupService
- **Dependency Injection** - Injection de `ILogger` et `StateManager` dans le constructeur
- **Template Method** - Structure de l'algorithme de sauvegarde dans `ExecuteBackupJobAsync`
- **Command Pattern** - Encapsulation des opérations de sauvegarde dans des objets `BackupJob`
- **State Pattern** - Gestion de l'état des sauvegardes via `StateManager`

### Interface Graphique (v2.0)
- **MVVM Pattern** - Séparation claire entre les modèles, vues et view models
- **Observer Pattern** - Notification des changements via INotifyPropertyChanged
- **Command Pattern** - Encapsulation des actions utilisateur dans des commandes
- **Converter Pattern** - Transformation des données pour l'affichage (NullToBoolConverter, BoolToColorConverter)

### Fonctionnalités Spéciales (v2.0)
- **Observer Pattern** - `BusinessSoftwareMonitor` pour surveiller les logiciels métier
- **Strategy Pattern** - `CryptoService` pour la stratégie de cryptage des fichiers

## 📋 Liste des Tâches par Version

### ✅ Configuration Initiale
- [x] Création de la solution Visual Studio
- [x] Création du dépôt GitHub
- [x] Invitation du tuteur sur le dépôt GitHub
- [x] Configuration des branches (main, develop, feature)
- [x] Mise en place du .gitignore pour Visual Studio
- [x] Définition des conventions de nommage et de codage

### ✅ Version 1.0 (Console)

#### Planification et Conception
- [x] Analyse des besoins détaillés
- [x] Création des diagrammes UML:
  - [x] Diagramme de cas d'utilisation
  - [x] Diagramme de classes
  - [x] Diagramme de séquence
  - [x] Diagramme d'activité
- [x] Définition de l'architecture du projet
- [x] Planification des tests

#### Structure du Projet
- [x] Création du projet principal (Application Console)
- [x] Création du projet de bibliothèque EasyLog.dll
- [x] Configuration des références entre projets
- [x] Configuration du système de build

#### Développement Core
- [x] Implémentation de la classe BackupJob
  - [x] Propriétés (Nom, Source, Cible, Type)
  - [x] Méthodes de validation
- [x] Implémentation du gestionnaire de travaux (max 5)
  - [x] Création des travaux
  - [x] Stockage des travaux
  - [x] Chargement des travaux
  - [x] Suppression des travaux
- [x] Implémentation des types de sauvegarde
  - [x] Sauvegarde complète
  - [x] Sauvegarde différentielle
- [x] Gestion des fichiers supprimés dans les sauvegardes complètes

#### Développement EasyLog.dll
- [x] Création de l'interface de logging
- [x] Implémentation du logger JSON
- [x] Gestion des fichiers journaliers (YYYY-MM-DD.json)
- [x] Méthodes pour enregistrer les actions de sauvegarde
- [x] Implémentation de design patterns (Factory, Decorator)

#### Développement État en Temps Réel
- [x] Création du gestionnaire d'état
- [x] Implémentation de la mise à jour en temps réel
- [x] Sauvegarde dans state.json
- [x] Chargement de l'état au démarrage

#### Interface Utilisateur
- [x] Implémentation du menu principal
- [x] Gestion des entrées utilisateur
- [x] Support multilingue (FR/EN)
  - [x] Système de traduction
  - [x] Fichiers de ressources
  - [x] Sélection de la langue
- [x] Gestion des travaux de sauvegarde
  - [x] Affichage de l'horodatage de la dernière sauvegarde
  - [x] Suppression des travaux

#### Ligne de Commande
- [x] Parsing des arguments (ex: EasySave.exe 1-3 ou 1;3)
- [x] Exécution automatique des travaux spécifiés
- [x] Gestion des erreurs de syntaxe

#### Tests et Validation
- [x] Tests fonctionnels de base
- [x] Tests unitaires
- [x] Tests d'intégration
- [x] Tests de performance
- [x] Tests multilingues
- [x] Validation des chemins réseau/externes

#### Documentation
- [x] Documentation utilisateur (1 page)
- [x] Documentation technique
- [x] Documentation de l'API EasyLog.dll
- [x] Commentaires du code (XML)

#### Finalisation
- [x] Compilation réussie du projet
- [x] Revue de code
- [x] Optimisation des performances
- [x] Correction des bugs
  - [x] Gestion des fichiers supprimés dans les sauvegardes complètes
- [x] Préparation du livrable
- [x] Démonstration

### ✅ Version 1.1 (Console améliorée)

#### Planification et Conception
- [x] Mise à jour des diagrammes UML
- [x] Planification de l'intégration XML

#### Développement
- [x] Mise à jour d'EasyLog.dll pour supporter XML
  - [x] Création de l'interface commune
  - [x] Implémentation du logger XML
  - [x] Factory pour sélection du format
- [x] Interface utilisateur pour sélection du format
- [x] Tests de compatibilité avec v1.0

#### Finalisation
- [x] Tests de régression
- [x] Mise à jour de la documentation
- [x] Préparation du livrable

### ✅ Version 2.0 (Interface Graphique)

#### Planification et Conception
- [x] Conception de l'interface graphique
- [x] Mise à jour des diagrammes UML pour MVVM
- [x] Planification de l'intégration de CryptoSoft

#### Structure du Projet
- [x] Création du projet WPF
- [x] Configuration de l'architecture MVVM
  - [x] Dossiers Models
  - [x] Dossiers ViewModels
  - [x] Dossiers Views

#### Développement Core
- [x] Adaptation du modèle pour nombre illimité de travaux
- [x] Intégration avec CryptoSoft
  - [x] Interface de communication
  - [x] Gestion des extensions à crypter
- [x] Détection de logiciel métier
  - [x] Création du service BusinessSoftwareMonitor
  - [x] Détection multi-méthode (processus, titres de fenêtres)
  - [x] Pause/reprise automatique des sauvegardes
- [x] Mise à jour des logs pour inclure le temps de cryptage

#### Interface Utilisateur
- [x] Création des vues principales
  - [x] Liste des travaux
  - [x] Création/Édition de travail
  - [x] Paramètres
  - [x] Exécution et suivi
- [x] Implémentation des ViewModels
- [x] Binding des données
- [x] Support multilingue dans l'interface

#### Tests et Validation
- [x] Tests unitaires
- [x] Tests d'interface utilisateur
- [x] Tests de compatibilité avec v1.0/1.1

#### Corrections et Améliorations
- [x] Correction des erreurs de compilation dans BackupJob.cs
- [x] Correction des références ambiguës dans SettingsWindow.xaml.cs
- [x] Mise à jour de l'interface ILogger dans EasyLog.dll
- [x] Adaptation de BackupService.cs pour utiliser la nouvelle interface ILogger
- [x] Correction de l'injection de dépendances dans App.xaml.cs
- [x] Ajout des convertisseurs manquants (NullToBoolConverter, BoolToColorConverter)
- [x] Résolution des problèmes d'icône d'application
- [x] Correction du problème de changement de langue dans l'interface
- [x] Correction du problème de validation du nom dans le dialogue de création de travail
- [x] Implémentation de la persistance des préférences utilisateur (langue, format de log)
- [x] Correction du problème de changement de format de log
- [x] Restructuration du stockage des logs (dossiers Logs/Json et Logs/Xml)
- [x] Amélioration de la détection du logiciel métier (multi-méthodes)
- [x] Correction des problèmes d'accès aux processus (gestion des exceptions)

#### Documentation
- [x] Mise à jour de la documentation utilisateur
- [x] Mise à jour de la documentation technique
- [x] Documentation des nouvelles fonctionnalités

#### Finalisation
- [x] Revue de code
- [x] Optimisation des performances
- [x] Tests de régression
- [x] Préparation du livrable

### 🟦 Version 3.0 (Avancé & Docker)

#### Planification et Conception
- [ ] Conception du système de sauvegarde parallèle
- [ ] Conception du service Docker
- [ ] Mise à jour des diagrammes UML

#### Développement Core
- [x] Implémentation des sauvegardes en parallèle
  - [x] Système de threading / Tâches
  - [x] Gestion des ressources partagées
- [ ] Gestion des fichiers prioritaires
  - [ ] Configuration des extensions prioritaires dans les paramètres
  - [ ] Implémentation de la règle stricte : Aucun transfert non-prioritaire si un fichier prioritaire est en attente (Globalement)
- [ ] Limitation de bande passante (Fichiers Volumineux)
  - [ ] Configuration du seuil de taille (n Ko)
  - [ ] Implémentation de la règle : Interdiction de transférer simultanément deux fichiers > n Ko
- [ ] CryptoSoft Mono-instance
  - [x] Implémentation du système de mutex (SemaphoreSlim)
  - [x] File d'attente globale pour l'accès au processus de chiffrement
- [x] Réimplémentation du mode CLI
  - [x] Création du projet EasySaveCLI
  - [x] Support des fonctionnalités de la v2.0 (cryptage, logiciel métier)
  - [x] Ajout des options de ligne de commande pour les nouvelles fonctionnalités
  - [x] Implémentation du point d'entrée (Program.cs) avec injection de dépendances
  - [x] Compilation réussie et génération de l'exécutable EasySaveCLI.exe
  - [x] Configuration des projets pour générer tous les exécutables dans un dossier commun
- [ ] Centralisation des Logs (Docker)
  - [ ] Développement du service de réception des logs (Console ou API)
  - [ ] Containerisation Docker du service
  - [ ] Implémentation des 3 modes de logging dans EasySave :
    - [ ] Local uniquement
    - [ ] Distant (Docker) uniquement
    - [ ] Local + Distant

#### Interface Utilisateur
- [x] Contrôles Play/Pause/Stop pour chaque travail
- [x] Affichage en temps réel de la progression
- [x] Vérification à la fermeture si des sauvegardes sont en cours
  - [x] Message d'avertissement en français et en anglais
  - [x] Option de confirmation pour l'utilisateur
- [ ] Interface de configuration des priorités
- [ ] Interface de configuration de la bande passante
- [ ] Interface de configuration de la centralisation des logs

#### Documentation
- [ ] Mise à jour de la documentation utilisateur
- [ ] Documentation des nouvelles fonctionnalités

#### Finalisation
- [ ] Revue de code
- [ ] Optimisation des performances
- [ ] Tests de régression
- [ ] Étude pour la v4.0 (Optimisation, bénéfice client)
- [ ] Préparation du livrable
- [ ] Préparation de la présentation finale

## 📂 Spécifications des Données (Mémoire technique)

### 1. Fichier Log Journalier (`YYYY-MM-DD.json/xml`)
Emplacement : dossier "Logs/Json" ou "Logs/Xml" dans le répertoire d'exécution.
**Champs obligatoires :**
- Timestamp
- Nom de la sauvegarde
- Source & Cible (Format UNC)
- Taille fichier & Temps de transfert (ms)
- *[v2.0+]* Temps de cryptage (ms, 0 si non, <0 si erreur)

### 2. Fichier d'État Temps Réel (`state.json`)
Emplacement : dossier racine du répertoire d'exécution.
**Champs obligatoires :**
- Nom du travail
- Timestamp dernière action
- État (Actif, Non Actif...)
- Progression (Nb fichiers total/restant, Taille totale/restante, % progression)
- Fichier en cours (Source/Dest)

### 3. Fichiers de Préférences Utilisateur
- **language.txt** : Stocke la préférence de langue (fr/en)
- **logformat.txt** : Stocke le format de log préféré (JSON/XML)
- **settings.json** : Stocke les paramètres utilisateur (logiciel métier, extensions à crypter, chemin de CryptoSoft)

### 4. Centralisation des Logs (v3.0)
- **Protocole :** TCP/IP (Sockets) ou HTTP (API REST)
- **Format :** JSON unique centralisé
- **Modes :** Local, Distant, Hybride

## Calendrier
- **Livrable 1 (Version 1.0)** : ✅ Complété
  - Diagrammes UML livrés
  - Application console fonctionnelle
  
- **Livrable 2 (Versions 1.1 et 2.0)** : ✅ Complété
  - Version 1.1 avec support XML ✅ Complété
  - Version 2.0 avec interface graphique ✅ Complété
  
- **Livrable 3 (Version 3.0)** : 🟦 En cours
  - Diagrammes UML à livrer l'avant-veille de la soutenance
  - Version finale avec toutes les fonctionnalités

## ⚠️ Règles d'or du développement
1. **Zéro Duplication :** Si une logique se répète, refactoriser immédiatement.
2. **Langue :** Tout le code (variables, fonctions) et commentaires doivent être en **Anglais**.
3. **Robustesse :** Penser aux cas d'erreurs (disque plein, réseau coupé, fichier verrouillé).
4. **CryptoSoft :** Attention, il est "Mono-instance" en v3.0, gérer les files d'attente.
5. **Compatibilité :** La DLL `EasyLog` doit rester compatible v1.0 même après update v3.0.
6. **Style de commentaires :** Les commentaires doivent être en anglais, concis et pertinents. Utiliser les balises XML pour documenter les classes et méthodes publiques, mais éviter la sur-documentation. Les commentaires doivent expliquer le "pourquoi" plutôt que le "comment" lorsque le code est suffisamment clair.
7. **Design Patterns :** Utiliser des design patterns appropriés pour améliorer la maintenabilité et l'extensibilité du code, mais éviter la sur-ingénierie.
