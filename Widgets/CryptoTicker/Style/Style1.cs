using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.CryptoTicker.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var settings = instance?.Settings ?? new Dictionary<string, string>();

        // ═══ Read settings ═══
        string coinId = GetString(settings, "coin", "bitcoin");

        // ★ پیش‌فرض هوشمند: ایرانی → toman، خارجی → usd
        string defaultVs = IsIranianRegion() ? "toman" : "usd";
        string vsCode = GetString(settings, "vs", defaultVs);

        // ═══ Root ═══
        var root = new Grid();

        // ═══ Background ═══
        var bg = new Border
        {
            CornerRadius = new CornerRadius(20)
        };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══════════════════════════════════════
        //  Layout
        // ═══════════════════════════════════════
        var stack = new StackPanel
        {
            Margin = new Thickness(18, 14, 18, 14)
        };

        // ── Header: Coin Name + Symbol ──
        var headerRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var coinNameText = new TextBlock
        {
            Text = "Bitcoin",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold
        };
        coinNameText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        headerRow.Children.Add(coinNameText);

        var coinSymbolText = new TextBlock
        {
            Text = "BTC",
            FontSize = 11,
            FontWeight = FontWeights.Medium,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 2, 0, 0)
        };
        coinSymbolText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        headerRow.Children.Add(coinSymbolText);

        stack.Children.Add(headerRow);

        // ── Price ──
        var priceText = new TextBlock
        {
            Text = "—",
            FontSize = 32,
            FontWeight = FontWeights.Light,
            FontFamily = new FontFamily("Segoe UI Variable Display Light, Segoe UI Light"),
            Margin = new Thickness(0, 8, 0, 0)
        };
        priceText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        stack.Children.Add(priceText);

        // ── Change ──
        var changeText = new TextBlock
        {
            Text = "—",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 4, 0, 0)
        };
        changeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(changeText);

        // ── Updated Time ──
        var timeText = new TextBlock
        {
            Text = "",
            FontSize = 10,
            Margin = new Thickness(0, 10, 0, 0)
        };
        timeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(timeText);

        root.Children.Add(stack);

        // ═══════════════════════════════════════
        //  Update Price
        // ═══════════════════════════════════════
        async void UpdatePrice()
        {
            try
            {
                App.Logger?.Info($"Crypto UI: fetching {coinId}/{vsCode}...");

                var data = await CryptoTickerService.GetPriceAsync(coinId, vsCode, forceRefresh: true);

                if (data == null)
                {
                    App.Logger?.Warn("Crypto UI: no data");
                    return;
                }

                if (!data.HasData)
                {
                    App.Logger?.Warn("Crypto UI: HasData = false");
                    return;
                }

                App.Logger?.Info($"Crypto UI: got {data.CoinName} {data.Price} {data.VsCurrency} ({data.Source})");

                coinNameText.Text = data.CoinName;
                coinSymbolText.Text = data.CoinSymbol;
                priceText.Text = $"{data.VsSymbol}{data.DisplayPrice}";
                changeText.Text = data.DisplayChange;

                if (data.IsUp)
                    changeText.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
                else
                    changeText.SetResourceReference(TextBlock.ForegroundProperty, "DangerBrush");

                timeText.Text = $"Updated: {data.UpdatedAt:HH:mm:ss}  •  {data.Source}";
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Crypto UI: UpdatePrice failed", ex);
            }
        }

        // ═══ Timer ═══
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        timer.Tick += (_, _) => UpdatePrice();
        timer.Start();

        root.Loaded += (_, _) => UpdatePrice();
        root.Unloaded += (_, _) => timer.Stop();

        return root;
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

    private static bool IsIranianRegion()
    {
        try
        {
            var region = System.Globalization.RegionInfo.CurrentRegion;
            return region.TwoLetterISORegionName.ToUpperInvariant() == "IR";
        }
        catch { return false; }
    }
}