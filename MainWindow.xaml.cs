// This file is part of AdminToolBox.
// Copyright (C) 2026 Alexandre ARMENIAN
//
// AdminToolBox is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation, either version 3
// of the License, or (at your option) any later version.
//
// AdminToolBox is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.InteropServices.WindowsRuntime;
using AdminToolBox.Models;
using AdminToolBox.Services;
using AdminToolBox.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace AdminToolBox;

public sealed partial class MainWindow : Window
{
    private List<ToolViewModel> _allTools = [];
    private ToolCategory? _selectedCategory;
    private string _searchText = string.Empty;
    private readonly Task<UpdateCheckResult> _updateCheckTask;
    private readonly AppSettings _settings;
    private bool _themeComboInitializing = true;

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SetMinimumWindowSize();
        SetWindowIcon();
        LoadSystemInfo();
        _settings = AppSettingsService.Load();
        LoadTheme();
        ApplyInfoPanelsCollapsed();
        _updateCheckTask = UpdateCheckService.CheckForUpdateAsync(AppInfoService.GetVersion());
        LoadAboutInfo();
        _ = LoadToolsAsync();
        UpdatePaneToggleButton(NavView.DisplayMode);
    }

    private void InfoToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.InfoPanelsCollapsed = !_settings.InfoPanelsCollapsed;
        AppSettingsService.Save(_settings);
        ApplyInfoPanelsCollapsed();
    }

    /// <summary>Chevron vers le haut quand les panneaux sont affichés (réduire), vers le bas quand ils sont réduits.</summary>
    private void ApplyInfoPanelsCollapsed()
    {
        InfoPanelsGrid.Visibility = _settings.InfoPanelsCollapsed ? Visibility.Collapsed : Visibility.Visible;
        InfoToggleIcon.Glyph = char.ConvertFromUtf32(_settings.InfoPanelsCollapsed ? 0xE70D : 0xE70E);
    }

    private void PaneToggleButton_Click(object sender, RoutedEventArgs e) => NavView.IsPaneOpen = !NavView.IsPaneOpen;

    /// <summary>
    /// Suit les recommandations Microsoft : le bouton hamburger ne migre dans la barre de titre
    /// qu'en mode Minimal (fenêtre étroite, panneau caché en overlay) ; en mode Compact/Expanded,
    /// c'est le bouton intégré du NavigationView (en haut du panneau) qui est utilisé.
    /// </summary>
    private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        => UpdatePaneToggleButton(args.DisplayMode);

    private void UpdatePaneToggleButton(NavigationViewDisplayMode displayMode)
    {
        var isMinimal = displayMode == NavigationViewDisplayMode.Minimal;
        PaneToggleButton.Visibility = isMinimal ? Visibility.Visible : Visibility.Collapsed;
        NavView.IsPaneToggleButtonVisible = !isMinimal;
        UpdateTitleBarPassthroughRegion();
        UpdateInfoPanelsLayout(isMinimal);
    }

    /// <summary>
    /// Panneaux Poste/Domaine/Utilisateur/Système et IP/Passerelle/DNS côte à côte (60%/40%) ;
    /// en mode minimal (fenêtre étroite), ils passent l'un sous l'autre en pleine largeur.
    /// </summary>
    private void UpdateInfoPanelsLayout(bool stacked)
    {
        Grid.SetColumnSpan(LeftInfoBorder, stacked ? 2 : 1);
        Grid.SetColumnSpan(RightInfoBorder, stacked ? 2 : 1);
        Grid.SetRow(RightInfoBorder, stacked ? 1 : 0);
        Grid.SetColumn(RightInfoBorder, stacked ? 0 : 1);
        RightInfoBorder.Margin = stacked ? new Thickness(0, 16, 0, 0) : new Thickness(8, 0, 0, 0);
    }

    /// <summary>
    /// SetTitleBar rend automatiquement les contrôles interactifs cliquables, mais ne suffit pas à
    /// exclure le double-clic-pour-agrandir du système : sans cette exclusion explicite, un
    /// double-clic sur le bouton hamburger agrandit la fenêtre comme n'importe où ailleurs dans la
    /// barre de titre.
    /// </summary>
    private void UpdateTitleBarPassthroughRegion()
    {
        var nonClientSource = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);

        // XamlRoot n'est peuplé qu'une fois la fenêtre affichée : lors de l'appel initial depuis le
        // constructeur (avant Activate), il vaut encore null.
        if (PaneToggleButton.Visibility != Visibility.Visible || AppTitleBar.XamlRoot is not { } xamlRoot)
        {
            nonClientSource.ClearRegionRects(NonClientRegionKind.Passthrough);
            return;
        }

        var scale = xamlRoot.RasterizationScale;
        var bounds = PaneToggleButton.TransformToVisual(AppTitleBar)
            .TransformBounds(new Rect(0, 0, PaneToggleButton.ActualWidth, PaneToggleButton.ActualHeight));

        var rect = new RectInt32(
            (int)(bounds.X * scale),
            (int)(bounds.Y * scale),
            (int)(bounds.Width * scale),
            (int)(bounds.Height * scale));

        nonClientSource.SetRegionRects(NonClientRegionKind.Passthrough, [rect]);
    }

    /// <summary>
    /// Empêche de redimensionner sous une taille où le contenu (recherche, panneau d'infos,
    /// grille d'outils) ne serait plus exploitable, même avec le NavigationView en mode minimal.
    /// </summary>
    private void SetMinimumWindowSize()
    {
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 624;
            presenter.PreferredMinimumHeight = 400;
        }
    }

    /// <summary>
    /// L'icône Win32 posée via ApplicationIcon dans le csproj ne s'applique qu'à l'exécutable sur
    /// le disque : la fenêtre WinUI (barre de titre, barre des tâches, Alt+Tab) a sa propre icône,
    /// à définir explicitement sur l'AppWindow.
    /// </summary>
    private async void SetWindowIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (!File.Exists(iconPath))
        {
            return;
        }

        AppWindow.SetIcon(iconPath);

        var pngBytes = IconExtractionService.GetIconPngBytes(iconPath);
        if (pngBytes is not null)
        {
            TitleBarIcon.Source = await CreateBitmapImageAsync(pngBytes);
        }
    }

    /// <summary>Masque le nom de l'app quand la barre de titre devient trop étroite pour lui et la recherche à la fois.</summary>
    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        TitleBarAppName.Visibility = e.NewSize.Width < 700 ? Visibility.Collapsed : Visibility.Visible;
        UpdateTitleBarPassthroughRegion();
    }

    /// <summary>
    /// XamlRoot n'est valide qu'une fois la fenêtre chargée : on resynchronise ici la région de
    /// passthrough au cas où le mode d'affichage initial (calculé avant ce point) était déjà Minimal.
    /// </summary>
    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e) => UpdateTitleBarPassthroughRegion();

    private void LoadTheme()
    {
        ThemeComboBox.SelectedIndex = _settings.Theme switch { "Light" => 1, "Dark" => 2, _ => 0 };
        ApplyTheme(_settings.Theme);
        _themeComboInitializing = false;
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_themeComboInitializing || ThemeComboBox.SelectedItem is not ComboBoxItem { Tag: string themeTag })
        {
            return;
        }

        ApplyTheme(themeTag);
        _settings.Theme = themeTag;
        AppSettingsService.Save(_settings);
    }

    private void ApplyTheme(string theme)
    {
        if (Content is FrameworkElement root)
        {
            root.RequestedTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default,
            };
        }
    }

    private void LoadAboutInfo()
    {
        AboutVersionText.Text = $"Version {AppInfoService.GetVersion()}";
        AboutBuildDateText.Text = $"Date de build : {AppInfoService.GetBuildDate():dd/MM/yyyy HH:mm}";
        AboutUpdateStatusText.Text = "Vérification des mises à jour...";
        _ = UpdateAboutStatusAsync();
    }

    private async Task UpdateAboutStatusAsync()
    {
        var result = await _updateCheckTask;
        AboutUpdateStatusText.Text = result switch
        {
            { Success: true, UpdateAvailable: true, LatestVersion: { } v } => $"Nouvelle version disponible : v{v}",
            { Success: true, UpdateAvailable: false } => "Vous utilisez la dernière version.",
            _ => "Impossible de vérifier les mises à jour (dépôt privé ou hors ligne).",
        };
    }

    // Glyphes Segoe Fluent Icons (identiques sous Segoe MDL2 Assets, vérifié empiriquement).
    private static readonly string GlyphPc = char.ConvertFromUtf32(0xE7F8);
    private static readonly string GlyphBuilding = char.ConvertFromUtf32(0xEC08);
    private static readonly string GlyphUser = char.ConvertFromUtf32(0xE77B);
    private static readonly string GlyphSetting = char.ConvertFromUtf32(0xE713);
    private static readonly string GlyphServer = char.ConvertFromUtf32(0xE968);
    private static readonly string GlyphGateway = char.ConvertFromUtf32(0xEBD2);
    private static readonly string GlyphGlobe = char.ConvertFromUtf32(0xE774);

    private void LoadSystemInfo()
    {
        var info = SystemInfoService.GetSnapshot();

        InfoItemsControl.ItemsSource = new List<InfoItem>
        {
            new(GlyphPc, "Poste", info.HostName),
            new(GlyphBuilding, "Domaine / groupe de travail", info.DomainOrWorkgroup),
            new(GlyphUser, "Utilisateur", info.UserName),
            new(GlyphSetting, "Système", info.OsVersion),
        };

        RightInfoItemsControl.ItemsSource = new List<InfoItem>
        {
            new(GlyphServer, "Adresse(s) IP", info.IPv4Addresses),
            new(GlyphGateway, "Passerelle", info.DefaultGateway),
            new(GlyphGlobe, "DNS", info.DnsServers),
        };
    }

    private async Task LoadToolsAsync()
    {
        _allTools = await Task.Run(() => ToolCatalog.All
            .Select(tool => new ToolViewModel(tool, ToolAvailabilityService.IsAvailable(tool))
            {
                IsFavorite = _settings.Favorites.Contains(tool.Name),
            })
            .ToList());

        ApplyFilter();
        await LoadIconsAsync();
    }

    private async Task LoadIconsAsync()
    {
        foreach (var toolViewModel in _allTools)
        {
            var pngBytes = await ResolveIconBytesAsync(toolViewModel.Tool);
            pngBytes ??= IconExtractionService.GetGenericUnavailableIconPngBytes();
            if (pngBytes is null)
            {
                continue;
            }

            toolViewModel.Icon = await CreateBitmapImageAsync(pngBytes);
        }
    }

    private static async Task<byte[]?> ResolveIconBytesAsync(ToolItem tool)
    {
        if (tool.IconImagePath is { } imagePath)
        {
            return File.Exists(imagePath) ? await File.ReadAllBytesAsync(imagePath) : null;
        }

        var resolvedPath = tool.IconPackageFamilyName is { } packageFamilyName
            ? ToolAvailabilityService.ResolvePackagedFullPath(packageFamilyName, tool.IconLookupFile)
            : ToolAvailabilityService.ResolveFullPath(tool.IconLookupFile);

        // SHGetFileInfo s'appuie sur des gestionnaires d'icônes COM (IExtractIcon) qui exigent
        // un appartement STA correctement initialisé : on reste donc sur le thread UI (STA)
        // plutôt que de déléguer à un thread du pool (MTA), sous peine d'échecs silencieux et
        // incohérents selon le thread réutilisé.
        return resolvedPath is not null ? IconExtractionService.GetIconPngBytes(resolvedPath) : null;
    }

    private static async Task<BitmapImage> CreateBitmapImageAsync(byte[] pngBytes)
    {
        var bitmapImage = new BitmapImage();
        using var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(pngBytes.AsBuffer());
        stream.Seek(0);
        await bitmapImage.SetSourceAsync(stream);
        return bitmapImage;
    }

    private void ApplyFilter()
    {
        IEnumerable<ToolViewModel> query = _allTools;

        if (_selectedCategory is { } category)
        {
            query = query.Where(t => t.Tool.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            query = query.Where(t =>
                t.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                t.Tool.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.OrderBy(t => t.Name).ToList();

        if (_selectedCategory is null)
        {
            var favorites = filtered.Where(t => t.IsFavorite).ToList();
            var rest = filtered.Where(t => !t.IsFavorite).ToList();

            FavoritesSection.Visibility = favorites.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FavoritesGridView.ItemsSource = favorites;
            AllToolsHeader.Visibility = favorites.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ToolsGridView.ItemsSource = rest;
        }
        else
        {
            FavoritesSection.Visibility = Visibility.Collapsed;
            AllToolsHeader.Visibility = Visibility.Collapsed;
            ToolsGridView.ItemsSource = filtered;
        }
    }

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: ToolViewModel toolViewModel })
        {
            return;
        }

        toolViewModel.IsFavorite = !toolViewModel.IsFavorite;

        if (toolViewModel.IsFavorite)
        {
            _settings.Favorites.Add(toolViewModel.Name);
        }
        else
        {
            _settings.Favorites.Remove(toolViewModel.Name);
        }

        AppSettingsService.Save(_settings);
        ApplyFilter();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ShowSettingsPanel();
            return;
        }

        if (args.SelectedItemContainer is not NavigationViewItem { Tag: string tag })
        {
            return;
        }

        HideSettingsPanel();
        _selectedCategory = tag == "All" ? null : Enum.Parse<ToolCategory>(tag);
        ApplyFilter();
    }

    private void ShowSettingsPanel()
    {
        InfoSection.Visibility = Visibility.Collapsed;
        ToolsScrollViewer.Visibility = Visibility.Collapsed;
        SettingsPanel.Visibility = Visibility.Visible;
    }

    private void HideSettingsPanel()
    {
        SettingsPanel.Visibility = Visibility.Collapsed;
        InfoSection.Visibility = Visibility.Visible;
        ToolsScrollViewer.Visibility = Visibility.Visible;
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
        {
            return;
        }

        _searchText = sender.Text;
        ApplyFilter();
    }

    private async void ToolsGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not ToolViewModel toolViewModel)
        {
            return;
        }

        if (!toolViewModel.IsAvailable)
        {
            await ShowDialogAsync(
                "Outil indisponible",
                $"« {toolViewModel.Name} » n'est pas installé sur ce poste. " +
                (toolViewModel.Tool.UnavailableHint ?? "Les outils Active Directory nécessitent souvent RSAT."));
            return;
        }

        try
        {
            ToolLauncherService.Launch(toolViewModel.Tool);
        }
        catch (ToolLaunchException ex)
        {
            await ShowDialogAsync("Erreur de lancement", ex.Message);
        }
    }

    private async Task ShowDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
