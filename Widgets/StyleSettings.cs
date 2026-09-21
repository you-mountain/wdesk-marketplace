using System.Collections.Generic;

namespace WDesk.Widgets;

public enum StyleSettingType
{
    Color,
    Text,
    Number,
    Toggle,
    Choice,
    Image,
    AppsList,   
}

public class StyleSettingField
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public StyleSettingType Type { get; set; } = StyleSettingType.Color;
    public object? DefaultValue { get; set; }
    public string? Group { get; set; }
    public List<string>? Choices { get; set; }
    public double Min { get; set; } = 0;
    public double Max { get; set; } = 100;
}

public class StyleSettingsSchema
{
    public List<StyleSettingField> Fields { get; set; } = new();
}