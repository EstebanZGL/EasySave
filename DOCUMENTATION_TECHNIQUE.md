# Documentation Technique EasySave v1.0

## Architecture générale

EasySave est une application de sauvegarde développée en C# (.NET 8.0) suivant une architecture modulaire. Le projet est divisé en deux composants principaux :

1. **EasySave** : Application console principale
2. **EasyLog** : Bibliothèque de classes pour la gestion des logs

## Structure du projet

```
EasySave/
├── Models/
│   └── BackupJob.cs           # Modèle de données pour les travaux de sauvegarde
├── Services/
│   ├── BackupJobManager.cs    # Gestion des travaux (CRUD, persistance)
│   ├── BackupService.cs       # Exécution des sauvegardes
│   ├── StateManager.cs        # Gestion de l'état en temps réel
│   └── TranslationService.cs  # Gestion du multilinguisme
├── EasySaveController.cs      # Contrôleur principal de l'application
└── Program.cs                 # Point d'entrée de l'application

EasyLog/
├── ILogger.cs                 # Interface pour les loggers
└── JsonLogger.cs              # Implémentation JSON du logger
```

## Composants principaux

### BackupJob

Représente un travail de sauvegarde avec les propriétés suivantes :
- **Name** : Nom du travail
- **SourcePath** : Chemin source des fichiers à sauvegarder
- **TargetPath** : Chemin cible où sauvegarder les fichiers
- **Type** : Type de sauvegarde (Complète ou Différentielle)

### BackupJobManager

Gère la création, le stockage, le chargement et la suppression des travaux de sauvegarde. Les travaux sont persistés dans un fichier JSON (`config.json`).

Limitations :
- Maximum 5 travaux de sauvegarde
- Noms de travaux uniques

### BackupService

Service responsable de l'exécution des sauvegardes :
- Sauvegarde complète : Copie tous les fichiers du dossier source vers le dossier cible
- Sauvegarde différentielle : Copie uniquement les fichiers modifiés depuis la dernière sauvegarde complète

### StateManager

Gère l'état en temps réel des travaux de sauvegarde, stocké dans `state.json`. Pour chaque travail en cours, il maintient :
- Nom du travail
- État actuel (Actif, Non Actif)
- Progression (fichiers traités/total, taille traitée/totale)
- Fichier en cours de traitement

### TranslationService

Gère le support multilingue (FR/EN) de l'application.

### EasyLog

Bibliothèque responsable de la journalisation des opérations de sauvegarde. Les logs sont stockés au format JSON dans des fichiers journaliers (`YYYY-MM-DD.json`).

## Fichiers de données

### config.json

Stocke la configuration des travaux de sauvegarde :

```json
[
  {
    "Name": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents",
    "TargetPath": "D:\\Backup\\Documents",
    "Type": 1
  },
  ...
]
```

### state.json

Stocke l'état en temps réel des travaux de sauvegarde :

```json
[
  {
    "Name": "Documents",
    "State": "Active",
    "TotalFiles": 100,
    "TotalFilesRemaining": 75,
    "TotalSize": 1048576,
    "TotalSizeRemaining": 786432,
    "Progress": 25,
    "CurrentFile": "C:\\Users\\Username\\Documents\\file.txt"
  },
  ...
]
```

### YYYY-MM-DD.json

Fichiers de log journaliers :

```json
[
  {
    "Timestamp": "2026-02-02T12:34:56.789Z",
    "BackupName": "Documents",
    "SourcePath": "C:\\Users\\Username\\Documents\\file.txt",
    "TargetPath": "D:\\Backup\\Documents\\file.txt",
    "FileSize": 1024,
    "TransferTime": 15
  },
  ...
]
```

## Guide de dépannage

### Erreurs courantes

1. **Erreur d'accès aux fichiers**
   - Vérifier les permissions des dossiers source et cible
   - Vérifier si des fichiers sont verrouillés par d'autres applications

2. **Erreur de sérialisation JSON**
   - Vérifier l'intégrité des fichiers config.json et state.json
   - En cas de corruption, supprimer les fichiers (ils seront recréés)

3. **Erreur "Out of memory"**
   - Se produit lors de la sauvegarde de très gros fichiers
   - Recommander à l'utilisateur de diviser les travaux en plus petites unités

### Logs d'erreur

Les erreurs de transfert sont indiquées dans les logs par un temps de transfert négatif. Par exemple, un temps de transfert de `-1` indique une erreur d'accès au fichier.

## Limites connues

1. Pas de cryptage des fichiers
2. Maximum 5 travaux de sauvegarde
3. Pas de sauvegarde en parallèle
4. Pas de reprise après interruption
5. Pas de compression des fichiers

## Évolutions prévues

1. **Version 1.1** : Support du format XML pour les logs
2. **Version 2.0** : Interface graphique WPF avec architecture MVVM
3. **Version 3.0** : Sauvegardes parallèles, gestion des priorités et conteneurisation Docker