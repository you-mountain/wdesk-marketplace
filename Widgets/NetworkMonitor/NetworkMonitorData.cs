using System;

namespace WDesk.Widgets.NetworkMonitor;

public class NetworkMonitorData
{
    // ═══ Speedtest Result ═══
    public SpeedtestResult? Result { get; set; }

    // ═══ State ═══
    public bool IsTesting { get; set; }
    public string Status { get; set; } = "Ready";
    public double Progress { get; set; } = 0;   // 0.0 to 1.0

    // ═══ Ping (منبع واحد برای UI) ═══
    /// <summary>آخرین مقدار Ping — از هر منبعی (Quick یا Speedtest)</summary>
    public double DisplayPingMs { get; set; } = -1;
    /// <summary>آخرین Jitter — فقط از Speedtest</summary>
    public double DisplayJitterMs { get; set; } = 0;
    /// <summary>منبع آخرین Ping: "quick" یا "speedtest" یا ""</summary>
    public string PingSource { get; set; } = "";
    /// <summary>زمان آخرین Ping</summary>
    public DateTime LastPingAt { get; set; } = DateTime.MinValue;

    // ═══ Quick Ping State ═══
    public bool IsPinging { get; set; }

    // ═══ Helpers ═══
    public static string FormatSpeed(double bytesPerSec)
    {
        if (bytesPerSec <= 0) return "—";
        if (bytesPerSec < 1024) return $"{bytesPerSec:0} B/s";
        if (bytesPerSec < 1024 * 1024) return $"{bytesPerSec / 1024:0.0} KB/s";
        if (bytesPerSec < 1024 * 1024 * 1024) return $"{bytesPerSec / 1024 / 1024:0.00} MB/s";
        return $"{bytesPerSec / 1024 / 1024 / 1024:0.00} GB/s";
    }

    public string DisplayPing =>
        DisplayPingMs > 0 ? $"{DisplayPingMs:0} ms" : "Timeout";

    public string DisplayJitter =>
        DisplayJitterMs > 0 ? $"±{DisplayJitterMs:0} ms" : "";
}

// ═══════════════════════════════════════════
//  Speedtest Result
// ═══════════════════════════════════════════
public class SpeedtestResult
{
    public double DownloadSpeed { get; set; }
    public double UploadSpeed { get; set; }
    public double PingMs { get; set; }
    public double JitterMs { get; set; }

    public string IP { get; set; } = "";
    public string ISP { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public string ServerName { get; set; } = "";

    public DateTime CompletedAt { get; set; }
    public bool Success { get; set; }

    public string DisplayDownload => DownloadSpeed > 0
        ? NetworkMonitorData.FormatSpeed(DownloadSpeed)
        : "—";

    public string DisplayUpload => UploadSpeed > 0
        ? NetworkMonitorData.FormatSpeed(UploadSpeed)
        : "—";
}