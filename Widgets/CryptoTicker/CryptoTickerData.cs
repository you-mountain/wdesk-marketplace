using System;
using System.Collections.Generic;

namespace WDesk.Widgets.CryptoTicker;

public class CryptoTickerData
{
    // ═══ Coin Info ═══
    public string CoinId { get; set; } = "bitcoin";
    public string CoinSymbol { get; set; } = "BTC";
    public string CoinName { get; set; } = "Bitcoin";

    // ═══ Price ═══
    public double Price { get; set; }
    public string VsCurrency { get; set; } = "usd";
    public string VsSymbol { get; set; } = "$";

    // ═══ 24h Change ═══
    public double Change24h { get; set; }
    public bool IsUp => Change24h >= 0;

    // ═══ Meta ═══
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool HasData { get; set; } = false;
    public string Source { get; set; } = "";

    // ═══ Display ═══
    public string DisplayPrice => HasData ? FormatPrice(Price) : "—";
    public string DisplayChange => HasData
        ? $"{(IsUp ? "+" : "")}{Change24h:0.00}%"
        : "—";

    private static string FormatPrice(double price)
    {
        if (price >= 1_000_000) return price.ToString("N0");
        if (price >= 1000) return price.ToString("N0");
        if (price >= 1) return price.ToString("N2");
        if (price >= 0.01) return price.ToString("N4");
        return price.ToString("N8");
    }
}