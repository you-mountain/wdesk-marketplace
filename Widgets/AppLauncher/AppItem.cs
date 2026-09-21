using System;

namespace WDesk.Widgets.AppLauncher;

public class AppItem
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string? CustomIconPath { get; set; }
    public string? WorkingDirectory { get; set; }
    public string? Arguments { get; set; }
    public string? Group { get; set; }        // دسته‌بندی
    public string? ColorHex { get; set; }     // رنگ fallback آیکون

    public string Serialize()
    {
        // فرمت: Name|Path|Icon|Group|Color|Args
        return string.Join("|",
            Escape(Name),
            Escape(Path),
            Escape(CustomIconPath ?? ""),
            Escape(Group ?? ""),
            Escape(ColorHex ?? ""),
            Escape(Arguments ?? ""));
    }

    public static AppItem? Deserialize(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        var parts = line.Split('|');
        if (parts.Length < 2) return null;

        var item = new AppItem
        {
            Name = Unescape(parts[0]),
            Path = Unescape(parts[1])
        };

        if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
            item.CustomIconPath = Unescape(parts[2]);
        if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]))
            item.Group = Unescape(parts[3]);
        if (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]))
            item.ColorHex = Unescape(parts[4]);
        if (parts.Length > 5 && !string.IsNullOrEmpty(parts[5]))
            item.Arguments = Unescape(parts[5]);

        return item;
    }

    private static string Escape(string s) => s.Replace("|", "\\|");
    private static string Unescape(string s) => s.Replace("\\|", "|");
}