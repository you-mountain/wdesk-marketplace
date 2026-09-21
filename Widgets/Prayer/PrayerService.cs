using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WDesk.Core;

namespace WDesk.Widgets.Prayer;

public static class PrayerService
{
    private static PrayerData _data = new();
    private static bool _initialized = false;
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public static PrayerData GetCurrent() => _data;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _data = new PrayerData();
        App.Logger.Info("Prayer: initialized");

        _ = RefreshAsync();
    }

    public static async Task RefreshAsync()
    {
        try
        {
            App.Logger.Info("Prayer: fetching times...");

            var url = "https://api.aladhan.com/v1/timingsByCity" +
                      "?city=Tehran&country=Iran&method=7";

            var json = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var dataEl)) return;

            if (dataEl.TryGetProperty("timings", out var timings))
            {
                _data.Fajr = GetTime(timings, "Fajr");
                _data.Sunrise = GetTime(timings, "Sunrise");
                _data.Dhuhr = GetTime(timings, "Dhuhr");
                _data.Asr = GetTime(timings, "Asr");
                _data.Maghrib = GetTime(timings, "Maghrib");
                _data.Isha = GetTime(timings, "Isha");
            }

            RebuildPrayers();
            CalculateNext();

            App.Logger.Info("Prayer: times updated");
        }
        catch (Exception ex)
        {
            App.Logger.Error("Prayer: fetch failed", ex);
        }
    }

    private static string GetTime(JsonElement el, string key)
    {
        if (el.TryGetProperty(key, out var v))
        {
            var str = v.GetString() ?? "";
            var spaceIdx = str.IndexOf(' ');
            if (spaceIdx > 0) str = str.Substring(0, spaceIdx);
            return str;
        }
        return "—";
    }

    private static void RebuildPrayers()
    {
        _data.Prayers = new List<PrayerTime>
        {
            new() { Name = "Fajr",    Time = _data.Fajr },
            new() { Name = "Sunrise", Time = _data.Sunrise },
            new() { Name = "Dhuhr",   Time = _data.Dhuhr },
            new() { Name = "Asr",     Time = _data.Asr },
            new() { Name = "Maghrib", Time = _data.Maghrib },
            new() { Name = "Isha",    Time = _data.Isha },
        };
    }

    public static void CalculateNext()
    {
        try
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            PrayerTime? next = null;
            DateTime? nextTime = null;

            foreach (var p in _data.Prayers)
            {
                if (!TimeSpan.TryParse(p.Time, out var ts)) continue;
                var dt = today + ts;
                if (dt > now)
                {
                    next = p;
                    nextTime = dt;
                    break;
                }
            }

            if (next == null && _data.Prayers.Count > 0)
            {
                var first = _data.Prayers[0];
                if (TimeSpan.TryParse(first.Time, out var ts))
                {
                    next = first;
                    nextTime = today.AddDays(1) + ts;
                }
            }

            foreach (var p in _data.Prayers) p.IsNext = false;

            if (next != null && nextTime.HasValue)
            {
                next.IsNext = true;
                _data.NextPrayerName = next.Name;
                _data.NextPrayerTime = next.Time;
                _data.TimeUntilNext = nextTime.Value - now;
            }
        }
        catch (Exception ex)
        {
            App.Logger.Error("Prayer: CalculateNext failed", ex);
        }
    }
}