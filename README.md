<div align="center">

# AdminToolBox

**Une boîte à outils d'administration Windows — tous les raccourcis utiles, réunis dans une seule fenêtre.**

Consoles MMC, panneaux de configuration, PowerShell, outils Active Directory... plus besoin de se souvenir du nom de la commande ni de fouiller dans les menus Windows.

</div>

---

## Aperçu

AdminToolBox est une application de bureau Windows (WinUI 3 / .NET 8) qui rassemble dans une interface unique, claire et réactive, les outils système que les administrateurs lancent le plus souvent : consoles `.msc`, applets du panneau de configuration, protocoles `ms-settings:`, consoles PowerShell/Terminal, etc.

Elle se lance systématiquement avec élévation administrateur (UAC), affiche un résumé réseau/système du poste en un coup d'œil, et s'adapte proprement au redimensionnement de la fenêtre — jusqu'à un mode compact façon Gestionnaire des tâches.

## Fonctionnalités

- **Catalogue de ~40 outils** classés en cinq catégories (Système, Réseau, Sécurité, Active Directory, Consoles), chacun avec sa véritable icône Windows extraite dynamiquement.
- **Recherche instantanée** intégrée à la barre de titre, filtrant par nom et description.
- **Détection de disponibilité** : un outil absent du poste (RSAT manquant, par exemple) apparaît grisé plutôt que d'échouer silencieusement au lancement.
- **Favoris** : une étoile sur chaque raccourci épingle l'outil dans une section dédiée, en tête de la catégorie « Tous ».
- **Panneau d'informations système** : nom du poste, domaine/groupe de travail, utilisateur, adresse(s) IP, passerelle, DNS et version de Windows (avec correction du bug connu de Microsoft qui affiche « Windows 10 » sur les postes Windows 11), en disposition adaptative sur toute largeur de fenêtre.
- **Interface adaptative** : le panneau de navigation bascule automatiquement entre affichage étendu, compact (survol) et minimal (icône hamburger dans la barre de titre, à la manière du Gestionnaire des tâches Windows 11), selon la largeur de la fenêtre.
- **Thème clair / sombre / système**, réglable depuis le panneau Paramètres et persisté entre les sessions.
- **Élévation automatique (UAC)** : l'application requiert systématiquement un jeton administrateur, adapté à son usage.
- **Vérification de mise à jour** au démarrage, avec statut affiché dans le panneau Paramètres.

## Catégories et outils

<details>
<summary><strong>Système</strong> (21 outils)</summary>

| Outil | Description |
|---|---|
| Registre | Éditeur du Registre Windows |
| Observateur d'événements | Journaux d'événements Windows |
| Gestionnaire des tâches | Processus, performances, démarrage |
| Gestionnaire de périphériques | Matériel et pilotes |
| Gestion de l'ordinateur | Console de gestion regroupée |
| Services | Services Windows |
| Stratégie de groupe locale | Éditeur de la stratégie de groupe locale |
| Stratégie de sécurité locale | Comptes, audit, droits utilisateur |
| Utilisateurs et groupes locaux | Comptes locaux de la machine |
| Gestion des disques | Partitions et volumes |
| Planificateur de tâches | Tâches planifiées |
| Moniteur de ressources | CPU, mémoire, disque, réseau en direct |
| Moniteur de performances | Compteurs de performance |
| Configuration système | Options et services au démarrage |
| Informations système | Détails matériel, pilotes, composants |
| Panneau de configuration | Panneau de configuration classique |
| Paramètres Windows | Nouvelle application Paramètres |
| Propriétés système | Nom d'ordinateur, matériel, protection système |
| Variables d'environnement | Édition des variables système/utilisateur |
| Nettoyage de disque | Libère de l'espace disque |
| Gestion de l'impression | Imprimantes, pilotes et files d'attente |

</details>

<details>
<summary><strong>Réseau</strong> (5 outils)</summary>

| Outil | Description |
|---|---|
| Connexions réseau | Cartes et connexions réseau |
| Pare-feu Windows Defender | Pare-feu avec fonctions avancées de sécurité |
| Partages et sessions | Dossiers partagés et sessions actives |
| Gestion DNS | Console DNS *(nécessite RSAT)* |
| Gestion DHCP | Console DHCP *(nécessite RSAT)* |

</details>

<details>
<summary><strong>Sécurité</strong> (3 outils)</summary>

| Outil | Description |
|---|---|
| Certificats (machine) | Magasin de certificats de l'ordinateur |
| BitLocker | Chiffrement de lecteur |
| Sécurité Windows | Windows Defender |

</details>

<details>
<summary><strong>Active Directory</strong> (4 outils, RSAT requis)</summary>

| Outil | Description |
|---|---|
| Utilisateurs et ordinateurs AD | Comptes, groupes, unités d'organisation |
| Gestion des stratégies de groupe | GPO du domaine |
| Sites et services AD | Topologie de réplication |
| Domaines et approbations AD | Relations d'approbation entre domaines |

</details>

<details>
<summary><strong>Consoles</strong> (5 outils)</summary>

| Outil | Description |
|---|---|
| PowerShell | Console PowerShell |
| PowerShell ISE | Éditeur de scripts PowerShell |
| Invite de commandes | Invite de commandes classique |
| Windows Terminal | Terminal moderne à onglets *(si installé)* |
| Console MMC vierge | Console de gestion Microsoft vierge |

</details>

## Prérequis

- Windows 10 1809 (build 17763) ou supérieur, Windows 11 recommandé
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)

## Compilation et exécution

```powershell
git clone https://github.com/pingoo78/AdminToolBox.git
cd AdminToolBox
dotnet build -p:Platform=x64
```

L'exécutable généré (`bin\x64\Debug\...\AdminToolBox.exe`) demande une élévation UAC au lancement — c'est attendu, l'application est conçue pour toujours s'exécuter avec un jeton administrateur.

> `-p:Platform=x64` est nécessaire : sans cette précision, le SDK peut mal résoudre la plateforme cible par défaut.

### Build de distribution (autonome)

```powershell
dotnet publish -c Release -r win-x64 -p:Platform=x64 --self-contained true
```

Génère un exécutable autonome (aucune dépendance à installer sur le poste cible) dans `bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\publish\`.

### Installateur (.msi)

```powershell
dotnet tool install --global wix --version 5.0.2
wix extension add WixToolset.UI.wixext/5.0.2
wix build Installer/AdminToolBox.wxs -arch x64 -ext WixToolset.UI.wixext `
  -d LicenseRtf="Installer\License.rtf" `
  -d PublishDir="bind\Release
et8.0-windows10.0.19041.0\win-x64\publish" `
  -out Installer/AdminToolBox.msi
```

> La version 5.x de WiX est utilisée volontairement : la v7 impose l'acceptation d'un « Open Source Maintenance Fee » (EULA) avant utilisation.

## Stack technique

- **WinUI 3** (Windows App SDK) sur **.NET 8**, en C#
- Application **non empaquetée** (pas de MSIX) — un simple exécutable autonome
- Icônes extraites nativement des exécutables/DLL Windows (`ExtractIconEx` / `SHGetFileInfo`), sans image tierce
- Persistance des préférences (thème, favoris) en JSON local, sous `%LocalAppData%\AdminToolBox`

## Structure du projet

```
AdminToolBox/
├── Controls/     Contrôles XAML personnalisés (WrapPanel adaptatif)
├── Models/       Modèles de données (ToolItem, ToolCategory, InfoItem)
├── Services/     Logique métier (catalogue, lancement, icônes, infos système, paramètres)
├── ViewModels/   Liaison de données pour l'interface
└── MainWindow.*  Fenêtre principale
```

## Licence

AdminToolBox est distribué sous licence [GNU GPL v3](LICENSE) : libre d'utilisation, de modification et de redistribution, à condition que tout dérivé distribué reste lui aussi sous GPL v3.

## Auteur

**Alexandre ARMENIAN**

