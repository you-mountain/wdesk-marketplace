using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace WDesk.Widgets.Themes;

/// <summary>
/// تم NoThink — سرمه‌ای تیره + سبز برند.
/// </summary>
public static class NoThinkTheme
{
    // ═══════════════════════════════════════════
    //  Core Colors
    // ═══════════════════════════════════════════
    public static readonly Color BgDark = Color.FromRgb(0x1A, 0x1D, 0x21);
    public static readonly Color BgSurface = Color.FromRgb(0x22, 0x26, 0x2B);
    public static readonly Color BgElevated = Color.FromRgb(0x2A, 0x2F, 0x35);
    public static readonly Color BgOverlay = Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF);

    // ═══ Accent سبز برند ═══
    public static readonly Color Accent = Color.FromRgb(0x8F, 0xB3, 0x39);   // سبز برند
    public static readonly Color AccentDark = Color.FromRgb(0x6F, 0x8F, 0x2A);
    public static readonly Color AccentLight = Color.FromRgb(0xA8, 0xCC, 0x4F);
    public static readonly Color AccentSoft = Color.FromArgb(0x33, 0x8F, 0xB3, 0x39);

    // ═══ Text ═══
    public static readonly Color TextPrimary = Color.FromRgb(0xFF, 0xFF, 0xFF);
    public static readonly Color TextSecondary = Color.FromRgb(0x9A, 0xA0, 0xA6);
    public static readonly Color TextMuted = Color.FromRgb(0x6A, 0x70, 0x76);

    // ═══ Semantic ═══
    public static readonly Color Red = Color.FromRgb(0xE6, 0x39, 0x46);
    public static readonly Color Orange = Color.FromRgb(0xFF, 0x95, 0x00);
    public static readonly Color Blue = Color.FromRgb(0x0A, 0x84, 0xFF);

    // ═══════════════════════════════════════════
    //  Brushes
    // ═══════════════════════════════════════════
    public static SolidColorBrush Brush(Color c) => new(c);

    public static SolidColorBrush BgDarkBrush => new(BgDark);
    public static SolidColorBrush BgSurfaceBrush => new(BgSurface);
    public static SolidColorBrush BgElevatedBrush => new(BgElevated);
    public static SolidColorBrush AccentBrush => new(Accent);
    public static SolidColorBrush AccentDarkBrush => new(AccentDark);
    public static SolidColorBrush AccentLightBrush => new(AccentLight);
    public static SolidColorBrush AccentSoftBrush => new(AccentSoft);
    public static SolidColorBrush TextPrimaryBrush => new(TextPrimary);
    public static SolidColorBrush TextSecondaryBrush => new(TextSecondary);
    public static SolidColorBrush TextMutedBrush => new(TextMuted);
    public static SolidColorBrush RedBrush => new(Red);
    public static SolidColorBrush OverlayBrush => new(BgOverlay);

    // ═══════════════════════════════════════════
    //  Shadows
    // ═══════════════════════════════════════════
    public static DropShadowEffect CardShadow() => new()
    {
        BlurRadius = 24,
        ShadowDepth = 4,
        Opacity = 0.35,
        Color = Colors.Black,
        Direction = 270
    };

    public static DropShadowEffect SoftShadow() => new()
    {
        BlurRadius = 12,
        ShadowDepth = 2,
        Opacity = 0.20,
        Color = Colors.Black,
        Direction = 270
    };

    // ═══════════════════════════════════════════
    //  Fonts
    // ═══════════════════════════════════════════
    public static FontFamily LightFont => new("Segoe UI Variable Display Light, Segoe UI Light, Segoe UI");
    public static FontFamily MainFont => new("Segoe UI Variable, Segoe UI");
    public static FontFamily IconFont => new("Segoe Fluent Icons, Segoe MDL2 Assets");

    // ═══════════════════════════════════════════
    //  Card
    // ═══════════════════════════════════════════
    public static Border MakeCard(double radius = 24, bool withShadow = true)
    {
        var card = new Border
        {
            Background = BgDarkBrush,
            CornerRadius = new CornerRadius(radius)
        };

        if (withShadow)
            card.Effect = CardShadow();

        return card;
    }

    // ═══════════════════════════════════════════
    //  Text
    // ═══════════════════════════════════════════
    public static TextBlock MakeText(
        string text,
        double size = 14,
        bool bold = false,
        Brush? foreground = null,
        HorizontalAlignment align = HorizontalAlignment.Left)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = size,
            FontFamily = MainFont,
            FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
            Foreground = foreground ?? TextPrimaryBrush,
            HorizontalAlignment = align,
            TextAlignment = align == HorizontalAlignment.Center
                ? TextAlignment.Center
                : TextAlignment.Left
        };
    }
}