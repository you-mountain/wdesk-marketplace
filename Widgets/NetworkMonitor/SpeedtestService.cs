using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WDesk.Core;

namespace WDesk.Widgets.NetworkMonitor;

public static class SpeedtestService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    // ═══════════════════════════════════════════
    //  Ping Test
    // ═══════════════════════════════════════════
    public static async Task<(double ping, double jitter)> PingAsync(
        string host = "8.8.8.8",
        int count = 5,
        CancellationToken ct = default)
    {
        var results = new System.Collections.Generic.List<double>();

        for (int i = 0; i < count; i++)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, 3000).WaitAsync(ct);

                if (reply.Status == IPStatus.Success)
                    results.Add(reply.RoundtripTime);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                App.Logger?.Warn($"PingAsync iteration {i} failed: {ex.Message}");
            }

            try { await Task.Delay(100, ct); }
            catch (OperationCanceledException) { break; }
        }

        if (results.Count == 0)
            return (-1, 0);

        double sum = 0;
        foreach (var r in results) sum += r;
        double avg = sum / results.Count;

        double jitterSum = 0;
        foreach (var r in results)
            jitterSum += Math.Pow(r - avg, 2);
        double jitter = Math.Sqrt(jitterSum / results.Count);

        return (avg, jitter);
    }

    // ═══════════════════════════════════════════
    //  Download Test
    // ═══════════════════════════════════════════
    public static async Task<double> TestDownloadAsync(
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        try
        {
            string[] urls =
            {
                "https://speed.cloudflare.com/__down?bytes=10000000",
                "https://librespeed.org/backend/garbage.php?ckSize=100",
            };

            foreach (var url in urls)
            {
                try
                {
                    App.Logger?.Info($"Speedtest: downloading from {url}");

                    var sw = Stopwatch.StartNew();
                    long totalBytes = 0;

                    using var response = await _http.GetAsync(
                        url, HttpCompletionOption.ResponseHeadersRead, ct);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(ct);
                    var buffer = new byte[16384];
                    int read;

                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        totalBytes += read;
                        if (progress != null && sw.Elapsed.TotalSeconds > 0)
                            progress.Report(totalBytes / sw.Elapsed.TotalSeconds);
                    }

                    sw.Stop();
                    double speed = totalBytes / sw.Elapsed.TotalSeconds;
                    App.Logger?.Info($"Speedtest: download = {speed / 1024 / 1024:0.00} MB/s");
                    return speed;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    App.Logger?.Warn($"Speedtest: download from {url} failed - {ex.Message}");
                }
            }

            return 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            App.Logger?.Error("Speedtest: download failed", ex);
            return 0;
        }
    }

    // ═══════════════════════════════════════════
    //  Upload Test
    // ═══════════════════════════════════════════
    public static async Task<double> TestUploadAsync(
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        try
        {
            string[] urls =
            {
                "https://speed.cloudflare.com/__up",
                "https://librespeed.org/backend/empty.php",
            };

            foreach (var url in urls)
            {
                try
                {
                    App.Logger?.Info($"Speedtest: uploading to {url}");

                    var data = new byte[1_000_000];
                    new Random().NextBytes(data);

                    var sw = Stopwatch.StartNew();
                    using var content = new ByteArrayContent(data);
                    var response = await _http.PostAsync(url, content, ct);
                    response.EnsureSuccessStatusCode();
                    sw.Stop();

                    double speed = data.Length / sw.Elapsed.TotalSeconds;
                    App.Logger?.Info($"Speedtest: upload = {speed / 1024 / 1024:0.00} MB/s");
                    return speed;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    App.Logger?.Warn($"Speedtest: upload to {url} failed - {ex.Message}");
                }
            }

            return 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            App.Logger?.Error("Speedtest: upload failed", ex);
            return 0;
        }
    }

    // ═══════════════════════════════════════════
    //  Connection Info
    // ═══════════════════════════════════════════
    public static async Task<(string ip, string isp, string city, string country)> GetConnectionInfoAsync(
        CancellationToken ct = default)
    {
        string[] urls =
        {
            "https://ipapi.co/json/",
            "https://ipwho.is/",
            "https://api.ipify.org?format=json",
        };

        foreach (var url in urls)
        {
            try
            {
                var response = await _http.GetStringAsync(url, ct);
                var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                string ip = "", isp = "", city = "", country = "";

                if (url.Contains("ipapi.co"))
                {
                    ip = root.TryGetProperty("ip", out var e1) ? e1.GetString() ?? "" : "";
                    isp = root.TryGetProperty("org", out var e2) ? e2.GetString() ?? "" : "";
                    city = root.TryGetProperty("city", out var e3) ? e3.GetString() ?? "" : "";
                    country = root.TryGetProperty("country_name", out var e4) ? e4.GetString() ?? "" : "";
                }
                else if (url.Contains("ipwho.is"))
                {
                    ip = root.TryGetProperty("ip", out var e1) ? e1.GetString() ?? "" : "";
                    if (root.TryGetProperty("connection", out var conn))
                    {
                        isp = conn.TryGetProperty("isp", out var e2) ? e2.GetString() ?? "" : "";
                        if (string.IsNullOrEmpty(isp))
                            isp = conn.TryGetProperty("org", out var e3) ? e3.GetString() ?? "" : "";
                    }
                    city = root.TryGetProperty("city", out var e4) ? e4.GetString() ?? "" : "";
                    country = root.TryGetProperty("country", out var e5) ? e5.GetString() ?? "" : "";
                }
                else
                {
                    ip = root.TryGetProperty("ip", out var e1) ? e1.GetString() ?? "" : "";
                }

                if (!string.IsNullOrEmpty(ip))
                    return (ip, isp, city, country);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                App.Logger?.Warn($"ConnectionInfo {url} failed - {ex.Message}");
            }
        }

        return ("", "", "", "");
    }

    // ═══════════════════════════════════════════
    //  Full Speedtest
    // ═══════════════════════════════════════════
    public static async Task<SpeedtestResult> RunFullTestAsync(
        IProgress<string>? statusProgress = null,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var result = new SpeedtestResult();

        try
        {
            statusProgress?.Report("Testing ping...");
            progress?.Report(0.05);
            var pingResult = await PingAsync("8.8.8.8", 5, ct);
            result.PingMs = pingResult.ping;
            result.JitterMs = pingResult.jitter;

            statusProgress?.Report("Getting connection info...");
            progress?.Report(0.15);
            var info = await GetConnectionInfoAsync(ct);
            result.IP = info.ip;
            result.ISP = info.isp;
            result.City = info.city;
            result.Country = info.country;

            statusProgress?.Report("Testing download...");
            progress?.Report(0.25);
            result.DownloadSpeed = await TestDownloadAsync(progress, ct);

            statusProgress?.Report("Testing upload...");
            progress?.Report(0.75);
            result.UploadSpeed = await TestUploadAsync(progress, ct);

            statusProgress?.Report("Done!");
            progress?.Report(1.0);

            result.Success = true;
            result.CompletedAt = DateTime.Now;

            App.Logger?.Info(
                $"Speedtest complete: ↓{result.DownloadSpeed / 1024 / 1024:0.00} MB/s " +
                $"↑{result.UploadSpeed / 1024 / 1024:0.00} MB/s " +
                $"Ping {result.PingMs:0} ms");
        }
        catch (OperationCanceledException)
        {
            statusProgress?.Report("Cancelled");
            result.Success = false;
        }
        catch (Exception ex)
        {
            App.Logger?.Error("Speedtest: full test failed", ex);
            statusProgress?.Report("Error");
            result.Success = false;
        }

        return result;
    }
}

// ═══════════════════════════════════════════
//  Server Model
// ═══════════════════════════════════════════
public class SpeedtestServer
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public string Host { get; set; } = "";
    public string Url { get; set; } = "";
}