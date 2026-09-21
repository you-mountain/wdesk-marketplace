using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WDesk.Core;

namespace WDesk.Widgets;

/// <summary>
/// کلاس پایه برای همه ویجت‌ها.
/// - ویجت‌های ساده می‌تونن CreateView رو override کنن.
/// - ویجت‌های چند-استایل می‌تونن GetStyles/GetStyleBuilder رو override کنن.
/// </summary>
public abstract class WidgetBase : IWidgetWithStyles
{
    // ═══════════════════════════════════════════
    //  Metadata
    // ═══════════════════════════════════════════
    public abstract WidgetMetadata Metadata { get; }

    // ═══════════════════════════════════════════
    //  Styles
    // ═══════════════════════════════════════════

    /// <summary>
    /// لیست استایل‌های این ویجت. پیش‌فرض: یک استایل به اسم "style1".
    /// </summary>
    public virtual IEnumerable<WStyle> GetStyles()
    {
        return new List<WStyle>
        {
            new()
            {
                Id = "style1",
                Name = "Default",
                Icon = "\uE8F1",
                PreviewEmoji = "🎨"
            }
        };
    }

    /// <summary>
    /// ساخت استایل بر اساس id. پیش‌فرض: null.
    /// اگه ویجت از CreateView استفاده می‌کنه، نیازی به override نیست.
    /// </summary>
    public virtual IStyleBuilder? GetStyleBuilder(string styleId)
    {
        return null;
    }

    /// <summary>
    /// استایل فعلی ویجت.
    /// </summary>
    public string GetCurrentStyle(PlacedWidget instance)
    {
        if (instance.Settings.TryGetValue("style", out var s) &&
            !string.IsNullOrEmpty(s))
            return s;

        return GetStyles().FirstOrDefault()?.Id ?? "";
    }

    // ═══════════════════════════════════════════
    //  ★ CreateView — هوشمند
    // ═══════════════════════════════════════════
    public virtual FrameworkElement CreateView(PlacedWidget instance)
    {
        var style = GetCurrentStyle(instance);
        var builder = GetStyleBuilder(style);

        // اگه استایل پیدا نشد، اولین استایل رو امتحان کن
        if (builder == null)
        {
            var first = GetStyles().FirstOrDefault();
            if (first != null)
                builder = GetStyleBuilder(first.Id);
        }

        return builder?.Build(instance) ?? new Grid();
    }

    // ═══════════════════════════════════════════
    //  Settings
    // ═══════════════════════════════════════════
    public virtual FrameworkElement CreateSettingsView(
        PlacedWidget instance,
        Action<Dictionary<string, string>> onSave)
    {
        return new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    Text = "This widget has no settings.",
                    FontSize = 12,
                    Margin = new Thickness(16)
                }
            }
        };
    }

    public virtual void ApplySettings(
        PlacedWidget instance,
        Dictionary<string, string> settings)
    { }
}