# 📝 Suivi du Projet EasySave (ProSoft)

## 📌 Vision Globale
Développement d'un logiciel de sauvegarde robuste pour la Suite ProSoft, évoluant de la version 1.0 (Console) à la version 3.0 (Graphique, Parallèle, Docker).
- **Client :** ProSoft (Usage interne et revente).
- **Prix unitaire :** 200 €HT
- **Contrat de maintenance annuel :** 12% prix d'achat (5/7 8-17h, mises à jour incluses)
- **Contrainte Critique :** Code, commentaires et logs 100% en ANGLAIS.
- **Architecture :** Modulaire, maintenable, pas de duplication de code.

## 🚀 État Actuel du Développement
- **Version en cours de développement :** v1.0
- **Dernier Livrable Validé :** Aucun
- **Prochaine Échéance :** Livrable 1 - Version 1.0
- **Focus Actuel :** Tests et finalisation de la version 1.0

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
- **Strategy Pattern** - Interface `ILogger` avec implémentations spécifiques (`JsonLogger`)
- **Factory Pattern** - `LoggerFactory` pour la création des différents types de loggers
- **Decorator Pattern** - `PerformanceLogger` pour enrichir les logs avec des métriques de performance

### BackupService
- **Dependency Injection** - Injection de `ILogger` et `StateManager` dans le constructeur
- **Template Method** - Structure de l'algorithme de sauvegarde dans `ExecuteBackupJobAsync`
- **Command Pattern** - Encapsulation des opérations de sauvegarde dans des objets `BackupJob`
- **State Pattern** - Gestion de l'état des sauvegardes via `StateManager`

## 📋 Liste des Tâches par Version

### 🔄 Configuration Initiale
- [x] Création de la solution Visual Studio
- [x] Création du dépôt GitHub
- [x] Invitation du tuteur sur le dépôt GitHub
- [ ] Configuration des branches (main, develop, feature)
- [ ] Mise en place du .gitignore pour Visual Studio
- [ ] Définition des conventions de nommage et de codage

### ✅ Version 1.0 (Console)

#### Planification et Conception
- [x] Analyse des besoins détaillés
- [ ] Création des diagrammes UML:
  - [ ] Diagramme de cas d'utilisation
  - [ ] Diagramme de classes
  - [ ] Diagramme de séquence
  - [ ] Diagramme d'activité
- [x] Définition de l'architecture du projet
- [ ] Planification des tests

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

#### Ligne de Commande
- [x] Parsing des arguments (ex: EasySave.exe 1-3 ou 1;3)
- [x] Exécution automatique des travaux spécifiés
- [x] Gestion des erreurs de syntaxe

#### Tests et Validation
- [x] Tests fonctionnels de base
- [ ] Tests unitaires
- [ ] Tests d'intégration
- [ ] Tests de performance
- [ ] Tests multilingues
- [ ] Validation des chemins réseau/externes

#### Documentation
- [x] Documentation utilisateur (1 page)
- [x] Documentation technique
- [x] Documentation de l'API EasyLog.dll
- [x] Commentaires du code (XML)

#### Finalisation
- [x] Compilation réussie du projet
- [ ] Revue de code
- [ ] Optimisation des performances
- [x] Correction des bugs
  - [x] Gestion des fichiers supprimés dans les sauvegardes complètes
- [ ] Préparation du livrable
- [ ] Démonstration

### 🟦 Version 1.1 (Console améliorée)

#### Planification et Conception
- [ ] Mise à jour des diagrammes UML
- [ ] Planification de l'intégration XML

#### Développement
- [ ] Mise à jour d'EasyLog.dll pour supporter XML
  - [ ] Création de l'interface commune
  - [ ] Implémentation du logger XML
  - [ ] Factory pour sélection du format
- [ ] Interface utilisateur pour sélection du format
- [ ] Tests de compatibilité avec v1.0

#### Finalisation
- [ ] Tests de régression
- [ ] Mise à jour de la documentation
- [ ] Préparation du livrable

### 🟦 Version 2.0 (Interface Graphique)

#### Planification et Conception
- [ ] Conception de l'interface graphique
- [ ] Mise à jour des diagrammes UML pour MVVM
- [ ] Planification de l'intégration de CryptoSoft

#### Structure du Projet
- [ ] Création du projet WPF
- [ ] Configuration de l'architecture MVVM
  - [ ] Dossiers Models
  - [ ] Dossiers ViewModels
  - [ ] Dossiers Views

#### Développement Core
- [ ] Adaptation du modèle pour nombre illimité de travaux
- [ ] Intégration avec CryptoSoft
  - [ ] Interface de communication
  - [ ] Gestion des extensions à crypter
- [ ] Détection de logiciel métier
- [ ] Mise à jour des logs pour inclure le temps de cryptage

#### Interface Utilisateur
- [ ] Création des vues principales
  - [ ] Liste des travaux
  - [ ] Création/Édition de travail
  - [ ] Paramètres
  - [ ] Exécution et suivi
- [ ] Implémentation des ViewModels
- [ ] Binding des données
- [ ] Support multilingue dans l'interface

#### Tests et Validation
- [ ] Tests unitaires
- [ ] Tests d'interface utilisateur
- [ ] Tests de compatibilité avec v1.0/1.1

#### Documentation
- [ ] Mise à jour de la documentation utilisateur
- [ ] Mise à jour de la documentation technique
- [ ] Documentation des nouvelles fonctionnalités

#### Finalisation
- [ ] Revue de code
- [ ] Optimisation des performances
- [ ] Tests de régression
- [ ] Préparation du livrable

### 🟪 Version 3.0 (Avancé & Docker)

#### Planification et Conception
- [ ] Conception du système de sauvegarde parallèle
- [ ] Conception du service Docker
- [ ] Mise à jour des diagrammes UML

#### Développement Core
- [ ] Implémentation des sauvegardes en parallèle
  - [ ] Système de threading
  - [ ] Gestion des ressources partagées
- [ ] Gestion des fichiers prioritaires
  - [ ] Système de priorité par extension
  - [ ] File d'attente intelligente
- [ ] Limitation de bande passante
  - [ ] Détection des fichiers volumineux
  - [ ] Gestion des transferts simultanés
- [ ] CryptoSoft Mono-instance
  - [ ] Implémentation du système de mutex
  - [ ] File d'attente pour cryptage

#### Interface Utilisateur
- [ ] Contrôles Play/Pause/Stop pour chaque travail
- [ ] Affichage en temps réel de la progression
- [ ] Interface de configuration des priorités
- [ ] Interface de configuration de la bande passante

#### Docker
- [ ] Création du service Docker
- [ ] Système de centralisation des logs
- [ ] Interface de configuration
- [ ] Tests de communication

#### Tests et Validation
- [ ] Tests de performance parallèle
- [ ] Tests de charge
- [ ] Tests de priorité
- [ ] Tests Docker
- [ ] Tests de régression

#### Documentation
- [ ] Mise à jour de la documentation utilisateur
- [ ] Documentation Docker
- [ ] Documentation des nouvelles fonctionnalités

#### Finalisation
- [ ] Revue de code
- [ ] Optimisation des performances
- [ ] Tests de régression
- [ ] Préparation du livrable
- [ ] Préparation de la présentation finale

## 📂 Spécifications des Données (Mémoire technique)

### 1. Fichier Log Journalier (`YYYY-MM-DD.json/xml`)
Emplacement : dossier "logs" dans le répertoire d'exécution.
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

## Calendrier
- **Livrable 1 (Version 1.0)** : À venir
  - Diagrammes UML à livrer la veille
  - Application console fonctionnelle
  
- **Livrable 2 (Versions 1.1 et 2.0)** : À venir
  - Diagrammes UML à livrer la veille
  - Version 1.1 avec support XML
  - Version 2.0 avec interface graphique
  
- **Livrable 3 (Version 3.0)** : À venir
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

## Modifications et Mises à jour
- 02/02/2026 : Création du fichier de suivi et configuration initiale du projet
- 02/02/2026 : Ajout de la liste détaillée des tâches par version
- 02/02/2026 : Développement des classes principales pour la version 1.0 (BackupJob, EasyLog, StateManager, BackupService, TranslationService)
- 02/02/2026 : Compilation réussie du projet et tests fonctionnels initiaux
- 02/02/2026 : Modification de l'emplacement des logs pour utiliser un dossier local au projet
- 02/02/2026 : Création de la documentation utilisateur et technique
- 02/02/2026 : Standardisation des commentaires de code (style concis)
- 02/02/2026 : Implémentation de design patterns dans EasyLog.dll (Factory, Decorator)
- 02/02/2026 : Correction du bug de gestion des fichiers supprimés dans les sauvegardes complètes
- 02/02/2026 : Identification et documentation des design patterns utilisés dans le projet