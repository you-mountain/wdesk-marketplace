using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Calendar.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var root = new Grid();

        // ═══ پس‌زمینه (Pill) ═══
        var bgBorder = new Border
        {
            CornerRadius = new CornerRadius(28)
        };
        bgBorder.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bgBorder);

        // ═══ کانتینر Clip ═══
        var clipGrid = new Grid { ClipToBounds = true };
        var clipBorder = new Border
        {
            CornerRadius = new CornerRadius(28),
            ClipToBounds = true
        };
        clipBorder.Child = clipGrid;
        root.Children.Add(clipBorder);

        // ═══ محتوا ═══
        var grid = new Grid
        {
            Margin = new Thickness(20, 12, 16, 10)
        };

        // ── سمت چپ ──
        var leftStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var dayNameText = new TextBlock
        {
            Text = "Saturday",
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Arial")
        };
        dayNameText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        leftStack.Children.Add(dayNameText);

        var monthYearText = new TextBlock
        {
            Text = "Apr 2025",
            FontSize = 11,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Arial"),
            Margin = new Thickness(0, 3, 0, 0)
        };
        monthYearText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        leftStack.Children.Add(monthYearText);

        grid.Children.Add(leftStack);

        // ── سمت راست: عدد بزرگ ──
        var dayNumberText = new TextBlock
        {
            Text = "26",
            FontSize = 80,
            FontWeight = FontWeights.Light,
            FontFamily = new FontFamily("Segoe UI Variable Display Light, Segoe UI Light, Segoe UI"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, -15, -35)
        };
        dayNumberText.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        grid.Children.Add(dayNumberText);

        clipGrid.Children.Add(grid);

        // ═══ Update ═══
        void UpdateDate()
        {
            var now = DateTime.Now;
            dayNameText.Text = now.ToString("dddd", CultureInfo.InvariantCulture);
            monthYearText.Text = now.ToString("MMM yyyy", CultureInfo.InvariantCulture);
            dayNumberText.Text = now.Day.ToString();
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        timer.Tick += (_, _) => UpdateDate();
        timer.Start();

        UpdateDate();

        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }
}