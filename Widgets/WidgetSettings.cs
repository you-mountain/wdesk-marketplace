using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WDesk.Widgets;

/// <summary>
/// سرویس سبک برای تنظیمات ویجت‌ها از طریق ContextMenu.
/// </summary>
public static class WidgetSettings
{
    // ═══════════════════════════════════════════
    //  Input Dialog
    // ═══════════════════════════════════════════
    public static void ShowInput(
        string title,
        string label,
        string currentValue,
        Action<string> onSave)
    {
        var win = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current?.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow,
            ShowInTaskbar = false,
            Background = (Brush)Application.Current.FindResource("BgBase")
        };

        var stack = new StackPanel { Margin = new Thickness(20) };

        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = (Brush)Application.Current.FindResource("TextPrimary"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var textBox = new TextBox
        {
            Text = currentValue,
            Padding = new Thickness(10, 8, 10, 8),
            FontSize = 13,
            Background = (Brush)Application.Current.FindResource("BgElevated"),
            Foreground = (Brush)Application.Current.FindResource("TextPrimary"),
            BorderBrush = (Brush)Application.Current.FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 16)
        };

        // Select all on focus
        textBox.Loaded += (_, _) =>
        {
            textBox.Focus();
            textBox.SelectAll();
        };

        stack.Children.Add(textBox);

        string? result = null;

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
            Background = (Brush)Application.Current.FindResource("BgElevated"),
            Foreground = (Brush)Application.Current.FindResource("TextPrimary"),
            BorderBrush = (Brush)Application.Current.FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Focusable = false
        };
        cancelBtn.Click += (_, _) => win.Close();

        var okBtn = new Button
        {
            Content = "OK",
            Padding = new Thickness(20, 8, 20, 8),
            Background = (Brush)Application.Current.FindResource("AccentBrush"),
            Foreground = new SolidColorBrush(Color.FromRgb(0x0A, 0x0B, 0x08)),
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand,
            Focusable = false,
            IsDefault = true
        };
        okBtn.Click += (_, _) =>
        {
            result = textBox.Text;
            win.Close();
        };

        btnRow.Children.Add(cancelBtn);
        btnRow.Children.Add(okBtn);
        stack.Children.Add(btnRow);

        // Enter key = OK
        win.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                result = textBox.Text;
                win.Close();
            }
            else if (e.Key == Key.Escape)
            {
                win.Close();
            }
        };

        win.Content = stack;
        win.ShowDialog();

        if (!string.IsNullOrWhiteSpace(result))
            onSave(result.Trim());
    }

    // ═══════════════════════════════════════════
    //  Toggle Dialog
    // ═══════════════════════════════════════════
    public static void ShowToggle(
        string title,
        string label,
        bool currentValue,
        Action<bool> onSave)
    {
        var win = new Window
        {
            Title = title,
            Width = 380,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current?.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow,
            ShowInTaskbar = false,
            Background = (Brush)Application.Current.FindResource("BgBase")
        };

        var stack = new StackPanel { Margin = new Thickness(20) };

        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = (Brush)Application.Current.FindResource("TextPrimary"),
            Margin = new Thickness(0, 0, 0, 16)
        });

        bool result = currentValue;

        var toggle = new CheckBox
        {
            Content = currentValue ? "Enabled" : "Disabled",
            IsChecked = currentValue,
            FontSize = 13,
            Foreground = (Brush)Application.Current.FindResource("TextPrimary")
        };

        toggle.Checked += (_, _) => toggle.Content = "Enabled";
        toggle.Unchecked += (_, _) => toggle.Content = "Disabled";

        stack.Children.Add(toggle);

        var btnRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var cancelBtn = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(20, 8, 20, 8),
            Margin = new Thickness(0, 0, 8, 0),
            Background = (Brush)Application.Current.FindResource("BgElevated"),
            Foreground = (Brush)Application.Current.FindResource("TextPrimary"),
            BorderBrush = (Brush)Application.Current.FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Focusable = false
        };
        cancelBtn.Click += (_, _) => win.Close();

        var okBtn = new Button
        {
            Content = "OK",
            Padding = new Thickness(20, 8, 20, 8),
            Background = (Brush)Application.Current.FindResource("AccentBrush"),
            Foreground = new SolidColorBrush(Color.FromRgb(0x0A, 0x0B, 0x08)),
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand,
            Focusable = false
        };
        okBtn.Click += (_, _) =>
        {
            result = toggle.IsChecked == true;
            win.Close();
        };

        btnRow.Children.Add(cancelBtn);
        btnRow.Children.Add(okBtn);
        stack.Children.Add(btnRow);

        win.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
                win.Close();
        };

        win.Content = stack;
        win.ShowDialog();

        onSave(result);
    }
}