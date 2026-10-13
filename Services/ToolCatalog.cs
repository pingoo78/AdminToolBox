using AdminToolBox.Models;

namespace AdminToolBox.Services;

/// <summary>Catalogue statique des outils proposés par la toolbox.</summary>
public static class ToolCatalog
{
    /// <summary>Chemin de LAPS UI (legacy, AdmPwd.UI.exe) : Program Files (x64) ou, à défaut, Program Files (x86).</summary>
    private static string LapsUiPath { get; } = new[]
    {
        Environment.SpecialFolder.ProgramFiles,
        Environment.SpecialFolder.ProgramFilesX86,
    }
    .Select(folder => Path.Combine(Environment.GetFolderPath(folder), "LAPS", "AdmPwd.UI.exe"))
    .FirstOrDefault(File.Exists) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LAPS", "AdmPwd.UI.exe");

    public static IReadOnlyList<ToolItem> All { get; } = new List<ToolItem>
    {
        // --- Système ---
        new() { Name = "Registre", Description = "Éditeur du Registre Windows", Category = ToolCategory.Systeme, FileName = "regedit.exe" },
        new() { Name = "Observateur d'événements", Description = "Journaux d'événements Windows", Category = ToolCategory.Systeme, FileName = "eventvwr.msc" },
        new() { Name = "Gestionnaire des tâches", Description = "Processus, performances, démarrage", Category = ToolCategory.Systeme, FileName = "taskmgr.exe" },
        new() { Name = "Gestionnaire de périphériques", Description = "Matériel et pilotes", Category = ToolCategory.Systeme, FileName = "devmgmt.msc" },
        new() { Name = "Gestion de l'ordinateur", Description = "Console de gestion regroupée", Category = ToolCategory.Systeme, FileName = "compmgmt.msc" },
        new() { Name = "Services", Description = "Services Windows", Category = ToolCategory.Systeme, FileName = "services.msc" },
        new() { Name = "Stratégie de groupe locale", Description = "Éditeur de la stratégie de groupe locale", Category = ToolCategory.Systeme, FileName = "gpedit.msc" },
        new() { Name = "Stratégie de sécurité locale", Description = "Comptes, audit, droits utilisateur", Category = ToolCategory.Systeme, FileName = "secpol.msc" },
        new() { Name = "Utilisateurs et groupes locaux", Description = "Comptes locaux de la machine", Category = ToolCategory.Systeme, FileName = "lusrmgr.msc" },
        new() { Name = "Gestion des disques", Description = "Partitions et volumes", Category = ToolCategory.Systeme, FileName = "diskmgmt.msc" },
        new() { Name = "Planificateur de tâches", Description = "Tâches planifiées", Category = ToolCategory.Systeme, FileName = "taskschd.msc" },
        new() { Name = "Moniteur de ressources", Description = "CPU, mémoire, disque, réseau en direct", Category = ToolCategory.Systeme, FileName = "resmon.exe" },
        new() { Name = "Moniteur de performances", Description = "Compteurs de performance", Category = ToolCategory.Systeme, FileName = "perfmon.exe" },
        new() { Name = "Configuration système", Description = "Options et services au démarrage", Category = ToolCategory.Systeme, FileName = "msconfig.exe" },
        new() { Name = "Informations système", Description = "Détails matériel, pilotes, composants", Category = ToolCategory.Systeme, FileName = "msinfo32.exe" },
        new() { Name = "Panneau de configuration", Description = "Panneau de configuration classique", Category = ToolCategory.Systeme, FileName = "control.exe" },
        // IconImagePath : l'icône embarquée dans SystemSettings.exe est un vieux pictogramme plat ;
        // on prend le vrai logo Fluent du paquet à la place.
        new() { Name = "Paramètres Windows", Description = "Nouvelle application Paramètres", Category = ToolCategory.Systeme, FileName = "ms-settings:", IconImagePath = @"C:\Windows\ImmersiveControlPanel\images\logo.targetsize-48_altform-unplated.png" },
        new() { Name = "Propriétés système", Description = "Nom d'ordinateur, matériel, protection système", Category = ToolCategory.Systeme, FileName = "sysdm.cpl" },
        new() { Name = "Variables d'environnement", Description = "Édition des variables système/utilisateur", Category = ToolCategory.Systeme, FileName = "rundll32.exe", Arguments = "sysdm.cpl,EditEnvironmentVariables", IconFileName = "sysdm.cpl" },
        new() { Name = "Nettoyage de disque", Description = "Libère de l'espace disque", Category = ToolCategory.Systeme, FileName = "cleanmgr.exe" },
        new() { Name = "Gestion de l'impression", Description = "Imprimantes, pilotes et files d'attente", Category = ToolCategory.Systeme, FileName = "printmanagement.msc" },

        // --- Réseau ---
        new() { Name = "Connexions réseau", Description = "Cartes et connexions réseau", Category = ToolCategory.Reseau, FileName = "ncpa.cpl" },
        new() { Name = "Pare-feu Windows Defender", Description = "Pare-feu avec fonctions avancées de sécurité", Category = ToolCategory.Reseau, FileName = "wf.msc" },
        new() { Name = "Partages et sessions", Description = "Dossiers partagés et sessions actives", Category = ToolCategory.Reseau, FileName = "fsmgmt.msc" },
        new() { Name = "Gestion DNS", Description = "Console DNS (nécessite RSAT)", Category = ToolCategory.Reseau, FileName = "dnsmgmt.msc" },
        new() { Name = "Gestion DHCP", Description = "Console DHCP (nécessite RSAT)", Category = ToolCategory.Reseau, FileName = "dhcpmgmt.msc" },

        // --- Sécurité ---
        new() { Name = "Certificats (machine)", Description = "Magasin de certificats de l'ordinateur", Category = ToolCategory.Securite, FileName = "certlm.msc" },
        // IconFileName : icône réelle de l'applet (trouvée via HKCR\CLSID\{D9EF8727-...}\DefaultIcon),
        // plutôt que celle générique de control.exe (une boîte à outils).
        new() { Name = "BitLocker", Description = "Chiffrement de lecteur", Category = ToolCategory.Securite, FileName = "control.exe", Arguments = "/name Microsoft.BitLockerDriveEncryption", IconFileName = "fvecpl.dll" },
        // IconFileName : icône réelle du bouclier Sécurité Windows (auparavant réglée par erreur
        // sur control.exe, la même icône générique que "Panneau de configuration").
        new() { Name = "Sécurité Windows", Description = "Windows Defender", Category = ToolCategory.Securite, FileName = "windowsdefender:", IconFileName = @"C:\Windows\System32\SecurityHealthSystray.exe" },

        // --- Active Directory (RSAT) ---
        new() { Name = "Utilisateurs et ordinateurs AD", Description = "Comptes, groupes, unités d'organisation — nécessite RSAT", Category = ToolCategory.ActiveDirectory, FileName = "dsa.msc" },
        new() { Name = "Gestion des stratégies de groupe", Description = "GPO du domaine — nécessite RSAT", Category = ToolCategory.ActiveDirectory, FileName = "gpmc.msc" },
        new() { Name = "Sites et services AD", Description = "Topologie de réplication — nécessite RSAT", Category = ToolCategory.ActiveDirectory, FileName = "dssite.msc" },
        new() { Name = "Domaines et approbations AD", Description = "Relations d'approbation entre domaines — nécessite RSAT", Category = ToolCategory.ActiveDirectory, FileName = "domain.msc" },

        // Grisé si LAPS UI (legacy) n'est pas installé, comme les outils RSAT.
        new() { Name = "LAPS UI", Description = "Mots de passe administrateur local (LAPS legacy)", Category = ToolCategory.ActiveDirectory, FileName = LapsUiPath, UnavailableHint = "Il nécessite LAPS UI (legacy, AdmPwd.UI.exe), à installer depuis le paquet LAPS de Microsoft." },

        // --- Consoles ---
        new() { Name = "PowerShell", Description = "Console PowerShell", Category = ToolCategory.Consoles, FileName = "powershell.exe" },
        new() { Name = "PowerShell ISE", Description = "Éditeur de scripts PowerShell", Category = ToolCategory.Consoles, FileName = "powershell_ise.exe" },
        new() { Name = "Invite de commandes", Description = "Invite de commandes classique", Category = ToolCategory.Consoles, FileName = "cmd.exe" },
        // IconPackageFamilyName : le wt.exe résolu via PATH n'est qu'un stub App Execution Alias
        // sans icône propre ; on va chercher l'icône dans le vrai dossier d'installation MSIX.
        new() { Name = "Windows Terminal", Description = "Terminal moderne à onglets (si installé)", Category = ToolCategory.Consoles, FileName = "wt.exe", IconPackageFamilyName = "Microsoft.WindowsTerminal_8wekyb3d8bbwe" },
        new() { Name = "Console MMC vierge", Description = "Console de gestion Microsoft vierge", Category = ToolCategory.Consoles, FileName = "mmc.exe" },
    };
}
