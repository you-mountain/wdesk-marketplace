using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WDesk.Core;

namespace WDesk.Widgets.AppLauncher.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public StyleSettingsSchema GetSettingsSchema() => new()
    {
        Fields = new List<StyleSettingField>
        {
            new() { Key = "showNames", Label = "Show App Names",   Type = StyleSettingType.Toggle, Group = "Layout", DefaultValue = true },
            new() { Key = "iconSize",  Label = "Icon Size",        Type = StyleSettingType.Number, Group = "Layout", DefaultValue = 48, Min = 32, Max = 80 },
            new() { Key = "columns",   Label = "Columns",          Type = StyleSettingType.Number, Group = "Layout", DefaultValue = 4, Min = 2, Max = 6 },
            new() { Key = "lineWidth", Label = "Petal Line Width", Type = StyleSettingType.Number, Group = "Layout", DefaultValue = 2, Min = 1, Max = 4 },
        }
    };

    public FrameworkElement Build(PlacedWidget instance)
    {
        var settings = instance?.Settings ?? new Dictionary<string, string>();

        bool showNames = GetBool(settings, "showNames", true);
        double iconSize = GetDouble(settings, "iconSize", 48);
        int columns = (int)GetDouble(settings, "columns", 4);
        double lineWidth = GetDouble(settings, "lineWidth", 2);

        string appsRaw = GetString(settings, "apps", "");
        var apps = AppLauncherCommon.ParseApps(appsRaw);

        var root = new Grid();

        // ═══════════════════════════════════════
        //  پس‌زمینه
        // ═══════════════════════════════════════
        var bgBorder = new Border
        {
            CornerRadius = new CornerRadius(24)
        };
        bgBorder.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bgBorder);

        // ═══════════════════════════════════════
        //  Content
        // ═══════════════════════════════════════
        if (apps.Count == 0)
        {
            // ── حالت خالی ──
            var emptyStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            double emptyScallopSize = 72;

            var scallopPath = new Path
            {
                Fill = Brushes.Transparent,
                StrokeThickness = 2.5,
                Width = emptyScallopSize,
                Height = emptyScallopSize,
                Stretch = Stretch.Uniform,
                Data = BuildScallopGeometry(100, 12, 0.92),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            scallopPath.SetResourceReference(Path.StrokeProperty, "AccentBrush");

            var plusIcon = new TextBlock
            {
                Text = "+",
                FontSize = emptyScallopSize * 0.42,
                FontWeight = FontWeights.Light,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            plusIcon.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");

            var emptyGrid = new Grid
            {
                Width = emptyScallopSize,
                Height = emptyScallopSize,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            emptyGrid.Children.Add(scallopPath);
            emptyGrid.Children.Add(plusIcon);

            emptyStack.Children.Add(emptyGrid);

            var noAppsText = new TextBlock
            {
                Text = "No apps configured",
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            noAppsText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
            emptyStack.Children.Add(noAppsText);

            var hintText = new TextBlock
            {
                Text = "Right-click → Add App",
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            };
            hintText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
            emptyStack.Children.Add(hintText);

            root.Children.Add(emptyStack);
        }
        else
        {
            // ── لیست اپ‌ها ──
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(12)
            };

            var wrap = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            double cellWidth = 260.0 / columns;

            // ═══ Refresh callback ═══
            Action onRefresh = () =>
            {
                try
                {
                    App.WindowManager?.RefreshWidget(instance.InstanceId);
                }
                catch (Exception ex)
                {
                    App.Logger?.Error("Refresh widget failed", ex);
                }
            };

            foreach (var app in apps)
            {
                wrap.Children.Add(MakeAppTile(
                    app,
                    iconSize,
                    cellWidth,
                    lineWidth,
                    showNames,
                    instance,
                    onRefresh));
            }

            scroll.Content = wrap;
            root.Children.Add(scroll);
        }

        return root;
    }

    // ═══════════════════════════════════════════
    //  App Tile
    // ═══════════════════════════════════════════
    private static FrameworkElement MakeAppTile(
        AppItem app,
        double iconSize,
        double cellWidth,
        double lineWidth,
        bool showNames,
        PlacedWidget placedWidget,
        Action onRefresh)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = cellWidth,
            Margin = new Thickness(2)
        };

        // ═══ ابعاد ═══
        double scallopSize = iconSize + 16;
        double glyphSize = iconSize;

        // ═══ گلبرگ — Stroke از AccentBrush ═══
        var scallopPath = new Path
        {
            Fill = Brushes.Transparent,
            StrokeThickness = lineWidth,
            Width = scallopSize,
            Height = scallopSize,
            Stretch = Stretch.Uniform,
            Data = BuildScallopGeometry(100, 12, 0.92),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        scallopPath.SetResourceReference(Path.StrokeProperty, "AccentBrush");

        // ═══ آیکون اپ ═══
        var iconSource = !string.IsNullOrEmpty(app.CustomIconPath)
            ? AppIconLoader.GetIcon(app.CustomIconPath, (int)glyphSize)
            : AppIconLoader.GetIcon(app.Path, (int)glyphSize);

        var iconGrid = new Grid
        {
            Width = iconSize,
            Height = iconSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (iconSource != null)
        {
            iconGrid.Children.Add(new Image
            {
                Source = iconSource,
                Width = glyphSize * 0.75,
                Height = glyphSize * 0.75,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
        else
        {
            // Fallback: حرف اول — با AccentBrush
            var fallbackText = new TextBlock
            {
                Text = string.IsNullOrEmpty(app.Name) ? "?" : app.Name.Substring(0, 1).ToUpperInvariant(),
                FontSize = glyphSize * 0.5,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            fallbackText.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
            iconGrid.Children.Add(fallbackText);
        }

        // ═══ ترکیب ═══
        var tileGrid = new Grid
        {
            Width = scallopSize,
            Height = scallopSize,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        tileGrid.Children.Add(scallopPath);
        tileGrid.Children.Add(iconGrid);

        stack.Children.Add(tileGrid);

        // ═══ Name ═══
        if (showNames)
        {
            var nameText = new TextBlock
            {
                Text = app.Name,
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 5, 0, 0),
                MaxWidth = cellWidth - 6
            };
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
            stack.Children.Add(nameText);
        }

        // ═══════════════════════════════════════
        //  Border کلیک‌پذیر
        // ═══════════════════════════════════════
        var clickable = new Border
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(2),
            Cursor = Cursors.Hand,
            Child = stack
        };

        // ── کلیک چپ: باز کردن اپ ──
        clickable.MouseLeftButtonDown += (_, e) =>
        {
            e.Handled = true;
            if (!AppLauncherCommon.Launch(app, out var error))
            {
                if (!string.IsNullOrEmpty(error))
                    MessageBox.Show(error, "App Launcher",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };

        // ── Hover: Stroke از AccentDarkBrush ──
        clickable.MouseEnter += (_, _) =>
        {
            scallopPath.SetResourceReference(Path.StrokeProperty, "AccentDarkBrush");
        };
        clickable.MouseLeave += (_, _) =>
        {
            scallopPath.SetResourceReference(Path.StrokeProperty, "AccentBrush");
        };

        // ═══════════════════════════════════════
        //  ContextMenu (راست‌کلیک)
        // ═══════════════════════════════════════
        var contextMenu = new ContextMenu
        {
            FlowDirection = FlowDirection.LeftToRight
        };

        // ── Open ──
        var miOpen = new MenuItem { Header = $"▶  Open {app.Name}" };
        miOpen.Click += (_, _) =>
        {
            AppLauncherCommon.Launch(app, out _);
        };
        contextMenu.Items.Add(miOpen);

        contextMenu.Items.Add(new Separator());

        // ── Open File Location ──
        var miOpenFolder = new MenuItem { Header = "📁  Open File Location" };
        miOpenFolder.Click += (_, _) =>
        {
            try
            {
                if (System.IO.File.Exists(app.Path))
                {
                    System.Diagnostics.Process.Start("explorer.exe",
                        $"/select,\"{app.Path}\"");
                }
            }
            catch (Exception ex)
            {
                App.Logger?.Error("Open folder failed", ex);
            }
        };
        contextMenu.Items.Add(miOpenFolder);

        contextMenu.Items.Add(new Separator());

        // ── Rename ──
        var miRename = new MenuItem { Header = "✎  Rename..." };
        miRename.Click += (_, _) =>
        {
            ShowRenameDialog(app, placedWidget, onRefresh);
        };
        contextMenu.Items.Add(miRename);

        // ── Remove ──
        var miRRemove = new MenuItem { Header = "✕  Remove" };
        miRRemove.Click += (_, _) =>
        {
            var result = MessageBox.Show(
                $"Remove \"{app.Name}\" from the widget?",
                "App Launcher",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppLauncherCommon.RemoveAppFromWidget(
                    placedWidget,
                    app.Path,
                    onSuccess: onRefresh);
            }
        };
        contextMenu.Items.Add(miRRemove);

        clickable.ContextMenu = contextMenu;

        // ── راست‌کلیک → باز کردن ContextMenu ──
        clickable.MouseRightButtonDown += (_, e) =>
        {
            e.Handled = true;
            clickable.ContextMenu.IsOpen = true;
        };

        return clickable;
    }

    // ═══════════════════════════════════════════
    //  Rename Dialog
    // ═══════════════════════════════════════════
    private static void ShowRenameDialog(AppItem app, PlacedWidget widget, Action onRefresh)
    {
        var win = new Window
        {
            Title = "Rename App",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current?.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow,
            ShowInTaskbar = false
        };
        win.SetResourceReference(Window.BackgroundProperty, "BgBase");

        var stack = new StackPanel { Margin = new Thickness(20) };

        var label = new TextBlock
        {
            Text = "App Name:",
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
        stack.Children.Add(label);

        var textBox = new TextBox
        {
            Text = app.Name,
            Padding = new Thickness(10, 8, 10, 8),
            FontSize = 13,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 16)
        };
        textBox.SetResourceReference(TextBox.BackgroundProperty, "BgElevated");
        textBox.SetResourceReference(TextBox.ForegroundProperty, "TextPrimary");
        textBox.SetResourceReference(TextBox.BorderBrushProperty, "BorderBrush");

        textBox.Loaded += (_, _) =>
        {
            textBox.Focus();
            textBox.SelectAll();
        };

        stack.Children.Add(textBox);

        var btnRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var cancelBtn = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(20, 8, 20, 8),
            Margin = new Thickness(0, 0, 8, 0),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Focusable = false
        };
        cancelBtn.SetResourceReference(Button.BackgroundProperty, "BgElevated");
        cancelBtn.SetResourceReference(Button.ForegroundProperty, "TextPrimary");
        cancelBtn.SetResourceReference(Button.BorderBrushProperty, "BorderBrush");
        cancelBtn.Click += (_, _) => win.Close();

        var okBtn = new Button
        {
            Content = "Save",
            Padding = new Thickness(20, 8, 20, 8),
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand,
            Focusable = false,
            Foreground = Brushes.White
        };
        okBtn.SetResourceReference(Button.BackgroundProperty, "AccentBrush");

        okBtn.Click += (_, _) =>
        {
            var newName = textBox.Text?.Trim();

            if (!string.IsNullOrEmpty(newName) && newName != app.Name)
            {
                try
                {
                    string currentRaw = widget.Settings.TryGetValue("apps", out var v) ? v : "";
                    var apps = AppLauncherCommon.ParseApps(currentRaw);

                    var target = apps.Find(a =>
                        string.Equals(a.Path, app.Path, StringComparison.OrdinalIgnoreCase));

                    if (target != null)
                    {
                        target.Name = newName;
                        widget.Settings["apps"] = AppLauncherCommon.SerializeApps(apps);
                        App.Settings.Save();

                        App.Notifications.Show("WDesk",
                            $"Renamed to {newName}", NotificationType.Success);

                        onRefresh?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    App.Logger?.Error("Rename failed", ex);
                }
            }

            win.Close();
        };

        btnRow.Children.Add(cancelBtn);
        btnRow.Children.Add(okBtn);
        stack.Children.Add(btnRow);

        win.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                okBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.Escape)
            {
                win.Close();
            }
        };

        win.Content = stack;
        win.ShowDialog();
    }

    // ═══════════════════════════════════════════
    //  Scallop Geometry (گلبرگ خط‌دار)
    // ═══════════════════════════════════════════
    private static Geometry BuildScallopGeometry(double size, int petals, double innerRatio)
    {
        double cx = size / 2;
        double cy = size / 2;
        double outerR = size / 2 * 0.95;
        double innerR = outerR * innerRatio;

        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            double startAngle = -Math.PI / 2;
            var startPoint = new Point(
                cx + outerR * Math.Cos(startAngle),
                cy + outerR * Math.Sin(startAngle));

            ctx.BeginFigure(startPoint, isFilled: true, isClosed: true);

            double angleStep = Math.PI * 2 / petals;
            double halfStep = angleStep / 2;

            for (int i = 0; i < petals; i++)
            {
                double tipAngle = startAngle + i * angleStep;
                double valleyAngle = tipAngle + halfStep;
                double nextTipAngle = tipAngle + angleStep;

                var valleyPoint = new Point(
                    cx + innerR * Math.Cos(valleyAngle),
                    cy + innerR * Math.Sin(valleyAngle));

                var nextTipPoint = new Point(
                    cx + outerR * Math.Cos(nextTipAngle),
                    cy + outerR * Math.Sin(nextTipAngle));

                double tipCtrlR = outerR * 1.02;
                var tipCtrl = new Point(
                    cx + tipCtrlR * Math.Cos(tipAngle + halfStep * 0.3),
                    cy + tipCtrlR * Math.Sin(tipAngle + halfStep * 0.3));

                double valleyCtrlR = innerR * 0.98;
                var valleyCtrl = new Point(
                    cx + valleyCtrlR * Math.Cos(valleyAngle - halfStep * 0.2),
                    cy + valleyCtrlR * Math.Sin(valleyAngle - halfStep * 0.2));

                ctx.BezierTo(tipCtrl, valleyCtrl, valleyPoint, true, true);

                var valleyCtrl2 = new Point(
                    cx + valleyCtrlR * Math.Cos(valleyAngle + halfStep * 0.2),
                    cy + valleyCtrlR * Math.Sin(valleyAngle + halfStep * 0.2));

                double nextTipCtrlR = outerR * 1.02;
                var nextTipCtrl = new Point(
                    cx + nextTipCtrlR * Math.Cos(nextTipAngle - halfStep * 0.3),
                    cy + nextTipCtrlR * Math.Sin(nextTipAngle - halfStep * 0.3));

                ctx.BezierTo(valleyCtrl2, nextTipCtrl, nextTipPoint, true, true);
            }
        }

        geometry.Freeze();
        return geometry;
    }

    // ═══════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════
    private static string GetString(Dictionary<string, string> s, string key, string fallback)
    {
        if (s != null && s.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v))
            return v;
        return fallback;
    }

    private static bool GetBool(Dictionary<string, string> s, string key, bool fallback)
    {
        if (s != null && s.TryGetValue(key, out var v))
            return v.Equals("true", StringComparison.OrdinalIgnoreCase);
        return fallback;
    }

    private static double GetDouble(Dictionary<string, string> s, string key, double fallback)
    {
        if (s != null && s.TryGetValue(key, out var v) &&
            double.TryParse(v, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
            return d;
        return fallback;
    }
}