using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Weather.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public StyleSettingsSchema GetSettingsSchema() => new()
    {
        Fields = new List<StyleSettingField>
        {
            new() { Key = "city",          Label = "City",           Type = StyleSettingType.Text,   Group = "Location", DefaultValue = "Tehran" },
            new() { Key = "useFahrenheit", Label = "Use Fahrenheit", Type = StyleSettingType.Toggle, Group = "Location", DefaultValue = false },
        }
    };

    public FrameworkElement Build(PlacedWidget instance)
    {
        var settings = instance?.Settings ?? new Dictionary<string, string>();
        string city = GetString(settings, "city", "Tehran");
        bool useF = GetBool(settings, "useFahrenheit", false);

        var root = new Grid();

        // ═══════════════════════════════════════
        //  پس‌زمینه
        // ═══════════════════════════════════════
        var bgBorder = new Border
        {
            CornerRadius = new CornerRadius(28)
        };
        bgBorder.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bgBorder);

        var mainStack = new StackPanel
        {
            Margin = new Thickness(16, 14, 16, 14)
        };

        // ═══════════════════════════════════════
        //  Row 1: Petal Blob + Cloud
        // ═══════════════════════════════════════
        var iconContainer = new Grid
        {
            Height = 96,
            Margin = new Thickness(0, 4, 0, 4)
        };

        // ═══ گلبرگ — با AccentBrush ═══
        var blobPath = new Path
        {
            Width = 88,
            Height = 88,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Data = BuildPetalBlob(100, 6, 0.85)
        };
        blobPath.SetResourceReference(Path.FillProperty, "AccentBrush");
        iconContainer.Children.Add(blobPath);

        // ═══ Cloud (pixel-art) ═══
        var cloudCanvas = new Canvas
        {
            Width = 56,
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        DrawPixelCloud(cloudCanvas, 4, Colors.White, "cloud");
        iconContainer.Children.Add(cloudCanvas);

        mainStack.Children.Add(iconContainer);

        // ═══════════════════════════════════════
        //  Row 2: Chip
        // ═══════════════════════════════════════
        var chipBorder = new Border
        {
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 6, 0, 6)
        };
        chipBorder.SetResourceReference(Border.BackgroundProperty, "WidgetBgElevated");
        mainStack.Children.Add(chipBorder);

        var chipGrid = new Grid();
        chipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        chipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        chipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // ── Left: 0% + sun ──
        var leftStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var pctText = new TextBlock
        {
            Text = "0%",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        pctText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        leftStack.Children.Add(pctText);

        var sunIcon = new TextBlock
        {
            Text = "☀",
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center
        };
        sunIcon.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        leftStack.Children.Add(sunIcon);

        Grid.SetColumn(leftStack, 0);
        chipGrid.Children.Add(leftStack);

        // ── Right: time + thermometer ──
        var rightStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var timeText = new TextBlock
        {
            Text = DateTime.Now.ToString("HH:mm"),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        timeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        rightStack.Children.Add(timeText);

        var thermoIcon = new TextBlock
        {
            Text = "🌡",
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center
        };
        thermoIcon.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        rightStack.Children.Add(thermoIcon);

        Grid.SetColumn(rightStack, 2);
        chipGrid.Children.Add(rightStack);

        chipBorder.Child = chipGrid;

        // ═══════════════════════════════════════
        //  Row 3: Today | Sun
        // ═══════════════════════════════════════
        var columnsGrid = new Grid
        {
            Height = 60,
            Margin = new Thickness(0, 6, 0, 0)
        };
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var leftCol = BuildDayColumn("Today");
        Grid.SetColumn(leftCol, 0);
        columnsGrid.Children.Add(leftCol);

        var divider = new Border
        {
            Width = 1,
            Height = 40,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0)
        };
        divider.SetResourceReference(Border.BackgroundProperty, "WidgetBgElevated");
        Grid.SetColumn(divider, 1);
        columnsGrid.Children.Add(divider);

        var rightCol = BuildDayColumn("Sun");
        Grid.SetColumn(rightCol, 2);
        columnsGrid.Children.Add(rightCol);

        mainStack.Children.Add(columnsGrid);

        root.Children.Add(mainStack);

        // ═══════════════════════════════════════
        //  Update Data
        // ═══════════════════════════════════════
        async void UpdateData()
        {
            try
            {
                var data = await WeatherService.GetAsync(city);
                if (data == null) return;

                double todayTemp = useF ? data.ToF(data.Temperature) : data.Temperature;
                double tomorrowTemp = useF ? data.ToF(data.TempMin) : data.TempMin;

                if (leftCol.Children.Count > 1 && leftCol.Children[1] is StackPanel leftRow)
                {
                    if (leftRow.Children.Count > 1 && leftRow.Children[1] is TextBlock t1)
                        t1.Text = $"{Math.Round(todayTemp)}°";
                }

                if (rightCol.Children.Count > 1 && rightCol.Children[1] is StackPanel rightRow)
                {
                    if (rightRow.Children.Count > 1 && rightRow.Children[1] is TextBlock t2)
                        t2.Text = $"{Math.Round(tomorrowTemp)}°";
                }

                string iconType = WeatherMapper.GetIconType(data.WeatherCode);
                DrawPixelCloud(cloudCanvas, 4, Colors.White, iconType);

                timeText.Text = DateTime.Now.ToString("HH:mm");
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Weather update failed", ex);
            }
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
        timer.Tick += (_, _) => UpdateData();
        timer.Start();

        root.Loaded += (_, _) => UpdateData();
        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }

    // ═══════════════════════════════════════════
    //  Day Column
    // ═══════════════════════════════════════════
    private static StackPanel BuildDayColumn(string dayLabel)
    {
        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var dayText = new TextBlock
        {
            Text = dayLabel,
            FontSize = 11,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        dayText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        stack.Children.Add(dayText);

        var tempRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0)
        };

        // ── Thermometer icon ──
        tempRow.Children.Add(MakeThermometerIcon());

        var tempText = new TextBlock
        {
            Text = "—°",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 0, 0)
        };
        tempText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        tempRow.Children.Add(tempText);

        stack.Children.Add(tempRow);
        return stack;
    }

    // ═══════════════════════════════════════════
    //  Thermometer Icon
    // ═══════════════════════════════════════════
    private static FrameworkElement MakeThermometerIcon()
    {
        var canvas = new Canvas
        {
            Width = 10,
            Height = 14,
            VerticalAlignment = VerticalAlignment.Center
        };

        var brush = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

        var stem = new Rectangle
        {
            Width = 2,
            Height = 9,
            Fill = brush,
            RadiusX = 1,
            RadiusY = 1
        };
        Canvas.SetLeft(stem, 4);
        Canvas.SetTop(stem, 0);
        canvas.Children.Add(stem);

        var bulb = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = brush
        };
        Canvas.SetLeft(bulb, 2);
        Canvas.SetTop(bulb, 8);
        canvas.Children.Add(bulb);

        return canvas;
    }

    // ═══════════════════════════════════════════
    //  Petal Blob
    // ═══════════════════════════════════════════
    private static Geometry BuildPetalBlob(double size, int petals, double smoothness)
    {
        double cx = size / 2;
        double cy = size / 2;
        double outerR = size / 2 * 0.95;
        double innerR = outerR * (1 - (1 - smoothness) * 0.15);

        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            double startAngle = -Math.PI / 2;
            var startPoint = new Point(
                cx + outerR * Math.Cos(startAngle),
                cy + outerR * Math.Sin(startAngle));

            ctx.BeginFigure(startPoint, isFilled: true, isClosed: true);

            double angleStep = Math.PI * 2 / petals;
            double halfStep = angleStep / 2;

            for (int i = 0; i < petals; i++)
            {
                double tipAngle = startAngle + i * angleStep;
                double valleyAngle = tipAngle + halfStep;
                double nextTipAngle = tipAngle + angleStep;

                var valleyPoint = new Point(
                    cx + innerR * Math.Cos(valleyAngle),
                    cy + innerR * Math.Sin(valleyAngle));

                var nextTipPoint = new Point(
                    cx + outerR * Math.Cos(nextTipAngle),
                    cy + outerR * Math.Sin(nextTipAngle));

                double tipCtrlR = outerR * 1.02;
                var tipCtrl = new Point(
                    cx + tipCtrlR * Math.Cos(tipAngle + halfStep * 0.3),
                    cy + tipCtrlR * Math.Sin(tipAngle + halfStep * 0.3));

                double valleyCtrlR = innerR * 0.98;
                var valleyCtrl = new Point(
                    cx + valleyCtrlR * Math.Cos(valleyAngle - halfStep * 0.2),
                    cy + valleyCtrlR * Math.Sin(valleyAngle - halfStep * 0.2));

                ctx.BezierTo(tipCtrl, valleyCtrl, valleyPoint, true, true);

                var valleyCtrl2 = new Point(
                    cx + valleyCtrlR * Math.Cos(valleyAngle + halfStep * 0.2),
                    cy + valleyCtrlR * Math.Sin(valleyAngle + halfStep * 0.2));

                double nextTipCtrlR = outerR * 1.02;
                var nextTipCtrl = new Point(
                    cx + nextTipCtrlR * Math.Cos(nextTipAngle - halfStep * 0.3),
                    cy + nextTipCtrlR * Math.Sin(nextTipAngle - halfStep * 0.3));

                ctx.BezierTo(valleyCtrl2, nextTipCtrl, nextTipPoint, true, true);
            }
        }

        geometry.Freeze();
        return geometry;
    }

    // ═══════════════════════════════════════════
    //  Pixel Cloud
    // ═══════════════════════════════════════════
    private static void DrawPixelCloud(Canvas canvas, int pixelSize, Color color, string iconType = "cloud")
    {
        canvas.Children.Clear();

        int[][] pattern = iconType switch
        {
            "sun" => new[]
            {
                new[] { 0,0,0,1,0,0,0 },
                new[] { 0,1,0,0,0,1,0 },
                new[] { 0,0,1,1,1,0,0 },
                new[] { 1,0,1,1,1,0,1 },
                new[] { 0,0,1,1,1,0,0 },
                new[] { 0,1,0,0,0,1,0 },
                new[] { 0,0,0,1,0,0,0 },
            },
            "rain" => new[]
            {
                new[] { 0,0,0,1,1,1,0,0 },
                new[] { 0,0,1,1,1,1,1,0 },
                new[] { 0,1,1,1,1,1,1,1 },
                new[] { 1,1,1,1,1,1,1,1 },
                new[] { 0,1,1,1,1,1,1,0 },
                new[] { 0,0,0,0,0,0,0,0 },
                new[] { 0,0,1,0,1,0,0,0 },
                new[] { 0,1,0,1,0,1,0,0 },
            },
            "snow" => new[]
            {
                new[] { 0,0,0,1,1,1,0,0 },
                new[] { 0,0,1,1,1,1,1,0 },
                new[] { 0,1,1,1,1,1,1,1 },
                new[] { 1,1,1,1,1,1,1,1 },
                new[] { 0,1,1,1,1,1,1,0 },
                new[] { 0,0,0,0,0,0,0,0 },
                new[] { 0,0,1,0,1,0,0,0 },
                new[] { 0,1,0,1,0,1,0,0 },
                new[] { 0,0,1,0,1,0,0,0 },
            },
            "thunder" => new[]
            {
                new[] { 0,0,0,1,1,1,0,0 },
                new[] { 0,0,1,1,1,1,1,0 },
                new[] { 0,1,1,1,1,1,1,1 },
                new[] { 1,1,1,1,1,1,1,1 },
                new[] { 0,1,1,1,1,1,1,0 },
                new[] { 0,0,0,0,0,1,0,0 },
                new[] { 0,0,0,0,1,1,0,0 },
                new[] { 0,0,0,1,1,0,0,0 },
                new[] { 0,0,1,1,0,0,0,0 },
            },
            "partly" => new[]
            {
                new[] { 0,0,0,0,0,1,1,0 },
                new[] { 0,0,0,0,1,1,0,0 },
                new[] { 0,0,0,0,0,0,0,0 },
                new[] { 0,0,0,1,1,1,0,0 },
                new[] { 0,0,1,1,1,1,1,0 },
                new[] { 0,1,1,1,1,1,1,1 },
                new[] { 0,1,1,1,1,1,1,1 },
                new[] { 0,0,1,1,1,1,1,0 },
            },
            "fog" => new[]
            {
                new[] { 0,0,1,1,1,1,0,0 },
                new[] { 0,1,1,1,1,1,1,0 },
                new[] { 1,1,1,1,1,1,1,1 },
                new[] { 0,0,0,0,0,0,0,0 },
                new[] { 1,1,1,1,1,1,1,1 },
                new[] { 0,0,0,0,0,0,0,0 },
                new[] { 1,1,1,1,1,1,1,1 },
            },
            _ => new[]
            {
                new[] { 0,0,0,0,0,0,1,1,1,0 },
                new[] { 0,0,0,0,0,1,1,1,1,1 },
                new[] { 0,0,0,1,1,1,1,1,1,1 },
                new[] { 0,1,1,1,1,1,1,1,1,1 },
                new[] { 1,1,1,1,1,1,1,1,1,1 },
                new[] { 1,1,1,1,1,1,1,1,1,1 },
                new[] { 0,1,1,1,1,1,1,1,1,0 },
                new[] { 0,0,1,1,1,1,1,1,0,0 },
            }
        };

        var brush = new SolidColorBrush(color);
        double spacing = pixelSize * 1.15;

        int rows = pattern.Length;
        int cols = pattern[0].Length;
        double patternWidth = cols * spacing;
        double patternHeight = rows * spacing;

        double offsetX = (canvas.Width - patternWidth) / 2;
        double offsetY = (canvas.Height - patternHeight) / 2;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (pattern[row][col] == 1)
                {
                    var dot = new Ellipse
                    {
                        Width = pixelSize,
                        Height = pixelSize,
                        Fill = brush
                    };
                    Canvas.SetLeft(dot, offsetX + col * spacing);
                    Canvas.SetTop(dot, offsetY + row * spacing);
                    canvas.Children.Add(dot);
                }
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════
    private static string GetString(Dictionary<string, string> s, string key, string fallback)
    {
        if (s != null && s.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v))
            return v;
        return fallback;
    }

    private static bool GetBool(Dictionary<string, string> s, string key, bool fallback)
    {
        if (s != null && s.TryGetValue(key, out var v))
            return v.Equals("true", StringComparison.OrdinalIgnoreCase);
        return fallback;
    }
}