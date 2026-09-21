using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WDesk.Core;

namespace WDesk.Widgets.AppLauncher;

internal static class AppLauncherCommon
{
    // ═══════════════════════════════════════════
    //  Launch
    // ═══════════════════════════════════════════
    public static bool Launch(AppItem app, out string errorMessage)
    {
        errorMessage = "";

        try
        {
            if (app == null) { errorMessage = "App is null"; return false; }
            if (string.IsNullOrEmpty(app.Path)) { errorMessage = "Path is empty"; return false; }
            if (!File.Exists(app.Path)) { errorMessage = $"File not found:\n{app.Path}"; return false; }

            var workingDir = !string.IsNullOrEmpty(app.WorkingDirectory)
                ? app.WorkingDirectory
                : Path.GetDirectoryName(app.Path) ?? "";

            var psi = new ProcessStartInfo
            {
                FileName = app.Path,
                UseShellExecute = true,
                WorkingDirectory = workingDir
            };

            Process.Start(psi);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"{ex.GetType().Name}: {ex.Message}";
            try { App.Logger?.Error($"Launch failed: {app?.Path}", ex); } catch { }
            return false;
        }
    }

    // ═══════════════════════════════════════════
    //  Parse / Serialize
    // ═══════════════════════════════════════════
    public static List<AppItem> ParseApps(string raw)
    {
        var list = new List<AppItem>();
        if (string.IsNullOrWhiteSpace(raw)) return list;

        var lines = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var parts = trimmed.Split('|');
            if (parts.Length >= 2)
            {
                list.Add(new AppItem
                {
                    Name = parts[0].Trim(),
                    Path = parts[1].Trim()
                });
            }
            else if (parts.Length == 1)
            {
                var path = parts[0].Trim();
                list.Add(new AppItem
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    Path = path
                });
            }
        }

        return list;
    }

    public static string SerializeApps(IEnumerable<AppItem> apps)
    {
        return string.Join("\n", apps.Select(a => $"{a.Name}|{a.Path}"));
    }

    // ═══════════════════════════════════════════
    //  ★ Add App Dialog (با ContextMenu استفاده می‌شه)
    // ═══════════════════════════════════════════
    public static AppItem? ShowAddAppDialog()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Choose an application",
            Filter = "Applications|*.exe;*.lnk;*.bat;*.cmd|All files|*.*",
            CheckFileExists = true
        };

        if (dlg.ShowDialog() != true) return null;

        string realPath = ResolveShortcut(dlg.FileName);
        string displayName = Path.GetFileNameWithoutExtension(dlg.FileName);

        return new AppItem
        {
            Name = displayName,
            Path = realPath
        };
    }

    // ═══════════════════════════════════════════
    //  ★ Remove App from Widget Settings
    // ═══════════════════════════════════════════
    public static void RemoveAppFromWidget(
        PlacedWidget widget,
        string appPath,
        Action onSuccess = null)
    {
        try
        {
            if (widget == null || widget.Settings == null) return;

            string currentRaw = widget.Settings.TryGetValue("apps", out var v) ? v : "";
            var apps = ParseApps(currentRaw);

            // حذف اپ با مسیر مشخص
            var removed = apps.RemoveAll(a =>
                string.Equals(a.Path, appPath, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
            {
                widget.Settings["apps"] = SerializeApps(apps);
                App.Settings.Save();

                App.Logger.Info($"App removed: {appPath}");
                App.Notifications.Show("WDesk", "App removed", NotificationType.Success);

                onSuccess?.Invoke();
            }
        }
        catch (Exception ex)
        {
            App.Logger.Error("RemoveAppFromWidget failed", ex);
            App.Notifications.Show("WDesk",
                $"Remove failed: {ex.Message}", NotificationType.Error);
        }
    }

    // ═══════════════════════════════════════════
    //  ★ Add App to Widget Settings
    // ═══════════════════════════════════════════
    public static void AddAppToWidget(
        PlacedWidget widget,
        Action onSuccess = null)
    {
        try
        {
            var app = ShowAddAppDialog();
            if (app == null) return;

            if (widget == null || widget.Settings == null) return;

            string currentRaw = widget.Settings.TryGetValue("apps", out var v) ? v : "";
            var apps = ParseApps(currentRaw);

            apps.Add(app);
            widget.Settings["apps"] = SerializeApps(apps);
            App.Settings.Save();

            App.Logger.Info($"App added: {app.Path}");
            App.Notifications.Show("WDesk",
                $"{app.Name} added", NotificationType.Success);

            onSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            App.Logger.Error("AddAppToWidget failed", ex);
            App.Notifications.Show("WDesk",
                $"Add failed: {ex.Message}", NotificationType.Error);
        }
    }

    // ═══════════════════════════════════════════
    //  Resolve .lnk Shortcut
    // ═══════════════════════════════════════════
    private static string ResolveShortcut(string path)
    {
        if (!path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            return path;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return path;

            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(path);
            string target = shortcut.TargetPath;

            if (!string.IsNullOrEmpty(target) && File.Exists(target))
                return target;
        }
        catch { }

        return path;
    }

    // ═══════════════════════════════════════════
    //  Get App Color
    // ═══════════════════════════════════════════
    public static Color GetAppColor(AppItem app)
    {
        if (!string.IsNullOrEmpty(app.Name))
        {
            int hash = 0;
            foreach (var c in app.Name) hash = (hash * 31 + c) & 0x7FFFFFFF;

            var palette = new[]
            {
                Color.FromRgb(0x8F, 0xB3, 0x39),
                Color.FromRgb(0x4F, 0xC3, 0xF7),
                Color.FromRgb(0xE7, 0x4C, 0x3C),
                Color.FromRgb(0x9B, 0x59, 0xB6),
                Color.FromRgb(0xFF, 0x98, 0x00),
                Color.FromRgb(0x00, 0xBC, 0xD4),
                Color.FromRgb(0xE9, 0x1E, 0x63),
            };

            return palette[hash % palette.Length];
        }

        return Color.FromRgb(0x8F, 0xB3, 0x39);
    }
}