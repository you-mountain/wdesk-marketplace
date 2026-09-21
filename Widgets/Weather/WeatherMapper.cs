namespace WDesk.Widgets.Weather;

public static class WeatherMapper
{
    /// <summary>
    /// WMO weather code → pixel icon type
    /// </summary>
    public static string GetIconType(int code) => code switch
    {
        0 => "sun",              // Clear sky
        1 => "sun",              // Mainly clear
        2 => "partly",           // Partly cloudy
        3 => "cloud",            // Overcast
        45 or 48 => "fog",       // Fog
        51 or 53 or 55 => "rain",        // Drizzle
        56 or 57 => "rain",      // Freezing drizzle
        61 or 63 => "rain",      // Rain
        65 => "heavy_rain",      // Heavy rain
        66 or 67 => "heavy_rain",// Freezing rain
        71 or 73 => "snow",      // Snow
        75 => "snow",            // Heavy snow
        77 => "snow",            // Snow grains
        80 or 81 => "rain",      // Rain showers
        82 => "heavy_rain",      // Violent rain showers
        85 or 86 => "snow",      // Snow showers
        95 => "thunder",         // Thunderstorm
        96 or 99 => "thunder",   // Thunderstorm with hail
        _ => "cloud"
    };
}