using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.Calendar.Style;

namespace WDesk.Widgets.Calendar;

public class CalendarWidget : WidgetBase
{
    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "calendar",
        NameKey = "widget.calendar.name",
        DescriptionKey = "widget.calendar.desc",
        Category = WidgetCategory.Time,
        Icon = "\uE787",   // Fluent: Calendar
        DefaultWidth = 260,
        DefaultHeight = 120,
        HasSettings = true
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}