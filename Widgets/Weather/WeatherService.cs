using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WDesk.Core;

namespace WDesk.Widgets.Weather;

public static class WeatherService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly Dictionary<string, (WeatherData data, DateTime at)> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public static async Task<WeatherData?> GetAsync(string city, bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(city)) city = "Tehran";

        if (!forceRefresh && _cache.TryGetValue(city, out var cached))
        {
            if (DateTime.Now - cached.at < CacheDuration)
                return cached.data;
        }

        try
        {
            // ── Geocoding ──
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" +
                         $"?name={Uri.EscapeDataString(city)}" +
                         $"&count=1&language=en&format=json";

            var geoJson = await _http.GetStringAsync(geoUrl);
            var geoDoc = JsonDocument.Parse(geoJson);

            if (!geoDoc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                if (city != "Tehran")
                    return await GetAsync("Tehran", forceRefresh);

                return null;
            }

            var first = results[0];
            double lat = first.GetProperty("latitude").GetDouble();
            double lon = first.GetProperty("longitude").GetDouble();
            string displayName = first.TryGetProperty("name", out var n)
                ? n.GetString() ?? city
                : city;

            // ── Weather ──
            var wxUrl = $"https://api.open-meteo.com/v1/forecast" +
                        $"?latitude={lat}&longitude={lon}" +
                        $"&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m" +
                        $"&daily=temperature_2m_max,temperature_2m_min,weather_code" +
                        $"&hourly=temperature_2m" +
                        $"&timezone=auto&forecast_days=5";

            var wxJson = await _http.GetStringAsync(wxUrl);
            var wxDoc = JsonDocument.Parse(wxJson);
            var root = wxDoc.RootElement;

            var data = new WeatherData { City = displayName };

            // ── Current ──
            if (root.TryGetProperty("current", out var cur))
            {
                data.Temperature = cur.GetProperty("temperature_2m").GetDouble();
                data.FeelsLike = cur.GetProperty("apparent_temperature").GetDouble();
                data.Humidity = cur.GetProperty("relative_humidity_2m").GetInt32();
                data.WindSpeed = cur.GetProperty("wind_speed_10m").GetDouble();
                data.WeatherCode = cur.GetProperty("weather_code").GetInt32();
            }

            // ── Daily ──
            if (root.TryGetProperty("daily", out var daily))
            {
                var maxArr = daily.GetProperty("temperature_2m_max");
                var minArr = daily.GetProperty("temperature_2m_min");
                var codeArr = daily.GetProperty("weather_code");
                var timeArr = daily.GetProperty("time");

                if (maxArr.GetArrayLength() > 0)
                {
                    data.TempMax = maxArr[0].GetDouble();
                    data.TempMin = minArr[0].GetDouble();
                }

                int days = Math.Min(5, maxArr.GetArrayLength());
                for (int i = 0; i < days; i++)
                {
                    var day = new ForecastDay
                    {
                        TempMax = maxArr[i].GetDouble(),
                        TempMin = minArr[i].GetDouble(),
                        WeatherCode = codeArr[i].GetInt32()
                    };

                    try
                    {
                        var dateStr = timeArr[i].GetString();
                        if (!string.IsNullOrEmpty(dateStr))
                            day.Date = DateTime.Parse(dateStr);
                        else
                            day.Date = DateTime.Now.AddDays(i);
                    }
                    catch
                    {
                        day.Date = DateTime.Now.AddDays(i);
                    }

                    (day.Condition, _) = MapWeatherCode(day.WeatherCode);
                    data.Forecast.Add(day);
                }
            }

            // ── Hourly ──
            if (root.TryGetProperty("hourly", out var hourly))
            {
                var temps = hourly.GetProperty("temperature_2m");
                int count = Math.Min(24, temps.GetArrayLength());
                for (int i = 0; i < count; i++)
                    data.HourlyTemps.Add(temps[i].GetDouble());
            }

            // ── Condition ──
            (data.Condition, data.ConditionEmoji) = MapWeatherCode(data.WeatherCode);
            data.UpdatedAt = DateTime.Now;

            _cache[city] = (data, DateTime.Now);
            return data;
        }
        catch (Exception ex)
        {
            try { App.Logger?.Error($"Weather fetch failed for '{city}'", ex); } catch { }

            if (_cache.TryGetValue(city, out var old))
                return old.data;

            return null;
        }
    }

    // ── Weather Code → Condition + Emoji ──
    private static (string condition, string emoji) MapWeatherCode(int code) => code switch
    {
        0 => ("Clear Sky", "☀️"),
        1 => ("Mainly Clear", "🌤️"),
        2 => ("Partly Cloudy", "⛅"),
        3 => ("Overcast", "☁️"),
        45 => ("Fog", "🌫️"),
        48 => ("Rime Fog", "🌫️"),
        51 => ("Light Drizzle", "🌦️"),
        53 => ("Drizzle", "🌦️"),
        55 => ("Dense Drizzle", "🌧️"),
        61 => ("Slight Rain", "🌧️"),
        63 => ("Moderate Rain", "🌧️"),
        65 => ("Heavy Rain", "⛈️"),
        71 => ("Slight Snow", "🌨️"),
        73 => ("Moderate Snow", "🌨️"),
        75 => ("Heavy Snow", "❄️"),
        77 => ("Snow Grains", "🌨️"),
        80 => ("Showers", "🌦️"),
        81 => ("Heavy Showers", "🌧️"),
        82 => ("Violent Showers", "⛈️"),
        85 => ("Snow Showers", "🌨️"),
        86 => ("Heavy Snow Showers", "❄️"),
        95 => ("Thunderstorm", "⛈️"),
        96 => ("Thunderstorm + Hail", "⛈️"),
        99 => ("Severe Thunderstorm", "⛈️"),
        _ => ("Unknown", "❓")
    };

    public static void ClearCache() => _cache.Clear();
}