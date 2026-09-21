using System;

namespace WDesk.Widgets.Music;

public class MusicData
{
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string AlbumTitle { get; set; } = "";
    public byte[]? ThumbnailBytes { get; set; }
    public TimeSpan Position { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPlaying { get; set; }
    public bool HasSession { get; set; }
    public string SourceApp { get; set; } = "";

    public string DisplayTitle => string.IsNullOrEmpty(Title) ? "No track" : Title;
    public string DisplayArtist => string.IsNullOrEmpty(Artist) ? "—" : Artist;
}