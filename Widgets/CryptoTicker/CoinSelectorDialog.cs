using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace WDesk.Widgets.CryptoTicker;

public static class CoinSelectorDialog
{
    // ═══════════════════════════════════════════
    //  لیست ارزهای دیجیتال
    // ═══════════════════════════════════════════
    public static readonly (string id, string symbol, string name)[] Coins =
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

    // ═══════════════════════════════════════════
    //  لیست ارزهای مرجع
    // ═══════════════════════════════════════════
    public static (string code, string symbol, string name)[] GetCurrencies()
    {
        // ── ایران ──
        if (IsIranianRegion())
        {
            return new[]
            {
                ("toman", "تومان ", "Iranian Toman"),
                ("usdt", "₮", "Tether (USDT)"),
            };
        }

        // ── خارج ──
        return new[]
        {
            ("usd", "$", "US Dollar"),
            ("eur", "€", "Euro"),
            ("gbp", "£", "British Pound"),
            ("usdt", "₮", "Tether (USDT)"),
        };
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

    // ═══════════════════════════════════════════
    //  Show Dialog
    // ═══════════════════════════════════════════
    public static void ShowCoinDialog(string currentCoinId, Action<string> onPicked)
    {
        var items = Array.ConvertAll(Coins, c => (c.id, $"{c.name} ({c.symbol})"));
        ShowDialog("Select Cryptocurrency", items, currentCoinId, onPicked);
    }

    public static void ShowCurrencyDialog(string currentCurrency, Action<string> onPicked)
    {
        var currencies = GetCurrencies();
        var items = Array.ConvertAll(currencies, c => (c.code, $"{c.symbol}  {c.name}"));
        ShowDialog("Select Currency", items, currentCurrency, onPicked);
    }

    // ═══════════════════════════════════════════
    //  Core Dialog
    // ═══════════════════════════════════════════
    private static void ShowDialog(
        string title,
        (string id, string display)[] items,
        string currentId,
        Action<string> onPicked)
    {
        var win = new Window
        {
            Title = title,
            Width = 380,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current?.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            ShowInTaskbar = false
        };

        var rootCard = new Border
        {
            CornerRadius = new CornerRadius(16),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                BlurRadius = 40,
                ShadowDepth = 8,
                Opacity = 0.4,
                Color = Colors.Black,
                Direction = 270
            }
        };
        rootCard.SetResourceReference(Border.BackgroundProperty, "BgBase");
        rootCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // ═══ Header ═══
        var header = new Border
        {
            CornerRadius = new CornerRadius(16, 16, 0, 0),
            Padding = new Thickness(20, 14, 12, 14)
        };
        header.SetResourceReference(Border.BackgroundProperty, "BgSurface");

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var headerText = new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
        Grid.SetColumn(headerText, 0);
        headerGrid.Children.Add(headerText);

        var closeBtn = new Button
        {
            Content = "\uE8BB",
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 12,
            Width = 32,
            Height = 32,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Focusable = false
        };
        closeBtn.SetResourceReference(Button.ForegroundProperty, "TextSecondary");
        closeBtn.Click += (_, _) => win.Close();
        Grid.SetColumn(closeBtn, 1);
        headerGrid.Children.Add(closeBtn);

        header.Child = headerGrid;
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        // ═══ Content ═══
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Padding = new Thickness(12, 8, 12, 12)
        };

        var stack = new StackPanel();

        foreach (var (id, display) in items)
        {
            var isSelected = id == currentId;

            var row = MakeItemRow(display, isSelected);
            row.MouseLeftButtonDown += (_, e) =>
            {
                e.Handled = true;
                onPicked?.Invoke(id);
                win.Close();
            };

            stack.Children.Add(row);
        }

        scroll.Content = stack;
        Grid.SetRow(scroll, 1);
        grid.Children.Add(scroll);

        rootCard.Child = grid;
        win.Content = rootCard;

        header.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { win.DragMove(); } catch { }
            }
        };

        win.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) win.Close();
        };

        win.ShowDialog();
    }

    // ═══════════════════════════════════════════
    //  Item Row (iOS Style)
    // ═══════════════════════════════════════════
    private static Border MakeItemRow(string display, bool isSelected)
    {
        var row = new Border
        {
            Height = 46,
            Margin = new Thickness(0, 2, 0, 2),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14, 0, 14, 0),
            Cursor = Cursors.Hand,
            BorderThickness = new Thickness(isSelected ? 2 : 1)
        };

        if (isSelected)
        {
            row.SetResourceReference(Border.BackgroundProperty, "AccentFaded");
            row.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
        }
        else
        {
            row.SetResourceReference(Border.BackgroundProperty, "BgElevated");
            row.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
        }

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var label = new TextBlock
        {
            Text = display,
            FontSize = 13,
            FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);

        if (isSelected)
        {
            var check = new TextBlock
            {
                Text = "\uE73E",
                FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            check.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            Grid.SetColumn(check, 1);
            grid.Children.Add(check);
        }

        row.Child = grid;

        row.MouseEnter += (_, _) =>
        {
            if (!isSelected)
                row.SetResourceReference(Border.BackgroundProperty, "BgHover");
        };
        row.MouseLeave += (_, _) =>
        {
            if (!isSelected)
                row.SetResourceReference(Border.BackgroundProperty, "BgElevated");
        };

        return row;
    }
}