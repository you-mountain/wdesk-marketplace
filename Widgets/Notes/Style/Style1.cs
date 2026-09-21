using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Notes.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    // ── Placeholder Texts ──
    private const string TITLE_PLACEHOLDER = "Title...";
    private const string CONTENT_PLACEHOLDER = "Write something...";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var settings = instance?.Settings ?? new Dictionary<string, string>();

        // ═══ Load notes ═══
        var notes = LoadNotes(settings);

        var root = new Grid();

        // ═══ Background ═══
        var bg = new Border
        {
            CornerRadius = new CornerRadius(20)
        };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══ Layout ═══
        var mainGrid = new Grid
        {
            Margin = new Thickness(12)
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // ═══ Header ═══
        var headerRow = new Grid
        {
            Margin = new Thickness(0, 0, 0, 8)
        };
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var headerText = new TextBlock
        {
            Text = "NOTES",
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetColumn(headerText, 0);
        headerRow.Children.Add(headerText);

        var addBtn = new Button
        {
            Content = "＋",
            FontSize = 18,
            Width = 28,
            Height = 28,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Focusable = false
        };
        addBtn.SetResourceReference(Button.ForegroundProperty, "AccentBrush");
        Grid.SetColumn(addBtn, 1);
        headerRow.Children.Add(addBtn);

        Grid.SetRow(headerRow, 0);
        mainGrid.Children.Add(headerRow);

        // ═══ Notes List ═══
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var notesStack = new StackPanel();

        // ── Rebuild ──
        Action rebuild = null!;
        rebuild = () =>
        {
            notesStack.Children.Clear();

            foreach (var note in notes.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.UpdatedAt))
            {
                notesStack.Children.Add(MakeNoteCard(note, notes, instance, () => rebuild()));
            }
        };

        // ── Add Button ──
        addBtn.Click += (_, _) =>
        {
            var newNote = new NoteData
            {
                Title = "",
                Content = "",
                Color = GetRandomColor()
            };
            notes.Add(newNote);
            SaveNotes(instance, notes);
            rebuild();

            // ★ Focus روی نوت جدید (title)
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var lastCard = notesStack.Children.OfType<Border>().LastOrDefault();
                if (lastCard?.Child is Grid g)
                {
                    var textBoxes = FindChildren<TextBox>(g).ToList();
                    textBoxes.FirstOrDefault()?.Focus();
                }
            }), DispatcherPriority.Background);
        };

        rebuild();
        scroll.Content = notesStack;

        Grid.SetRow(scroll, 1);
        mainGrid.Children.Add(scroll);

        root.Children.Add(mainGrid);

        return root;
    }

    // ═══════════════════════════════════════════
    //  Note Card — با Placeholder
    // ═══════════════════════════════════════════
    private static FrameworkElement MakeNoteCard(
        NoteData note,
        List<NoteData> allNotes,
        PlacedWidget instance,
        Action onRefresh)
    {
        var card = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 8),
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(note.Color)),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF))
        };

        var stack = new StackPanel();

        // ═══════════════════════════════════════
        //  Title Field با Placeholder
        // ═══════════════════════════════════════
        var titleBox = new TextBox
        {
            Text = note.Title,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            Tag = "title",
            CaretBrush = Brushes.White
        };

        titleBox.FlowDirection = TextDirectionHelper.GetFlowDirection(note.Title);

        // ── Placeholder برای Title ──
        var titlePlaceholder = new TextBlock
        {
            Text = TITLE_PLACEHOLDER,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
            IsHitTestVisible = false,
            Padding = new Thickness(0),
            Margin = new Thickness(0)
        };

        // ── Grid: TextBox + Placeholder روی هم ──
        var titleGrid = new Grid();
        titleGrid.Children.Add(titleBox);
        titleGrid.Children.Add(titlePlaceholder);

        // ── نمایش/مخفی کردن Placeholder ──
        Action updateTitlePlaceholder = () =>
        {
            titlePlaceholder.Visibility = string.IsNullOrEmpty(titleBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // جهت placeholder بر اساس محتوا
            titlePlaceholder.FlowDirection = TextDirectionHelper.GetFlowDirection(titleBox.Text);
        };

        titleBox.TextChanged += (_, _) =>
        {
            note.Title = titleBox.Text;
            note.UpdatedAt = DateTime.Now;
            titleBox.FlowDirection = TextDirectionHelper.GetFlowDirection(titleBox.Text);
            updateTitlePlaceholder();
            SaveNotes(instance, allNotes);
        };

        // ★ اولین بار
        updateTitlePlaceholder();

        stack.Children.Add(titleGrid);

        // ═══════════════════════════════════════
        //  Content Field با Placeholder
        // ═══════════════════════════════════════
        var contentBox = new TextBox
        {
            Text = note.Content,
            FontSize = 12,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 4, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            MinHeight = 40,
            MaxHeight = 200,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Tag = "content",
            CaretBrush = Brushes.White
        };

        contentBox.FlowDirection = TextDirectionHelper.GetFlowDirection(note.Content);

        // ── Placeholder برای Content ──
        var contentPlaceholder = new TextBlock
        {
            Text = CONTENT_PLACEHOLDER,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
            IsHitTestVisible = false,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 4, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };

        // ── Grid: TextBox + Placeholder ──
        var contentGrid = new Grid();
        contentGrid.Children.Add(contentBox);
        contentGrid.Children.Add(contentPlaceholder);

        // ── نمایش/مخفی کردن Placeholder ──
        Action updateContentPlaceholder = () =>
        {
            contentPlaceholder.Visibility = string.IsNullOrEmpty(contentBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            contentPlaceholder.FlowDirection = TextDirectionHelper.GetFlowDirection(contentBox.Text);
        };

        contentBox.TextChanged += (_, _) =>
        {
            note.Content = contentBox.Text;
            note.UpdatedAt = DateTime.Now;
            contentBox.FlowDirection = TextDirectionHelper.GetFlowDirection(contentBox.Text);
            updateContentPlaceholder();
            SaveNotes(instance, allNotes);
        };

        // ★ اولین بار
        updateContentPlaceholder();

        stack.Children.Add(contentGrid);

        // ═══════════════════════════════════════
        //  Footer: Date + Delete
        // ═══════════════════════════════════════
        var footer = new Grid
        {
            Margin = new Thickness(0, 6, 0, 0)
        };
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var dateText = new TextBlock
        {
            Text = note.UpdatedAt.ToString("HH:mm • MMM d"),
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(dateText, 0);
        footer.Children.Add(dateText);

        var deleteBtn = new Button
        {
            Content = "✕",
            FontSize = 11,
            Width = 22,
            Height = 22,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF)),
            Cursor = Cursors.Hand,
            Focusable = false,
            ToolTip = "Delete"
        };
        deleteBtn.Click += (_, _) =>
        {
            var result = MessageBox.Show(
                "Delete this note?",
                "Notes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                allNotes.Remove(note);
                SaveNotes(instance, allNotes);
                onRefresh();
            }
        };
        Grid.SetColumn(deleteBtn, 1);
        footer.Children.Add(deleteBtn);

        stack.Children.Add(footer);
        card.Child = stack;

        return card;
    }

    // ═══════════════════════════════════════════
    //  Save / Load
    // ═══════════════════════════════════════════
    private static List<NoteData> LoadNotes(Dictionary<string, string> settings)
    {
        if (settings.TryGetValue("notes", out var json))
            return NoteData.DeserializeList(json);
        return new List<NoteData>();
    }

    private static void SaveNotes(PlacedWidget instance, List<NoteData> notes)
    {
        try
        {
            instance.Settings["notes"] = NoteData.SerializeList(notes);
            App.Settings.Save();
        }
        catch { }
    }

    // ═══════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════
    private static string GetRandomColor()
    {
        var colors = new[]
        {
            "#FF8FB339",  // سبز برند
            "#FFE74C3C",  // قرمز
            "#FF4FC3F7",  // آبی
            "#FF9B59B6",  // بنفش
            "#FFFF9800",  // نارنجی
            "#FF00BCD4",  // فیروزه‌ای
            "#FFE91E63",  // صورتی
        };
        return colors[new Random().Next(colors.Length)];
    }

    private static IEnumerable<T> FindChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T t)
                yield return t;

            foreach (var descendant in FindChildren<T>(child))
                yield return descendant;
        }
    }
}