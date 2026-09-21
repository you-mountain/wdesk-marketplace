using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.NetworkMonitor.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var root = new Grid();

        // ═══ Background ═══
        var bg = new Border { CornerRadius = new CornerRadius(20) };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══ Layout ═══
        var stack = new StackPanel { Margin = new Thickness(16, 14, 16, 14) };

        // ═══════════════════════════════════════
        //  Header
        // ═══════════════════════════════════════
        var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // ── Title + Status ──
        var titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        var titleText = new TextBlock
        {
            Text = "NETWORK",
            FontSize = 10,
            FontWeight = FontWeights.SemiBold
        };
        titleText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        titleStack.Children.Add(titleText);

        var statusText = new TextBlock
        {
            Text = "Ready",
            FontSize = 9,
            Margin = new Thickness(0, 2, 0, 0)
        };
        statusText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        titleStack.Children.Add(statusText);

        Grid.SetColumn(titleStack, 0);
        headerGrid.Children.Add(titleStack);

        // ═══════════════════════════════════════
        //  ★ دکمه‌ی Ping
        // ═══════════════════════════════════════
        var pingBtn = new Button
        {
            Width = 28,
            Height = 28,
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Quick Ping (8.8.8.8)",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Focusable = false,
            Template = BuildCircleTemplate("WidgetBgElevated", "WidgetBg", 14)
        };
        Grid.SetColumn(pingBtn, 1);
        headerGrid.Children.Add(pingBtn);

        var pingIcon = new TextBlock
        {
            Text = "\uE701",
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        pingIcon.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        pingBtn.Content = pingIcon;

        // ★ کلیک Ping — ساده و مستقیم
        pingBtn.Click += async (_, e) =>
        {
            e.Handled = true;

            try
            {
                App.Logger?.Info("Ping button clicked");

                pingIcon.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
                pingBtn.IsEnabled = false;

                await NetworkMonitorService.RunQuickPingAsync();

                var d = NetworkMonitorService.GetCurrent();
                App.Logger?.Info(
                    $"Ping done: DisplayPing={d.DisplayPingMs:0} ms, " +
                    $"Source={d.PingSource}, At={d.LastPingAt:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Ping click failed", ex);
            }
            finally
            {
                pingIcon.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
                pingBtn.IsEnabled = true;
            }
        };

        // ═══════════════════════════════════════
        //  ★ دکمه‌ی Speedtest
        // ═══════════════════════════════════════
        var speedtestBtn = new Button
        {
            Width = 40,
            Height = 40,
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Focusable = false,
            Template = BuildCircleTemplate("AccentBrush", "AccentLightBrush", 20)
        };
        Grid.SetColumn(speedtestBtn, 2);
        headerGrid.Children.Add(speedtestBtn);

        var speedtestIcon = new TextBlock
        {
            Text = "\uE768",
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 16,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        speedtestBtn.Content = speedtestIcon;

        speedtestBtn.Click += async (_, e) =>
        {
            e.Handled = true;

            var d = NetworkMonitorService.GetCurrent();
            if (d.IsTesting) return;

            speedtestBtn.IsEnabled = false;
            try
            {
                await NetworkMonitorService.RunFullTestAsync();
            }
            finally
            {
                speedtestBtn.IsEnabled = true;
            }
        };

        stack.Children.Add(headerGrid);

        // ═══════════════════════════════════════
        //  Progress
        // ═══════════════════════════════════════
        var progressBar = new ProgressBar
        {
            Height = 4,
            Minimum = 0,
            Maximum = 1,
            Value = 0,
            Margin = new Thickness(0, 0, 0, 12),
            Visibility = Visibility.Collapsed
        };
        progressBar.SetResourceReference(ProgressBar.ForegroundProperty, "AccentBrush");
        progressBar.SetResourceReference(ProgressBar.BackgroundProperty, "WidgetBgElevated");
        progressBar.SetResourceReference(ProgressBar.BorderBrushProperty, "WidgetBgElevated");
        stack.Children.Add(progressBar);

        // ═══════════════════════════════════════
        //  Speed Row (Download / Upload)
        // ═══════════════════════════════════════
        var speedGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        speedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        speedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var dlStack = new StackPanel();
        var dlLabel = new TextBlock
        {
            Text = "⬇  DOWNLOAD",
            FontSize = 9,
            FontWeight = FontWeights.SemiBold
        };
        dlLabel.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        dlStack.Children.Add(dlLabel);

        var dlValue = new TextBlock
        {
            Text = "—",
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 2, 0, 0)
        };
        dlValue.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        dlStack.Children.Add(dlValue);

        Grid.SetColumn(dlStack, 0);
        speedGrid.Children.Add(dlStack);

        var ulStack = new StackPanel();
        var ulLabel = new TextBlock
        {
            Text = "⬆  UPLOAD",
            FontSize = 9,
            FontWeight = FontWeights.SemiBold
        };
        ulLabel.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        ulStack.Children.Add(ulLabel);

        var ulValue = new TextBlock
        {
            Text = "—",
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 2, 0, 0)
        };
        ulValue.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        ulStack.Children.Add(ulValue);

        Grid.SetColumn(ulStack, 1);
        speedGrid.Children.Add(ulStack);

        stack.Children.Add(speedGrid);

        // ═══════════════════════════════════════
        //  Ping Row
        // ═══════════════════════════════════════
        var pingRow = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        pingRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        pingRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        pingRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var pingRowIcon = new TextBlock
        {
            Text = "\uE701",
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        };
        pingRowIcon.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetColumn(pingRowIcon, 0);
        pingRow.Children.Add(pingRowIcon);

        var pingText = new TextBlock
        {
            Text = "Ping: —",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        };
        pingText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        Grid.SetColumn(pingText, 1);
        pingRow.Children.Add(pingText);

        var jitterText = new TextBlock
        {
            Text = "",
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        jitterText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetColumn(jitterText, 2);
        pingRow.Children.Add(jitterText);

        stack.Children.Add(pingRow);

        // ── Divider ──
        var divider = new Border { Height = 1, Margin = new Thickness(0, 4, 0, 8) };
        divider.SetResourceReference(Border.BackgroundProperty, "WidgetBgElevated");
        stack.Children.Add(divider);

        // ── ISP / Location ──
        var ispText = new TextBlock
        {
            Text = "ISP: —",
            FontSize = 10,
            Margin = new Thickness(0, 0, 0, 4)
        };
        ispText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(ispText);

        var locationText = new TextBlock
        {
            Text = "Location: —",
            FontSize = 10
        };
        locationText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(locationText);

        root.Children.Add(stack);

        // ═══════════════════════════════════════
        //  Timer — آپدیت UI
        // ═══════════════════════════════════════
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };

        timer.Tick += (_, _) =>
        {
            try
            {
                var d = NetworkMonitorService.GetCurrent();

                // ── Status ──
                if (statusText.Text != d.Status)
                    statusText.Text = d.Status;

                // ── Icon: Play / Stop ──
                var expectedIcon = d.IsTesting ? "\uE769" : "\uE768";
                if (speedtestIcon.Text != expectedIcon)
                    speedtestIcon.Text = expectedIcon;

                // ── Progress ──
                if (d.IsTesting)
                {
                    if (progressBar.Visibility != Visibility.Visible)
                        progressBar.Visibility = Visibility.Visible;
                    if (Math.Abs(progressBar.Value - d.Progress) > 0.01)
                        progressBar.Value = d.Progress;
                }
                else
                {
                    if (progressBar.Visibility != Visibility.Collapsed)
                        progressBar.Visibility = Visibility.Collapsed;
                }

                // ═══════════════════════════════════════
                //  ★ Ping — از فیلد واحد DisplayPingMs
                // ═══════════════════════════════════════
                var expectedPing = d.DisplayPingMs > 0
                    ? $"Ping: {d.DisplayPingMs:0} ms"
                    : (d.PingSource == "" ? "Ping: —" : "Ping: Timeout");

                if (pingText.Text != expectedPing)
                    pingText.Text = expectedPing;

                // ── Jitter — فقط وقتی منبع Speedtest باشه ──
                var expectedJitter = (d.PingSource == "speedtest" && d.DisplayJitterMs > 0)
                    ? $"±{d.DisplayJitterMs:0} ms"
                    : "";

                if (jitterText.Text != expectedJitter)
                    jitterText.Text = expectedJitter;

                // ── Speedtest Result ──
                if (d.Result != null)
                {
                    if (dlValue.Text != d.Result.DisplayDownload)
                        dlValue.Text = d.Result.DisplayDownload;

                    if (ulValue.Text != d.Result.DisplayUpload)
                        ulValue.Text = d.Result.DisplayUpload;

                    var expectedIsp = string.IsNullOrEmpty(d.Result.ISP)
                        ? "ISP: —"
                        : $"ISP: {d.Result.ISP}";
                    if (ispText.Text != expectedIsp)
                        ispText.Text = expectedIsp;

                    var expectedLoc = string.IsNullOrEmpty(d.Result.City)
                        ? "Location: —"
                        : $"Location: {d.Result.City}, {d.Result.Country}";
                    if (locationText.Text != expectedLoc)
                        locationText.Text = expectedLoc;
                }
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Timer tick failed", ex);
            }
        };
        timer.Start();

        root.Loaded += (_, _) => NetworkMonitorService.Initialize();
        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }

    // ═══════════════════════════════════════════
    //  Helper: Template دکمه‌ی دایره‌ای
    // ═══════════════════════════════════════════
    private static ControlTemplate BuildCircleTemplate(
        string normalKey, string hoverKey, double cornerRadius)
    {
        var template = new ControlTemplate(typeof(Button));

        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "Bd";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
        border.SetResourceReference(Border.BackgroundProperty, normalKey);

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

        border.AppendChild(presenter);
        template.VisualTree = border;

        // Hover
        var hover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
        var hoverSetter = new Setter
        {
            Property = Border.BackgroundProperty,
            TargetName = "Bd",
            Value = new DynamicResourceExtension(hoverKey)
        };
        hover.Setters.Add(hoverSetter);
        template.Triggers.Add(hover);

        // Pressed
        var pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
        pressed.Setters.Add(new Setter
        {
            Property = Border.OpacityProperty,
            TargetName = "Bd",
            Value = 0.75
        });
        template.Triggers.Add(pressed);

        // Disabled
        var disabled = new Trigger { Property = Button.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter
        {
            Property = Border.OpacityProperty,
            TargetName = "Bd",
            Value = 0.5
        });
        template.Triggers.Add(disabled);

        return template;
    }
}