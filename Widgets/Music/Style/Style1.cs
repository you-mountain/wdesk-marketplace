using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Music.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var settings = instance?.Settings ?? new Dictionary<string, string>();

        double coverSize = 72;
        double buttonSize = 48;

        // ═══════════════════════════════════════
        //  Root
        // ═══════════════════════════════════════
        var root = new Grid();

        // ═══ Background ═══
        var bg = new Border
        {
            CornerRadius = new CornerRadius(20)
        };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══════════════════════════════════════
        //  Layout
        // ═══════════════════════════════════════
        var grid = new Grid
        {
            Margin = new Thickness(14, 10, 14, 10)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // ═══ Cover ═══
        var coverEllipse = new Ellipse
        {
            Width = coverSize,
            Height = coverSize,
            Fill = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A))
        };

        var coverBorder = new Border
        {
            Width = coverSize,
            Height = coverSize,
            CornerRadius = new CornerRadius(coverSize / 2),
            ClipToBounds = true,
            Child = coverEllipse,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        };

        Grid.SetColumn(coverBorder, 0);
        grid.Children.Add(coverBorder);

        // ═══ Info ═══
        var infoStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        };

        var titleText = new TextBlock
        {
            Text = "No track",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 220
        };
        titleText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        infoStack.Children.Add(titleText);

        var artistText = new TextBlock
        {
            Text = "—",
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 3, 0, 0),
            MaxWidth = 220
        };
        artistText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        infoStack.Children.Add(artistText);

        Grid.SetColumn(infoStack, 1);
        grid.Children.Add(infoStack);

        // ═══ Play Button ═══
        var playBtn = new Border
        {
            Width = buttonSize,
            Height = buttonSize,
            CornerRadius = new CornerRadius(buttonSize / 2),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };
        playBtn.SetResourceReference(Border.BackgroundProperty, "AccentBrush");
        Grid.SetColumn(playBtn, 2);
        grid.Children.Add(playBtn);

        var playIcon = new TextBlock
        {
            Text = "\uE768",
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 18,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        playBtn.Child = playIcon;

        // Click
        playBtn.MouseLeftButtonDown += async (_, e) =>
        {
            e.Handled = true;
            await MusicService.PlayPauseAsync();
        };

        // Hover
        playBtn.MouseEnter += (_, _) =>
        {
            playBtn.SetResourceReference(Border.BackgroundProperty, "AccentLightBrush");
        };
        playBtn.MouseLeave += (_, _) =>
        {
            playBtn.SetResourceReference(Border.BackgroundProperty, "AccentBrush");
        };

        root.Children.Add(grid);

        // ═══════════════════════════════════════
        //  ★ State برای تشخیص تغییر
        // ═══════════════════════════════════════
        string lastTitle = "";
        string lastArtist = "";
        bool lastIsPlaying = false;
        byte[]? lastThumbnail = null;

        // ═══════════════════════════════════════
        //  Timer
        // ═══════════════════════════════════════
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

        timer.Tick += (_, _) =>
        {
            try
            {
                var data = MusicService.GetCurrent();

                // ── Title: فقط اگه عوض شده ──
                if (lastTitle != data.DisplayTitle)
                {
                    titleText.Text = data.DisplayTitle;
                    lastTitle = data.DisplayTitle;
                }

                // ── Artist: فقط اگه عوض شده ──
                if (lastArtist != data.DisplayArtist)
                {
                    artistText.Text = data.DisplayArtist;
                    lastArtist = data.DisplayArtist;
                }

                // ── Play Icon: فقط اگه عوض شده ──
                if (lastIsPlaying != data.IsPlaying)
                {
                    playIcon.Text = data.IsPlaying ? "\uE769" : "\uE768";
                    lastIsPlaying = data.IsPlaying;
                }

                // ── ★ Cover: هر بار thumbnail جدید اومد، آپدیت کن ──
                if (data.ThumbnailBytes != null && data.ThumbnailBytes.Length > 0)
                {
                    if (lastThumbnail == null || !ByteArraysEqual(lastThumbnail, data.ThumbnailBytes))
                    {
                        var img = MusicService.BytesToImage(data.ThumbnailBytes);
                        if (img != null)
                        {
                            coverEllipse.Fill = new ImageBrush(img)
                            {
                                Stretch = Stretch.UniformToFill
                            };
                            lastThumbnail = data.ThumbnailBytes;
                        }
                    }
                }
                else
                {
                    // اگه thumbnail پاک شد، به حالت اولیه برگردون
                    if (lastThumbnail != null)
                    {
                        coverEllipse.Fill = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
                        lastThumbnail = null;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Music: refresh failed", ex);
            }
        };
        timer.Start();

        // Initial
        root.Loaded += async (_, _) =>
        {
            try
            {
                await MusicService.InitializeAsync();
                await MusicService.ForceRefreshAsync();
            }
            catch { }
        };

        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }

    // ═══════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════
    private static bool ByteArraysEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;

        // فقط ۶۴ بایت اول و طول رو چک کن (سرعت)
        int checkLen = Math.Min(64, a.Length);
        for (int i = 0; i < checkLen; i++)
            if (a[i] != b[i]) return false;

        return true;
    }
}