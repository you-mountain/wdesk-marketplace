using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WDesk.Widgets.AppLauncher;

public static class AppIconLoader
{
    private static readonly Dictionary<string, ImageSource?> _cache = new();

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, string lpIconPath, ref ushort lpiIcon);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static ImageSource? GetIcon(string path, int size = 48)
    {
        if (string.IsNullOrEmpty(path)) return null;

        string cacheKey = $"{path}_{size}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        try
        {
            if (!File.Exists(path))
            {
                _cache[cacheKey] = null;
                return null;
            }

            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".ico" || ext == ".bmp")
            {
                var img = LoadImage(path, size);
                _cache[cacheKey] = img;
                return img;
            }

            ushort iconIndex = 0;
            IntPtr hIcon = ExtractAssociatedIcon(IntPtr.Zero, path, ref iconIndex);
            if (hIcon == IntPtr.Zero)
            {
                _cache[cacheKey] = null;
                return null;
            }

            try
            {
                var bitmap = Imaging.CreateBitmapSourceFromHIcon(
                    hIcon, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(size, size));

                bitmap.Freeze();
                _cache[cacheKey] = bitmap;
                return bitmap;
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }
        catch
        {
            _cache[cacheKey] = null;
            return null;
        }
    }

    public static ImageSource? GetCustomIcon(string path, int size = 48)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

        string cacheKey = $"custom_{path}_{size}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var img = LoadImage(path, size);
        _cache[cacheKey] = img;
        return img;
    }

    private static ImageSource? LoadImage(string path, int size)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.DecodePixelWidth = size;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    public static void ClearCache() => _cache.Clear();
}