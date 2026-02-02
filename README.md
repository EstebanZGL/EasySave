# EasySave

## Présentation

EasySave est une solution de sauvegarde robuste et évolutive développée par ProSoft. Cette application console permet de créer et d'exécuter jusqu'à 5 travaux de sauvegarde différents, avec support des sauvegardes complètes et différentielles.

![Version](https://img.shields.io/badge/version-1.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Langage](https://img.shields.io/badge/langage-C%23-green)

## Fonctionnalités

- **Gestion de travaux de sauvegarde** : Création, exécution et suivi de jusqu'à 5 travaux
- **Types de sauvegarde** : Support des sauvegardes complètes et différentielles
- **Interface multilingue** : Support du français et de l'anglais
- **Journalisation** : Logs détaillés au format JSON
- **Suivi en temps réel** : État et progression des travaux de sauvegarde
- **Exécution par ligne de commande** : Possibilité d'exécuter des travaux spécifiques via arguments

## Structure du projet

Le projet est divisé en deux composants principaux :

- **EasySave** : Application console principale
- **EasyLog** : Bibliothèque de gestion des logs

## Prérequis

- Windows 10/11
- .NET 8.0 Runtime
- Droits d'accès aux dossiers source et cible

## Installation

1. Téléchargez la dernière version depuis la page des releases
2. Décompressez l'archive dans le dossier de votre choix
3. Lancez l'application via `EasySave.exe`

## Utilisation rapide

### Via l'interface console

1. Lancez `EasySave.exe`
2. Suivez les instructions à l'écran pour créer et exécuter des travaux de sauvegarde

### Via ligne de commande

Exécutez des travaux spécifiques directement :

```
EasySave.exe 1     # Exécute le travail n°1
EasySave.exe 1-3   # Exécute les travaux n°1 à 3
EasySave.exe 1;3;5 # Exécute les travaux n°1, 3 et 5
```

## Documentation

- [Manuel d'utilisation](./MANUEL_UTILISATION.md) - Guide pour les utilisateurs finaux
- [Documentation technique](./DOCUMENTATION_TECHNIQUE.md) - Documentation pour le support technique et les développeurs

## Roadmap

- **Version 1.1** : Support du format XML pour les logs
- **Version 2.0** : Interface graphique WPF avec architecture MVVM
- **Version 3.0** : Sauvegardes parallèles, gestion des priorités et conteneurisation Docker

## Contribution

Ce projet est développé dans le cadre d'un projet interne ProSoft. Pour contribuer :

1. Forkez le dépôt
2. Créez une branche pour votre fonctionnalité (`git checkout -b feature/ma-fonctionnalite`)
3. Committez vos changements (`git commit -m 'Ajout de ma fonctionnalité'`)
4. Poussez vers la branche (`git push origin feature/ma-fonctionnalite`)
5. Ouvrez une Pull Request

## Licence

© 2026 ProSoft. Tous droits réservés.