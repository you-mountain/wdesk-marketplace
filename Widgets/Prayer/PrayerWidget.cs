using System.Windows;
using WDesk.Core;
using WDesk.Widgets.Prayer.Style;

namespace WDesk.Widgets.Prayer;

public class PrayerWidget : WidgetBase
{
    public PrayerWidget()
    {
        PrayerService.Initialize();
    }

    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "prayer",
        NameKey = "widget.prayer.name",
        DescriptionKey = "widget.prayer.desc",
        Category = WidgetCategory.Islamic,
        Icon = "\uE8C0",
        DefaultWidth = 280,
        DefaultHeight = 220,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}