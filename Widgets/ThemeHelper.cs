using System;
using System.Windows;
using System.Windows.Media;

namespace WDesk.Widgets;

/// <summary>
/// خوندن رنگ‌های تم فعلی از Application.Resources.
/// اگه رنگی نبود، fallback به مقدار پیش‌فرض.
/// باعث می‌شه پنل تنظیمات ویجت‌ها با تم Light/Dark برنامه هماهنگ بشه.
/// </summary>
public static class ThemeHelper
{
    // ═══════════════════════════════════════
    //  Core colors
    // ═══════════════════════════════════════
    public static Color TextPrimary => GetColor("TextPrimary", Color.FromRgb(0xF0, 0xF0, 0xF0));
    public static Color TextSecondary => GetColor("TextSecondary", Color.FromRgb(0xAA, 0xAA, 0xAA));
    public static Color TextTertiary => GetColor("TextMuted", Color.FromRgb(0x77, 0x77, 0x77));
    public static Color BgBase => GetColor("BgBase", Color.FromRgb(0x14, 0x14, 0x16));
    public static Color BgSurface => GetColor("BgSurface", Color.FromRgb(0x1E, 0x1E, 0x22));
    public static Color BgElevated => GetColor("BgElevated", Color.FromRgb(0x28, 0x28, 0x2E));
    public static Color BgHover => GetColor("BgHover", Color.FromRgb(0x32, 0x32, 0x3A));
    public static Color BgInput => GetColor("BgElevated", Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF));
    public static Color BorderBrush => GetColor("BorderBrush", Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
    public static Color BorderLight => GetColor("BorderLightBrush", Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));
    public static Color Divider => GetColor("BorderBrush", Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));
    public static Color Accent => GetColor("AccentColor", Color.FromRgb(0x8F, 0xB3, 0x39));
    public static Color AccentText => Color.FromRgb(0x0A, 0x0B, 0x08);
    public static Color ChipBg => GetColor("BgElevated", Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF));
    public static Color ChipSelected => GetColor("AccentFaded", Color.FromArgb(0x33, 0x8F, 0xB3, 0x39));
    public static Color Danger => GetColor("DangerBrush", Color.FromRgb(0xE0, 0x52, 0x52));
    public static Color Success => GetColor("SuccessBrush", Color.FromRgb(0x6F, 0xBF, 0x4A));
    public static Color Warning => GetColor("WarningBrush", Color.FromRgb(0xF0, 0xA0, 0x40));

    // ═══════════════════════════════════════
    //  Brushes (جدید هر بار ساخته می‌شه تا تغییرات تم آنی اعمال بشه)
    // ═══════════════════════════════════════
    public static SolidColorBrush Brush(Color c) => new(c);

    public static SolidColorBrush TextPrimaryBrush => new(TextPrimary);
    public static SolidColorBrush TextSecondaryBrush => new(TextSecondary);
    public static SolidColorBrush TextTertiaryBrush => new(TextTertiary);
    public static SolidColorBrush BgBaseBrush => new(BgBase);
    public static SolidColorBrush BgSurfaceBrush => new(BgSurface);
    public static SolidColorBrush BgElevatedBrush => new(BgElevated);
    public static SolidColorBrush BgInputBrush => new(BgInput);
    public static SolidColorBrush BorderBrushBrush => new(BorderBrush);
    public static SolidColorBrush BorderLightBrush => new(BorderLight);
    public static SolidColorBrush DividerBrush => new(Divider);
    public static SolidColorBrush AccentBrush => new(Accent);
    public static SolidColorBrush ChipBgBrush => new(ChipBg);
    public static SolidColorBrush ChipSelectedBrush => new(ChipSelected);
    public static SolidColorBrush DangerBrush => new(Danger);
    public static SolidColorBrush SuccessBrush => new(Success);
    public static SolidColorBrush WarningBrush => new(Warning);

    // ═══════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════
    private static Color GetColor(string key, Color fallback)
    {
        try
        {
            var app = Application.Current;
            if (app == null) return fallback;

            // 1. سعی کن از Resources با کلید بگیری
            if (app.TryFindResource(key) is SolidColorBrush brush)
                return brush.Color;

            // 2. اگه Color خالص بود
            if (app.TryFindResource(key) is Color color)
                return color;
        }
        catch { }

        return fallback;
    }

    // ═══════════════════════════════════════
    //  تشخیص تم Light/Dark
    // ═══════════════════════════════════════
    public static bool IsLightTheme
    {
        get
        {
            try
            {
                var bg = BgBase;
                // اگه روشنایی پس‌زمینه > 0.5، تم روشنه
                double brightness = (bg.R * 0.299 + bg.G * 0.587 + bg.B * 0.114) / 255.0;
                return brightness > 0.5;
            }
            catch { return false; }
        }
    }
}