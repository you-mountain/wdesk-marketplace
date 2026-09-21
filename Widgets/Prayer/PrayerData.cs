using System;
using System.Collections.Generic;

namespace WDesk.Widgets.Prayer;

public class PrayerData
{
    public string City { get; set; } = "Tehran";
    public string Country { get; set; } = "Iran";

    public string Fajr { get; set; } = "—";
    public string Sunrise { get; set; } = "—";
    public string Dhuhr { get; set; } = "—";
    public string Asr { get; set; } = "—";
    public string Maghrib { get; set; } = "—";
    public string Isha { get; set; } = "—";

    public string NextPrayerName { get; set; } = "";
    public string NextPrayerTime { get; set; } = "";
    public TimeSpan TimeUntilNext { get; set; }

    public List<PrayerTime> Prayers { get; set; } = new();

    public string DisplayCountdown
    {
        get
        {
            if (TimeUntilNext <= TimeSpan.Zero) return "";
            if (TimeUntilNext.TotalHours >= 1)
                return $"{(int)TimeUntilNext.TotalHours}h {TimeUntilNext.Minutes}m";
            return $"{TimeUntilNext.Minutes}m";
        }
    }
}

public class PrayerTime
{
    public string Name { get; set; } = "";
    public string Time { get; set; } = "";
    public bool IsNext { get; set; } = false;
}