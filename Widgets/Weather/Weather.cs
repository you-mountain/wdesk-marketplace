using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.Weather.Style;

namespace WDesk.Widgets.Weather;

public class WeatherWidget : WidgetBase
{
    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "weather",
        NameKey = "widget.weather.name",
        DescriptionKey = "widget.weather.desc",
        Category = WidgetCategory.Weather,
        Icon = "\uE9CA",
        DefaultWidth = 240,
        DefaultHeight = 220
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}