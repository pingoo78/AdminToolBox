using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AdminToolBox.Services;

/// <summary>
/// Extrait l'icône native associée à un exécutable/console résolu sur disque.
/// On essaie d'abord <c>ExtractIconEx</c>, qui lit directement la ressource icône embarquée
/// dans le module (fiable pour .exe/.dll/.cpl) ; pour les fichiers sans ressource icône propre
/// (consoles MMC .msc), on se rabat sur <c>SHGetFileInfo</c>, qui suit le même pipeline shell
/// que l'Explorateur pour résoudre l'icône associée.
/// </summary>
public static class IconExtractionService
{
    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[]? phiconLarge, IntPtr[]? phiconSmall, uint nIcons);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private static readonly ConcurrentDictionary<string, byte[]?> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Lazy<byte[]?> GenericUnavailableIcon = new(() => HandleToPngBytes(ExtractGenericShellIcon()));

    /// <param name="resolvedFullPath">Chemin complet retourné par <see cref="ToolAvailabilityService.ResolveFullPath"/>.</param>
    /// <returns>Bytes PNG de l'icône, ou null si l'extraction échoue.</returns>
    public static byte[]? GetIconPngBytes(string resolvedFullPath)
    {
        return Cache.GetOrAdd(resolvedFullPath, static path => HandleToPngBytes(ExtractFromModuleResource(path) ?? ExtractViaShell(path)));
    }

    /// <summary>
    /// Icône générique de document (le même repli que l'Explorateur pour un type de fichier
    /// inconnu), affichée pour les outils absents du poste faute d'icône réelle à extraire.
    /// </summary>
    public static byte[]? GetGenericUnavailableIconPngBytes() => GenericUnavailableIcon.Value;

    private static byte[]? HandleToPngBytes(IntPtr? hIcon)
    {
        if (hIcon is not { } handle || handle == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            using var icon = Icon.FromHandle(handle);
            using var bitmap = icon.ToBitmap();
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
        catch (Exception ex) when (ex is ArgumentException or ExternalException)
        {
            return null;
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static IntPtr? ExtractFromModuleResource(string path)
    {
        var large = new IntPtr[1];
        var count = ExtractIconEx(path, 0, large, null, 1);
        return count > 0 && large[0] != IntPtr.Zero ? large[0] : null;
    }

    private static IntPtr? ExtractViaShell(string path)
    {
        var info = new SHFILEINFO();
        var handleResult = SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI_ICON | SHGFI_LARGEICON);
        return handleResult != IntPtr.Zero && info.hIcon != IntPtr.Zero ? info.hIcon : null;
    }

    private static IntPtr? ExtractGenericShellIcon()
    {
        var info = new SHFILEINFO();
        var handleResult = SHGetFileInfo(
            "__admintoolbox_missing__",
            FILE_ATTRIBUTE_NORMAL,
            ref info,
            (uint)Marshal.SizeOf<SHFILEINFO>(),
            SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);
        return handleResult != IntPtr.Zero && info.hIcon != IntPtr.Zero ? info.hIcon : null;
    }
}
