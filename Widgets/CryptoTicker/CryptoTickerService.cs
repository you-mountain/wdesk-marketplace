using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WDesk.Core;

namespace WDesk.Widgets.CryptoTicker;

public static class CryptoTickerService
{
    // ═══════════════════════════════════════════
    //  HTTP Client
    // ═══════════════════════════════════════════
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly Dictionary<string, (CryptoTickerData data, DateTime at)> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    // ═══════════════════════════════════════════
    //  Region Detection
    // ═══════════════════════════════════════════
    private static bool? _isIranianUser = null;

    /// <summary>
    /// تشخیص خودکار کاربر ایرانی از روی Region ویندوز
    /// </summary>
    private static bool IsIranianUser()
    {
        if (_isIranianUser.HasValue) return _isIranianUser.Value;

        try
        {
            var region = System.Globalization.RegionInfo.CurrentRegion;
            string code = region.TwoLetterISORegionName.ToUpperInvariant();

            _isIranianUser = code == "IR";

            App.Logger?.Info($"Crypto: region = {code}, Iranian = {_isIranianUser.Value}");
        }
        catch
        {
            _isIranianUser = false;
        }

        return _isIranianUser.Value;
    }

    // ═══════════════════════════════════════════
    //  Get Price — با Fallback
    // ═══════════════════════════════════════════
    public static async Task<CryptoTickerData?> GetPriceAsync(
        string coinId,
        string vsCurrency,
        bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(coinId)) coinId = "bitcoin";
        if (string.IsNullOrWhiteSpace(vsCurrency)) vsCurrency = "usd";

        coinId = coinId.Trim().ToLowerInvariant();
        vsCurrency = vsCurrency.Trim().ToLowerInvariant();

        // ★ Normalize
        if (vsCurrency == "irt") vsCurrency = "toman";
        if (vsCurrency == "rls") vsCurrency = "toman";
        if (vsCurrency == "usd") vsCurrency = "usd";

        string cacheKey = $"{coinId}_{vsCurrency}";

        if (!forceRefresh && _cache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.Now - cached.at < CacheDuration)
                return cached.data;
        }

        CryptoTickerData? result = null;

        // ═══ اگه ارز تومان/ریال باشه → Nobitex ═══
        if (vsCurrency == "toman" || vsCurrency == "usdt")
        {
            result = await FetchFromNobitexAsync(coinId, vsCurrency);
            if (result != null) goto done;
        }

        // ═══ اول CoinGecko ═══
        result = await FetchFromCoinGeckoAsync(coinId, vsCurrency);
        if (result != null) goto done;

        // ═══ Fallback: Nobitex ═══
        App.Logger?.Info("Crypto: CoinGecko failed, trying Nobitex...");
        result = await FetchFromNobitexAsync(coinId, vsCurrency);
        if (result != null) goto done;

        // ═══ Fallback: CryptoCompare ═══
        App.Logger?.Info("Crypto: trying CryptoCompare...");
        result = await FetchFromCryptoCompareAsync(coinId, vsCurrency);
        if (result != null) goto done;

        App.Logger?.Error($"Crypto: all APIs failed for {coinId}/{vsCurrency}");
        return null;

    done:
        _cache[cacheKey] = (result!, DateTime.Now);
        return result;
    }

    // ═══════════════════════════════════════════
    //  1. CoinGecko (خارج از ایران)
    // ═══════════════════════════════════════════
    private static async Task<CryptoTickerData?> FetchFromCoinGeckoAsync(string coinId, string vsCurrency)
    {
        try
        {
            string vs = vsCurrency switch
            {
                "toman" => "usd",   // Nobitex برای تومان
                _ => vsCurrency
            };

            string url = $"https://api.coingecko.com/api/v3/simple/price" +
                         $"?ids={Uri.EscapeDataString(coinId)}" +
                         $"&vs_currencies={Uri.EscapeDataString(vs)}" +
                         $"&include_24hr_change=true";

            App.Logger?.Info($"Crypto: CoinGecko GET {url}");

            var response = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty(coinId, out var coinData))
                return null;

            var data = new CryptoTickerData
            {
                CoinId = coinId,
                VsCurrency = vsCurrency,
                CoinSymbol = GetSymbolForCoin(coinId),
                CoinName = GetNameForCoin(coinId),
                VsSymbol = GetSymbolForVsCurrency(vsCurrency),
                HasData = true,
                UpdatedAt = DateTime.Now,
                Source = "CoinGecko"
            };

            if (coinData.TryGetProperty(vs, out var priceEl))
                data.Price = priceEl.GetDouble();

            string changeKey = $"{vs}_24h_change";
            if (coinData.TryGetProperty(changeKey, out var changeEl))
                data.Change24h = changeEl.GetDouble();

            App.Logger?.Info($"Crypto: CoinGecko OK - {coinId}={data.Price} {vs}");
            return data;
        }
        catch (Exception ex)
        {
            App.Logger?.Warn($"Crypto: CoinGecko failed - {ex.Message}");
            return null;
        }
    }

    // ═══════════════════════════════════════════
    //  2. Nobitex (ایران)
    // ═══════════════════════════════════════════
    private static async Task<CryptoTickerData?> FetchFromNobitexAsync(string coinId, string vsCurrency)
    {
        try
        {
            string baseSymbol = GetNobitexSymbol(coinId);

            string quoteSymbol = vsCurrency switch
            {
                "toman" => "rls",
                "usdt" => "usdt",
                _ => "rls"
            };

            string url = $"https://apiv2.nobitex.ir/market/stats" +
                         $"?srcCurrency={baseSymbol}" +
                         $"&dstCurrency={quoteSymbol}";

            App.Logger?.Info($"Crypto: Nobitex GET {url}");

            var response = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("status", out var statusEl) ||
                statusEl.GetString() != "ok")
                return null;

            if (!root.TryGetProperty("stats", out var statsEl))
                return null;

            string marketKey = $"{baseSymbol}-{quoteSymbol}";

            if (!statsEl.TryGetProperty(marketKey, out var marketEl))
                return null;

            var data = new CryptoTickerData
            {
                CoinId = coinId,
                VsCurrency = vsCurrency,
                CoinSymbol = GetSymbolForCoin(coinId),
                CoinName = GetNameForCoin(coinId),
                VsSymbol = GetSymbolForVsCurrency(vsCurrency),
                HasData = true,
                UpdatedAt = DateTime.Now,
                Source = "Nobitex"
            };

            // ── Price ──
            if (marketEl.TryGetProperty("latest", out var latestEl))
            {
                var latestStr = latestEl.GetString() ?? "0";
                if (double.TryParse(latestStr,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var price))
                {
                    // Nobitex از ریال استفاده می‌کنه → تومان = تقسیم بر ۱۰
                    data.Price = (vsCurrency == "toman") ? price / 10.0 : price;
                }
            }

            // ── Change ──
            if (marketEl.TryGetProperty("dayChange", out var changeEl))
            {
                var changeStr = changeEl.GetString() ?? "0";
                if (double.TryParse(changeStr,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var change))
                {
                    data.Change24h = change;
                }
            }

            App.Logger?.Info($"Crypto: Nobitex OK - {marketKey}={data.Price} ({data.Change24h}%)");
            return data;
        }
        catch (Exception ex)
        {
            App.Logger?.Warn($"Crypto: Nobitex failed - {ex.Message}");
            return null;
        }
    }

    // ═══════════════════════════════════════════
    //  3. CryptoCompare (Fallback عمومی)
    // ═══════════════════════════════════════════
    private static async Task<CryptoTickerData?> FetchFromCryptoCompareAsync(string coinId, string vsCurrency)
    {
        try
        {
            string fsym = GetSymbolForCoin(coinId);
            string tsym = vsCurrency.ToUpperInvariant();

            // تومان رو پشتیبانی نمی‌کنه → USD
            if (tsym == "TOMAN") tsym = "USD";

            string url = $"https://min-api.cryptocompare.com/data/pricemultifull?fsyms={fsym}&tsyms={tsym}";

            var response = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("RAW", out var rawEl)) return null;
            if (!rawEl.TryGetProperty(fsym, out var coinEl)) return null;
            if (!coinEl.TryGetProperty(tsym, out var priceEl)) return null;

            var data = new CryptoTickerData
            {
                CoinId = coinId,
                VsCurrency = vsCurrency,
                CoinSymbol = fsym,
                CoinName = GetNameForCoin(coinId),
                VsSymbol = GetSymbolForVsCurrency(vsCurrency),
                HasData = true,
                UpdatedAt = DateTime.Now,
                Source = "CryptoCompare"
            };

            if (priceEl.TryGetProperty("PRICE", out var p))
                data.Price = p.GetDouble();

            if (priceEl.TryGetProperty("CHANGEPCT24HOUR", out var c))
                data.Change24h = c.GetDouble();

            return data;
        }
        catch (Exception ex)
        {
            App.Logger?.Warn($"Crypto: CryptoCompare failed - {ex.Message}");
            return null;
        }
    }

    // ═══════════════════════════════════════════
    //  Symbol Mappings
    // ═══════════════════════════════════════════
    private static string GetNobitexSymbol(string coinId) => coinId.ToLowerInvariant() switch
    {
        "bitcoin" => "btc",
        "ethereum" => "eth",
        "binancecoin" => "bnb",
        "ripple" => "xrp",
        "cardano" => "ada",
        "solana" => "sol",
        "dogecoin" => "doge",
        "polkadot" => "dot",
        "tron" => "trx",
        "litecoin" => "ltc",
        "matic-network" => "matic",
        "shiba-inu" => "shib",
        "avalanche-2" => "avax",
        "uniswap" => "uni",
        "chainlink" => "link",
        _ => coinId.ToLowerInvariant()
    };

    private static string GetSymbolForCoin(string coinId) => GetNobitexSymbol(coinId).ToUpperInvariant();

    private static string GetNameForCoin(string coinId) => coinId.ToLowerInvariant() switch
    {
        "bitcoin" => "Bitcoin",
        "ethereum" => "Ethereum",
        "binancecoin" => "BNB",
        "ripple" => "XRP",
        "cardano" => "Cardano",
        "solana" => "Solana",
        "dogecoin" => "Dogecoin",
        "polkadot" => "Polkadot",
        "tron" => "TRON",
        "litecoin" => "Litecoin",
        "matic-network" => "Polygon",
        "shiba-inu" => "Shiba Inu",
        "avalanche-2" => "Avalanche",
        "uniswap" => "Uniswap",
        "chainlink" => "Chainlink",
        _ => coinId
    };

    private static string GetSymbolForVsCurrency(string vs) => vs.ToLowerInvariant() switch
    {
        "toman" => "تومان ",
        "usd" => "$",
        "usdt" => "₮",
        "eur" => "€",
        "gbp" => "£",
        "jpy" => "¥",
        "cny" => "¥",
        "try" => "₺",
        "btc" => "₿",
        "eth" => "Ξ",
        "aed" => "د.إ",
        "inr" => "₹",
        "rub" => "₽",
        "krw" => "₩",
        "cad" => "C$",
        "aud" => "A$",
        "chf" => "Fr",
        _ => vs.ToUpperInvariant() + " "
    };

    public static void ClearCache() => _cache.Clear();
}