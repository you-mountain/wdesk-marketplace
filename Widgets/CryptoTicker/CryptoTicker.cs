using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.CryptoTicker.Style;

namespace WDesk.Widgets.CryptoTicker;

public class CryptoTickerWidget : WidgetBase
{
    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "cryptoticker",
        NameKey = "widget.cryptoticker.name",
        DescriptionKey = "widget.cryptoticker.desc",
        Category = WidgetCategory.Productivity,
        Icon = "\uE8D6",   // Fluent: Currency
        DefaultWidth = 260,
        DefaultHeight = 140,
        HasSettings = true
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }

    public override FrameworkElement CreateSettingsView(
        PlacedWidget instance,
        Action<System.Collections.Generic.Dictionary<string, string>> onSave)
    {
        return CryptoTickerSettings.Build(instance, onSave);
    }
}