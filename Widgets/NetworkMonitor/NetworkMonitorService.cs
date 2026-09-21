using System;
using System.Threading;
using System.Threading.Tasks;
using WDesk.Core;

namespace WDesk.Widgets.NetworkMonitor;

public static class NetworkMonitorService
{
    private static NetworkMonitorData _data = new();
    private static bool _initialized = false;

    private static CancellationTokenSource? _speedtestCts;
    private static CancellationTokenSource? _pingCts;
    private static readonly object _lock = new();

    public static NetworkMonitorData GetCurrent() => _data;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _data = new NetworkMonitorData
        {
            Status = "Ready",
            IsTesting = false,
            IsPinging = false,
            DisplayPingMs = -1
        };

        App.Logger?.Info("NetworkMonitor: initialized");
    }

    // ═══════════════════════════════════════════
    //  Full Speedtest
    // ═══════════════════════════════════════════
    public static async Task RunFullTestAsync()
    {
        if (_data.IsTesting) return;

        _data.IsTesting = true;
        _data.Status = "Starting...";
        _data.Progress = 0;

        var cts = new CancellationTokenSource();
        _speedtestCts = cts;

        try
        {
            var statusProgress = new Progress<string>(s => _data.Status = s);
            var progress = new Progress<double>(p => _data.Progress = p);

            var result = await SpeedtestService.RunFullTestAsync(
                statusProgress, progress, cts.Token);

            _data.Result = result;
            _data.Status = result.Success ? "Completed!" : "Failed";

            // ★ اگه Speedtest موفق بود، DisplayPing رو آپدیت کن (اگه QuickPing جدیدتر نیست)
            if (result.Success && result.PingMs > 0)
            {
                bool quickIsNewer =
                    _data.PingSource == "quick" &&
                    _data.LastPingAt > result.CompletedAt;

                if (!quickIsNewer)
                {
                    _data.DisplayPingMs = result.PingMs;
                    _data.DisplayJitterMs = result.JitterMs;
                    _data.PingSource = "speedtest";
                    _data.LastPingAt = result.CompletedAt;
                    App.Logger?.Info($"NetworkMonitor: DisplayPing <- speedtest ({result.PingMs:0} ms)");
                }
                else
                {
                    App.Logger?.Info("NetworkMonitor: keeping QuickPing (newer than speedtest)");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _data.Status = "Cancelled";
        }
        catch (Exception ex)
        {
            App.Logger?.Error("NetworkMonitor: full test failed", ex);
            _data.Status = "Error";
        }
        finally
        {
            _data.IsTesting = false;
            if (_speedtestCts == cts) _speedtestCts = null;
            try { cts.Dispose(); } catch { }
        }
    }

    // ═══════════════════════════════════════════
    //  Quick Ping
    // ═══════════════════════════════════════════
    public static async Task RunQuickPingAsync()
    {
        CancellationTokenSource cts;

        lock (_lock)
        {
            // ★ اگه قبلی در حال ping بود، cancel کن
            try { _pingCts?.Cancel(); } catch { }
            try { _pingCts?.Dispose(); } catch { }

            cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            _pingCts = cts;
            _data.IsPinging = true;
        }

        try
        {
            App.Logger?.Info("NetworkMonitor: quick ping starting...");

            var result = await SpeedtestService.PingAsync("8.8.8.8", 3, cts.Token);

            var pingMs = result.ping > 0 ? result.ping : -1;

            _data.DisplayPingMs = pingMs;
            _data.DisplayJitterMs = 0;  // QuickPing jitter نداره
            _data.PingSource = "quick";
            _data.LastPingAt = DateTime.Now;

            App.Logger?.Info($"NetworkMonitor: DisplayPing <- quick ({pingMs:0} ms)");
        }
        catch (OperationCanceledException)
        {
            App.Logger?.Info("NetworkMonitor: quick ping cancelled");
            _data.DisplayPingMs = -1;
            _data.PingSource = "quick";
            _data.LastPingAt = DateTime.Now;
        }
        catch (Exception ex)
        {
            App.Logger?.Error("NetworkMonitor: quick ping failed", ex);
            _data.DisplayPingMs = -1;
            _data.PingSource = "quick";
            _data.LastPingAt = DateTime.Now;
        }
        finally
        {
            _data.IsPinging = false;

            lock (_lock)
            {
                if (_pingCts == cts)
                {
                    try { _pingCts.Dispose(); } catch { }
                    _pingCts = null;
                }
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Cancel
    // ═══════════════════════════════════════════
    public static void CancelSpeedtest()
    {
        try { _speedtestCts?.Cancel(); } catch { }
    }

    public static void CancelQuickPing()
    {
        lock (_lock)
        {
            try { _pingCts?.Cancel(); } catch { }
        }
    }
}