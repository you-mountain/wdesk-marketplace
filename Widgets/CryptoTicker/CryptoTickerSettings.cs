using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WDesk.Core;

namespace WDesk.Widgets.CryptoTicker;

public static class CryptoTickerSettings
{
    // ═══ لیست ارزهای دیجیتال معروف ═══
    private static readonly (string id, string symbol, string name)[] PopularCoins =
    {
        ("bitcoin", "BTC", "Bitcoin"),
        ("ethereum", "ETH", "Ethereum"),
        ("binancecoin", "BNB", "BNB"),
        ("ripple", "XRP", "XRP"),
        ("cardano", "ADA", "Cardano"),
        ("solana", "SOL", "Solana"),
        ("dogecoin", "DOGE", "Dogecoin"),
        ("polkadot", "DOT", "Polkadot"),
        ("tron", "TRX", "TRON"),
        ("litecoin", "LTC", "Litecoin"),
        ("matic-network", "MATIC", "Polygon"),
        ("shiba-inu", "SHIB", "Shiba Inu"),
        ("avalanche-2", "AVAX", "Avalanche"),
        ("uniswap", "UNI", "Uniswap"),
        ("chainlink", "LINK", "Chainlink"),
    };

    // ═══ لیست ارزهای مرجع ═══
    private static readonly (string code, string symbol, string name)[] VsCurrencies =
    {
        ("usd", "$", "US Dollar"),
        ("eur", "€", "Euro"),
        ("gbp", "£", "British Pound"),
        ("jpy", "¥", "Japanese Yen"),
        ("cny", "¥", "Chinese Yuan"),
        ("try", "₺", "Turkish Lira"),
        ("irr", "﷼", "Iranian Rial"),
        ("btc", "₿", "Bitcoin"),
        ("eth", "Ξ", "Ethereum"),
        ("aed", "د.إ", "UAE Dirham"),
        ("inr", "₹", "Indian Rupee"),
        ("rub", "₽", "Russian Ruble"),
        ("krw", "₩", "South Korean Won"),
        ("cad", "C$", "Canadian Dollar"),
        ("aud", "A$", "Australian Dollar"),
        ("chf", "Fr", "Swiss Franc"),
    };

    public static FrameworkElement Build(
        PlacedWidget instance,
        Action<Dictionary<string, string>> onSave)
    {
        try
        {
            var root = new StackPanel { Margin = new Thickness(20) };

            // ═══ Coin Selector ═══
            root.Children.Add(MakeSectionHeader("Cryptocurrency"));

            var coinCombo = new ComboBox
            {
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                BorderThickness = new Thickness(1)
            };
            coinCombo.SetResourceReference(ComboBox.BackgroundProperty, "BgElevated");
            coinCombo.SetResourceReference(ComboBox.ForegroundProperty, "TextPrimary");
            coinCombo.SetResourceReference(ComboBox.BorderBrushProperty, "BorderBrush");

            foreach (var coin in PopularCoins)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{coin.name} ({coin.symbol})",
                    Tag = coin.id
                };
                coinCombo.Items.Add(item);
            }

            // ── انتخاب پیش‌فرض ──
            string currentCoin = GetSetting(instance, "coin", "bitcoin");
            foreach (ComboBoxItem item in coinCombo.Items)
            {
                if ((item.Tag as string) == currentCoin)
                {
                    coinCombo.SelectedItem = item;
                    break;
                }
            }

            root.Children.Add(coinCombo);

            // ═══ Vs Currency Selector ═══
            root.Children.Add(MakeSectionHeader("Currency", 20));

            var vsCombo = new ComboBox
            {
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                BorderThickness = new Thickness(1)
            };
            vsCombo.SetResourceReference(ComboBox.BackgroundProperty, "BgElevated");
            vsCombo.SetResourceReference(ComboBox.ForegroundProperty, "TextPrimary");
            vsCombo.SetResourceReference(ComboBox.BorderBrushProperty, "BorderBrush");

            foreach (var vs in VsCurrencies)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{vs.name} ({vs.code.ToUpperInvariant()})",
                    Tag = vs.code
                };
                vsCombo.Items.Add(item);
            }

            string currentVs = GetSetting(instance, "vs", "usd");
            foreach (ComboBoxItem item in vsCombo.Items)
            {
                if ((item.Tag as string) == currentVs)
                {
                    vsCombo.SelectedItem = item;
                    break;
                }
            }

            root.Children.Add(vsCombo);

            // ═══ Live Preview ═══
            root.Children.Add(MakeSectionHeader("Preview", 20));

            var previewBorder = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14),
                BorderThickness = new Thickness(1)
            };
            previewBorder.SetResourceReference(Border.BackgroundProperty, "BgElevated");
            previewBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var previewText = new TextBlock
            {
                Text = "Loading...",
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap
            };
            previewText.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");
            previewBorder.Child = previewText;
            root.Children.Add(previewBorder);

            // ── Preview Update ──
            Action updatePreview = async () =>
            {
                try
                {
                    string coinId = (coinCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "bitcoin";
                    string vsCode = (vsCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "usd";

                    var data = await CryptoTickerService.GetPriceAsync(coinId, vsCode);
                    if (data == null)
                    {
                        previewText.Text = "Failed to load";
                        return;
                    }

                    previewText.Text = $"{data.CoinName} ({data.CoinSymbol}): {data.VsSymbol}{data.DisplayPrice}  {data.DisplayChange}";
                }
                catch { }
            };

            coinCombo.SelectionChanged += (_, _) => updatePreview();
            vsCombo.SelectionChanged += (_, _) => updatePreview();

            // ── اولین Preview ──
            updatePreview();

            // ═══ Apply Button ═══
            var applyBtn = new Button
            {
                Content = "Apply",
                Padding = new Thickness(24, 10, 24, 10),
                Margin = new Thickness(0, 24, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x0A, 0x0B, 0x08))
            };
            applyBtn.SetResourceReference(Button.BackgroundProperty, "AccentBrush");

            applyBtn.Click += (_, _) =>
            {
                string coinId = (coinCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "bitcoin";
                string vsCode = (vsCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "usd";

                var newSettings = new Dictionary<string, string>(instance.Settings)
                {
                    ["coin"] = coinId,
                    ["vs"] = vsCode
                };

                onSave(newSettings);
            };

            root.Children.Add(applyBtn);

            return new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = root
            };
        }
        catch (Exception ex)
        {
            return new TextBlock
            {
                Text = $"Settings error: {ex.Message}",
                Foreground = Brushes.Red,
                Margin = new Thickness(16),
                TextWrapping = TextWrapping.Wrap
            };
        }
    }

    // ═══════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════
    private static TextBlock MakeSectionHeader(string text, double topMargin = 0) => new()
    {
        Text = text,
        FontSize = 12,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, topMargin, 0, 8)
    };

    private static string GetSetting(PlacedWidget instance, string key, string fallback)
    {
        if (instance?.Settings?.TryGetValue(key, out var v) == true && !string.IsNullOrEmpty(v))
            return v;
        return fallback;
    }
}