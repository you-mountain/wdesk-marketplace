using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.AppLauncher.Style;

namespace WDesk.Widgets.AppLauncher;

public class AppLauncherWidget : WidgetBase
{
    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "applauncher",
        NameKey = "widget.applauncher.name",
        DescriptionKey = "widget.applauncher.desc",
        Category = WidgetCategory.Productivity,
        Icon = "\uE71D",
        DefaultWidth = 280,
        DefaultHeight = 240,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}