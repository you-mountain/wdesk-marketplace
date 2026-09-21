using System.Windows.Media;
using System.Windows.Media.Effects;

namespace WDesk.Widgets.Themes;

/// <summary>
/// رنگ‌ها و استایل‌های مشترک برای سبک iOS / Neumorphic.
/// </summary>
public static class iOSTheme
{
    // ═══════════════════════════════════════
    //  Colors
    // ═══════════════════════════════════════
    public static readonly Color CardBg = Color.FromRgb(0xFF, 0xFF, 0xFF);
    public static readonly Color CardBgSoft = Color.FromRgb(0xF7, 0xF7, 0xFA);
    public static readonly Color PageBg = Color.FromRgb(0xF0, 0xF0, 0xF5);
    public static readonly Color TextPrimary = Color.FromRgb(0x1C, 0x1C, 0x1E);
    public static readonly Color TextSecondary = Color.FromRgb(0x8E, 0x8E, 0x93);
    public static readonly Color TextTertiary = Color.FromRgb(0xAE, 0xAE, 0xB2);
    public static readonly Color Divider = Color.FromArgb(0x14, 0x00, 0x00, 0x00);

    // ═══ Accent colors ═══
    public static readonly Color Blue = Color.FromRgb(0x0A, 0x84, 0xFF);
    public static readonly Color Green = Color.FromRgb(0x34, 0xC7, 0x59);
    public static readonly Color Red = Color.FromRgb(0xFF, 0x3B, 0x30);
    public static readonly Color Orange = Color.FromRgb(0xFF, 0x95, 0x00);
    public static readonly Color Yellow = Color.FromRgb(0xFF, 0xCC, 0x00);
    public static readonly Color Purple = Color.FromRgb(0xAF, 0x52, 0xDE);
    public static readonly Color Pink = Color.FromRgb(0xFF, 0x2D, 0x55);
    public static readonly Color Teal = Color.FromRgb(0x30, 0xB0, 0xC6);
    public static readonly Color Indigo = Color.FromRgb(0x5E, 0x5C, 0xE6);

    // ═══ Palette for app icons (cycle through) ═══
    public static readonly Color[] AppPalette =
    {
        Blue, Green, Red, Orange, Purple, Pink, Teal, Indigo, Yellow
    };

    // ═══════════════════════════════════════
    //  Brushes
    // ═══════════════════════════════════════
    public static SolidColorBrush Brush(Color c) => new(c);
    public static SolidColorBrush CardBrush => new(CardBg);
    public static SolidColorBrush PageBrush => new(PageBg);
    public static SolidColorBrush TextBrush => new(TextPrimary);
    public static SolidColorBrush SubTextBrush => new(TextSecondary);

    // ═══════════════════════════════════════
    //  Shadows
    // ═══════════════════════════════════════
    public static DropShadowEffect CardShadow() => new()
    {
        BlurRadius = 24,
        ShadowDepth = 2,
        Opacity = 0.10,
        Color = Colors.Black,
        Direction = 270
    };

    public static DropShadowEffect SoftShadow() => new()
    {
        BlurRadius = 16,
        ShadowDepth = 1,
        Opacity = 0.06,
        Color = Colors.Black,
        Direction = 270
    };

    public static DropShadowEffect HoverShadow() => new()
    {
        BlurRadius = 32,
        ShadowDepth = 4,
        Opacity = 0.20,
        Color = Colors.Black,
        Direction = 270
    };

    // ═══════════════════════════════════════
    //  Helper: رنگ بر اساس hash نام
    // ═══════════════════════════════════════
    public static Color GetColorForName(string name)
    {
        if (string.IsNullOrEmpty(name)) return Blue;
        int hash = 0;
        foreach (var c in name) hash = (hash * 31 + c) & 0x7FFFFFFF;
        return AppPalette[hash % AppPalette.Length];
    }
}