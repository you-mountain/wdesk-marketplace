using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Prayer.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var root = new Grid();

        var bg = new Border { CornerRadius = new CornerRadius(20) };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        var stack = new StackPanel { Margin = new Thickness(16, 14, 16, 14) };

        // Header
        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

        var titleText = new TextBlock
        {
            Text = "🕌 PRAYER TIMES",
            FontSize = 10,
            FontWeight = FontWeights.SemiBold
        };
        titleText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        header.Children.Add(titleText);

        var nextText = new TextBlock
        {
            Text = "Loading...",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 6, 0, 0)
        };
        nextText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        header.Children.Add(nextText);

        var countdownText = new TextBlock
        {
            Text = "",
            FontSize = 11,
            Margin = new Thickness(0, 2, 0, 0)
        };
        countdownText.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        header.Children.Add(countdownText);

        stack.Children.Add(header);

        // Prayer List
        var listPanel = new StackPanel();
        var prayerRows = new Dictionary<string, TextBlock>();

        foreach (var name in new[] { "Fajr", "Sunrise", "Dhuhr", "Asr", "Maghrib", "Isha" })
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock { Text = name, FontSize = 11 };
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
            Grid.SetColumn(nameText, 0);
            row.Children.Add(nameText);

            var timeText = new TextBlock
            {
                Text = "—",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            timeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
            Grid.SetColumn(timeText, 1);
            row.Children.Add(timeText);

            prayerRows[name] = timeText;
            listPanel.Children.Add(row);
        }

        stack.Children.Add(listPanel);
        root.Children.Add(stack);

        // Timer
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        timer.Tick += (_, _) =>
        {
            try
            {
                var d = PrayerService.GetCurrent();
                PrayerService.CalculateNext();

                if (!string.IsNullOrEmpty(d.NextPrayerName))
                {
                    var expected = $"{d.NextPrayerName}  •  {d.NextPrayerTime}";
                    if (nextText.Text != expected)
                        nextText.Text = expected;

                    var cd = d.DisplayCountdown;
                    var expectedCd = string.IsNullOrEmpty(cd) ? "" : $"in {cd}";
                    if (countdownText.Text != expectedCd)
                        countdownText.Text = expectedCd;
                }

                foreach (var p in d.Prayers)
                {
                    if (!prayerRows.TryGetValue(p.Name, out var tb)) continue;

                    if (tb.Text != p.Time) tb.Text = p.Time;

                    if (p.IsNext)
                        tb.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
                    else
                        tb.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
                }
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Prayer timer failed", ex);
            }
        };
        timer.Start();

        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }
}