using System;
using System.Windows;
using WDesk.Core;
using WDesk.Widgets.Music.Style;

namespace WDesk.Widgets.Music;

public class MusicWidget : WidgetBase
{
    public MusicWidget()
    {
        _ = MusicService.InitializeAsync();
    }

    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "music",
        NameKey = "widget.music.name",
        DescriptionKey = "widget.music.desc",
        Category = WidgetCategory.Productivity,
        Icon = "\uE8D6",
        DefaultWidth = 360,
        DefaultHeight = 110,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}