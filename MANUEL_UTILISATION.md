# Manuel d'Utilisation - EasySave v1.0

## Introduction

EasySave est un logiciel de sauvegarde robuste développé par ProSoft, permettant de créer et d'exécuter jusqu'à 5 travaux de sauvegarde différents. Ce manuel vous guidera à travers les fonctionnalités principales de la version 1.0.

## Installation

1. Décompressez le fichier ZIP contenant EasySave dans le dossier de votre choix
2. Aucune installation supplémentaire n'est requise

## Démarrage

Lancez l'application en double-cliquant sur `EasySave.exe`. Vous verrez apparaître le menu principal :

```
===== EasySave v1.0 =====
1. Créer un travail de sauvegarde
2. Exécuter un travail de sauvegarde
3. Afficher les travaux de sauvegarde
4. Changer de langue (FR/EN)
0. Quitter
Votre choix : 
```

## Création d'un travail de sauvegarde

1. Sélectionnez l'option **1** dans le menu principal
2. Entrez un nom unique pour votre travail de sauvegarde
3. Spécifiez le dossier source à sauvegarder (chemin complet)
4. Spécifiez le dossier de destination (chemin complet)
5. Choisissez le type de sauvegarde :
   - **1** pour une sauvegarde complète (copie intégrale des fichiers)
   - **2** pour une sauvegarde différentielle (copie uniquement des fichiers modifiés depuis la dernière sauvegarde complète)

## Exécution d'un travail de sauvegarde

### Via le menu

1. Sélectionnez l'option **2** dans le menu principal
2. Entrez le numéro du travail à exécuter (visible dans la liste des travaux)
3. La progression de la sauvegarde s'affichera à l'écran

### Via la ligne de commande

Pour exécuter des travaux spécifiques sans passer par le menu :

- Pour un seul travail : `EasySave.exe 1` (exécute le travail n°1)
- Pour une plage de travaux : `EasySave.exe 1-3` (exécute les travaux n°1, 2 et 3)
- Pour des travaux spécifiques : `EasySave.exe 1;3;5` (exécute les travaux n°1, 3 et 5)

## Affichage des travaux de sauvegarde

1. Sélectionnez l'option **3** dans le menu principal
2. La liste des travaux configurés s'affiche avec leurs détails :
   - Numéro et nom du travail
   - Dossier source
   - Dossier cible
   - Type de sauvegarde (Complète/Différentielle)

## Changement de langue

1. Sélectionnez l'option **4** dans le menu principal
2. Choisissez la langue souhaitée :
   - **1** pour le Français
   - **2** pour l'Anglais

## Suivi des sauvegardes

EasySave génère automatiquement deux types de fichiers pour suivre l'activité :

1. **Fichiers journaliers** (logs) : Situés dans le dossier `logs` à côté de l'exécutable, au format `YYYY-MM-DD.json`
2. **Fichier d'état** : Situé dans le dossier d'exécution, nommé `state.json`

Ces fichiers peuvent être consultés pour vérifier le bon déroulement des sauvegardes.

## Résolution des problèmes courants

| Problème | Solution |
|----------|----------|
| Chemin source introuvable | Vérifiez que le dossier existe et que vous avez les droits d'accès |
| Chemin cible inaccessible | Vérifiez que le dossier existe, que vous avez les droits d'écriture et qu'il y a suffisamment d'espace disque |
| Fichier verrouillé | Fermez les applications qui pourraient utiliser les fichiers à sauvegarder |

## Support technique

Pour toute assistance supplémentaire, contactez le support technique ProSoft :
- Email : support@prosoft.com
- Téléphone : 01 23 45 67 89 (5/7 jours, 8h-17h)