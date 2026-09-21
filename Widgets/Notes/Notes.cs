using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.Notes.Style;

namespace WDesk.Widgets.Notes;

public class NotesWidget : WidgetBase
{
    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "notes",
        NameKey = "widget.notes.name",
        DescriptionKey = "widget.notes.desc",
        Category = WidgetCategory.Productivity,
        Icon = "\uE70B",   // Fluent: Edit
        DefaultWidth = 320,
        DefaultHeight = 280,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}