using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Clock.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public StyleSettingsSchema GetSettingsSchema() => new()
    {
        Fields = new List<StyleSettingField>
        {
            new() { Key = "use24Hour", Label = "24-Hour Format", Type = StyleSettingType.Toggle, Group = "Clock", DefaultValue = true },
            new() { Key = "showAmPm",  Label = "Show AM/PM",     Type = StyleSettingType.Toggle, Group = "Clock", DefaultValue = false },
        }
    };

    public FrameworkElement Build(PlacedWidget instance)
    {
        var settings = instance?.Settings ?? new Dictionary<string, string>();

        bool use24 = GetBool(settings, "use24Hour", true);
        bool showAmPm = GetBool(settings, "showAmPm", false);

        var root = new Grid();

        // ═══════════════════════════════════════
        //  پس‌زمینه — Pill
        // ═══════════════════════════════════════
        var bgBorder = new Border
        {
            CornerRadius = new CornerRadius(48)
        };
        bgBorder.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bgBorder);

        // ═══════════════════════════════════════
        //  Main Stack
        // ═══════════════════════════════════════
        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8)
        };

        // ── ساعت ──
        var hourText = new TextBlock
        {
            Text = "00",
            FontSize = 56,
            FontWeight = FontWeights.Light,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Arial"),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            LineHeight = 56
        };
        hourText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        stack.Children.Add(hourText);

        // ── دقیقه ──
        var minuteText = new TextBlock
        {
            Text = "00",
            FontSize = 56,
            FontWeight = FontWeights.Light,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Arial"),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            LineHeight = 56
        };
        minuteText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(minuteText);

        // ── AM/PM (اختیاری) ──
        var ampmText = new TextBlock
        {
            Text = "",
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Arial"),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0),
            Visibility = showAmPm && !use24 ? Visibility.Visible : Visibility.Collapsed
        };
        ampmText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(ampmText);

        root.Children.Add(stack);

        // ═══════════════════════════════════════
        //  Update Clock
        // ═══════════════════════════════════════
        void UpdateClock()
        {
            var now = DateTime.Now;

            if (use24)
            {
                hourText.Text = now.ToString("HH");
                minuteText.Text = now.ToString("mm");
                ampmText.Visibility = Visibility.Collapsed;
            }
            else
            {
                int hour12 = now.Hour % 12;
                if (hour12 == 0) hour12 = 12;

                hourText.Text = hour12.ToString("00");
                minuteText.Text = now.ToString("mm");

                if (showAmPm)
                {
                    ampmText.Text = now.Hour < 12 ? "AM" : "PM";
                    ampmText.Visibility = Visibility.Visible;
                }
                else
                {
                    ampmText.Visibility = Visibility.Collapsed;
                }
            }
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => UpdateClock();
        timer.Start();

        UpdateClock();

        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }

    private static bool GetBool(Dictionary<string, string> s, string key, bool fallback)
    {
        if (s != null && s.TryGetValue(key, out var v))
            return v.Equals("true", StringComparison.OrdinalIgnoreCase);
        return fallback;
    }
}