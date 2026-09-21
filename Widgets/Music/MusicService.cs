using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using WDesk.Core;

namespace WDesk.Widgets.Music;

public static class MusicService
{
    private static GlobalSystemMediaTransportControlsSessionManager? _manager;
    private static GlobalSystemMediaTransportControlsSession? _session;
    private static bool _initialized = false;
    private static MusicData _lastData = new();

    // ═══════════════════════════════════════════
    //  Initialize
    // ═══════════════════════════════════════════
    public static async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            App.Logger?.Info("Music: requesting SMTC manager...");
            _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            if (_manager == null)
            {
                App.Logger?.Warn("Music: SMTC manager null");
                return;
            }

            App.Logger?.Info("Music: SMTC manager acquired");
            _manager.CurrentSessionChanged += (s, e) =>
            {
                App.Logger?.Info("Music: session changed");
                RefreshSession();
            };

            RefreshSession();
        }
        catch (Exception ex)
        {
            App.Logger?.Error("Music: init failed", ex);
            _initialized = false;
        }
    }

    // ═══════════════════════════════════════════
    //  Refresh Session
    // ═══════════════════════════════════════════
    private static void RefreshSession()
    {
        try
        {
            // Unsubscribe old
            if (_session != null)
            {
                try { _session.MediaPropertiesChanged -= OnMediaChanged; } catch { }
                try { _session.TimelinePropertiesChanged -= OnTimelineChanged; } catch { }
            }

            _session = _manager?.GetCurrentSession();

            if (_session == null)
            {
                App.Logger?.Info("Music: no session");
                _lastData = new MusicData
                {
                    HasSession = false,
                    Title = "No track",
                    Artist = "Open Spotify or Groove"
                };
                return;
            }

            App.Logger?.Info("Music: session acquired");

            try { _session.MediaPropertiesChanged += OnMediaChanged; } catch { }
            try { _session.TimelinePropertiesChanged += OnTimelineChanged; } catch { }

            _ = UpdateAsync();
        }
        catch (Exception ex)
        {
            App.Logger?.Error("Music: RefreshSession failed", ex);
        }
    }

    private static async void OnMediaChanged(
        GlobalSystemMediaTransportControlsSession sender,
        MediaPropertiesChangedEventArgs args)
    {
        await UpdateAsync();
    }

    private static async void OnTimelineChanged(
        GlobalSystemMediaTransportControlsSession sender,
        TimelinePropertiesChangedEventArgs args)
    {
        await UpdateAsync();
    }

    // ═══════════════════════════════════════════
    //  Get
    // ═══════════════════════════════════════════
    public static MusicData GetCurrent() => _lastData ?? new MusicData();

    public static async Task ForceRefreshAsync()
    {
        try { await UpdateAsync(); }
        catch { }
    }

    // ═══════════════════════════════════════════
    //  Update
    // ═══════════════════════════════════════════
    private static async Task UpdateAsync()
    {
        try
        {
            if (_session == null)
            {
                _lastData = new MusicData
                {
                    HasSession = false,
                    Title = "No track",
                    Artist = "Open Spotify or Groove"
                };
                return;
            }

            var data = new MusicData { HasSession = true };

            // ── Media properties ──
            try
            {
                var props = await _session.TryGetMediaPropertiesAsync();
                if (props != null)
                {
                    data.Title = props.Title ?? "";
                    data.Artist = props.Artist ?? "";
                    data.AlbumTitle = props.AlbumTitle ?? "";

                    if (props.Thumbnail != null)
                    {
                        try
                        {
                            using var stream = await props.Thumbnail.OpenReadAsync();
                            var bytes = new byte[stream.Size];
                            using var reader = new Windows.Storage.Streams.DataReader(stream);
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(bytes);
                            data.ThumbnailBytes = bytes;
                        }
                        catch (Exception ex)
                        {
                            App.Logger?.Warn($"Music: thumbnail failed: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger?.Warn($"Music: TryGetMediaProperties failed: {ex.Message}");
            }

            // ── Playback ──
            try
            {
                var playback = _session.GetPlaybackInfo();
                if (playback != null)
                {
                    data.IsPlaying = playback.PlaybackStatus ==
                        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                }
            }
            catch { }

            // ── Timeline ──
            try
            {
                var timeline = _session.GetTimelineProperties();
                if (timeline != null)
                {
                    data.Position = timeline.Position;
                    data.Duration = timeline.EndTime - timeline.StartTime;
                }
            }
            catch { }

            _lastData = data;

            App.Logger?.Info($"Music: Title='{data.Title}', Artist='{data.Artist}', Playing={data.IsPlaying}");
        }
        catch (Exception ex)
        {
            App.Logger?.Error("Music: UpdateAsync failed", ex);
        }
    }

    // ═══════════════════════════════════════════
    //  Controls
    // ═══════════════════════════════════════════
    public static async Task PlayPauseAsync()
    {
        try
        {
            if (_session == null) return;

            var playback = _session.GetPlaybackInfo();
            if (playback?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                await _session.TryPauseAsync();
            else
                await _session.TryPlayAsync();
        }
        catch (Exception ex)
        {
            App.Logger?.Error("Music: PlayPause failed", ex);
        }
    }

    public static async Task NextAsync()
    {
        try { if (_session != null) await _session.TrySkipNextAsync(); } catch { }
    }

    public static async Task PrevAsync()
    {
        try { if (_session != null) await _session.TrySkipPreviousAsync(); } catch { }
    }

    // ═══════════════════════════════════════════
    //  Thumbnail → ImageSource
    // ═══════════════════════════════════════════
    public static System.Windows.Media.ImageSource? BytesToImage(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;

        try
        {
            using var ms = new System.IO.MemoryStream(bytes);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }
}