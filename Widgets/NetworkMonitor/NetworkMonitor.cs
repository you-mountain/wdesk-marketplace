using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.NetworkMonitor.Style;

namespace WDesk.Widgets.NetworkMonitor;

public class NetworkMonitorWidget : WidgetBase
{
    public NetworkMonitorWidget()
    {
        NetworkMonitorService.Initialize();
    }

    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "networkmonitor",
        NameKey = "widget.networkmonitor.name",
        DescriptionKey = "widget.networkmonitor.desc",
        Category = WidgetCategory.System,
        Icon = "\uE701",
        DefaultWidth = 300,
        DefaultHeight = 260,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}