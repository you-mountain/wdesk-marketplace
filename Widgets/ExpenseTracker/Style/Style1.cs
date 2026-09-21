using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WDesk.Core;
using WDesk.Helpers;

namespace WDesk.Widgets.ExpenseTracker.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    private static readonly FontFamily IconFont =
        new("Segoe Fluent Icons, Segoe MDL2 Assets");

    // ═══ State ═══
    private string _currentPeriod = "month";

    // ═══ UI Refs ═══
    private StackPanel? _listStack;
    private Canvas? _chartCanvas;
    private TextBlock? _balValue;
    private TextBlock? _balLabel;
    private TextBlock? _titleText;
    private TextBlock? _subText;
    private TextBlock? _incomeText;
    private TextBlock? _expText;
    private Button? _currencyBtn;
    private Button? _addBtn;
    private readonly Dictionary<string, Button> _tabButtons = new();
    private bool _isRtl;

    // ═══════════════════════════════════════════
    //  Radius Constants (متمرکز)
    // ═══════════════════════════════════════════
    private const double RadiusWidget = 20;   // پس‌زمینه اصلی ویجت
    private const double RadiusCard = 16;     // کارت‌ها (balance, chart)
    private const double RadiusButton = 14;   // دکمه‌ها
    private const double RadiusChip = 18;     // chip ها
    private const double RadiusRow = 10;      // ردیف آیتم‌ها

    private static Brush Res(string key) =>
        (Brush)Application.Current.FindResource(key);

    private static string L(string key) => LocalizationHelper.T(key);

    public FrameworkElement Build(PlacedWidget instance)
    {
        ExpenseService.Initialize();

        _isRtl = LocalizationHelper.Instance.IsRtl;

        var root = new Grid();

        // ═══ Background ═══
        var bg = new Border { CornerRadius = new CornerRadius(RadiusWidget) };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══ Layout ═══
        var mainGrid = new Grid { Margin = new Thickness(16, 14, 16, 14) };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // ═══════════════════════════════════════
        //  1. Header
        // ═══════════════════════════════════════
        var header = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        _titleText = new TextBlock
        {
            Text = L("expense.title"),
            FontSize = 10,
            FontWeight = FontWeights.SemiBold
        };
        _titleText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        titleStack.Children.Add(_titleText);

        _subText = new TextBlock
        {
            Text = L("expense.subtitle"),
            FontSize = 9,
            Margin = new Thickness(0, 2, 0, 0)
        };
        _subText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        titleStack.Children.Add(_subText);

        Grid.SetColumn(titleStack, 0);
        header.Children.Add(titleStack);

        _currencyBtn = new Button
        {
            Content = ExpenseService.GetSettings().DefaultCurrency,
            Padding = new Thickness(14, 6, 14, 6),
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Background = Res("WidgetBgElevated"),
            Foreground = Res("WidgetTextSecondary"),
            VerticalAlignment = VerticalAlignment.Center,
            Template = BuildButtonTemplate(RadiusChip)   // ★ گرد
        };
        _currencyBtn.Click += OnCurrencyClick;
        Grid.SetColumn(_currencyBtn, 1);
        header.Children.Add(_currencyBtn);

        Grid.SetRow(header, 0);
        mainGrid.Children.Add(header);

        // ═══════════════════════════════════════
        //  2. Balance Card — ★ گرد
        // ═══════════════════════════════════════
        var balanceCard = new Border
        {
            CornerRadius = new CornerRadius(RadiusCard),
            Padding = new Thickness(14, 12, 14, 12),
            Margin = new Thickness(0, 0, 0, 12)
        };
        balanceCard.SetResourceReference(Border.BackgroundProperty, "WidgetBgElevated");

        var balStack = new StackPanel();

        _balLabel = new TextBlock
        {
            Text = L("expense.balance"),
            FontSize = 10
        };
        _balLabel.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        balStack.Children.Add(_balLabel);

        _balValue = new TextBlock
        {
            Text = "—",
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 2, 0, 6)
        };
        _balValue.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        balStack.Children.Add(_balValue);

        var ioGrid = new Grid();
        ioGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ioGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var incomeStack = new StackPanel { Orientation = Orientation.Horizontal };
        incomeStack.Children.Add(new TextBlock
        {
            Text = "\uE74A",
            FontFamily = IconFont,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(0x6F, 0xBF, 0x4A)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });
        _incomeText = new TextBlock { Text = "—", FontSize = 10 };
        _incomeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        incomeStack.Children.Add(_incomeText);
        Grid.SetColumn(incomeStack, 0);
        ioGrid.Children.Add(incomeStack);

        var expStack = new StackPanel { Orientation = Orientation.Horizontal };
        expStack.Children.Add(new TextBlock
        {
            Text = "\uE74B",
            FontFamily = IconFont,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0x52, 0x52)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });
        _expText = new TextBlock { Text = "—", FontSize = 10 };
        _expText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        expStack.Children.Add(_expText);
        Grid.SetColumn(expStack, 1);
        ioGrid.Children.Add(expStack);

        balStack.Children.Add(ioGrid);
        balanceCard.Child = balStack;

        Grid.SetRow(balanceCard, 1);
        mainGrid.Children.Add(balanceCard);

        // ═══════════════════════════════════════
        //  3. Chart — ★ گرد
        // ═══════════════════════════════════════
        var chartBorder = new Border
        {
            CornerRadius = new CornerRadius(RadiusCard),
            Padding = new Thickness(10, 8, 10, 8),
            Margin = new Thickness(0, 0, 0, 12)
        };
        chartBorder.SetResourceReference(Border.BackgroundProperty, "WidgetBgElevated");

        var chartGrid = new Grid();
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var chartLabel = new TextBlock
        {
            Text = "7d",
            FontSize = 9,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 8, 2)
        };
        chartLabel.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetColumn(chartLabel, 0);
        chartGrid.Children.Add(chartLabel);

        _chartCanvas = new Canvas { Height = 40 };
        Grid.SetColumn(_chartCanvas, 1);
        chartGrid.Children.Add(_chartCanvas);

        chartBorder.Child = chartGrid;
        chartBorder.SizeChanged += (_, _) => DrawChart();
        chartBorder.Loaded += (_, _) => DrawChart();

        Grid.SetRow(chartBorder, 2);
        mainGrid.Children.Add(chartBorder);

        // ═══════════════════════════════════════
        //  4. Tabs — ★ گرد
        // ═══════════════════════════════════════
        var tabsGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        for (int i = 0; i < 4; i++)
            tabsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var periods = new (string Key, string LabelKey)[]
        {
            ("today", "common.today"),
            ("week",  "common.week"),
            ("month", "common.month"),
            ("all",   "common.all"),
        };

        for (int i = 0; i < periods.Length; i++)
        {
            var (key, labelKey) = periods[i];
            var btn = new Button
            {
                Content = L(labelKey),
                Padding = new Thickness(4, 8, 4, 8),
                FontSize = 11,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent,
                Foreground = Res("WidgetTextMuted"),
                Margin = new Thickness(
                    i == 0 ? 0 : 3,
                    0,
                    i == periods.Length - 1 ? 0 : 3,
                    0),
                Template = BuildButtonTemplate(RadiusButton)   // ★ گرد
            };

            var capturedKey = key;
            btn.Click += (_, _) => OnTabClick(capturedKey);

            _tabButtons[key] = btn;
            Grid.SetColumn(btn, i);
            tabsGrid.Children.Add(btn);
        }

        Grid.SetRow(tabsGrid, 3);
        mainGrid.Children.Add(tabsGrid);

        // ═══════════════════════════════════════
        //  5. List
        // ═══════════════════════════════════════
        _listStack = new StackPanel();

        var listScroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _listStack
        };

        Grid.SetRow(listScroll, 4);
        mainGrid.Children.Add(listScroll);

        // ═══════════════════════════════════════
        //  6. Add Button — ★ گرد
        // ═══════════════════════════════════════
        _addBtn = new Button
        {
            Content = "＋  " + L("expense.add"),
            Padding = new Thickness(12, 12, 12, 12),
            Margin = new Thickness(0, 10, 0, 0),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Background = Res("AccentBrush"),
            Foreground = Brushes.White,
            Template = BuildButtonTemplate(RadiusButton)   // ★ گرد
        };
        _addBtn.Click += OnAddClick;
        Grid.SetRow(_addBtn, 5);
        mainGrid.Children.Add(_addBtn);

        root.Children.Add(mainGrid);

        RefreshAll();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += (_, _) =>
        {
            RefreshAll();
            if (_currencyBtn != null)
                _currencyBtn.Content = ExpenseService.GetSettings().DefaultCurrency;
        };
        timer.Start();

        root.Loaded += (_, _) => timer.Start();
        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }

    // ═══════════════════════════════════════════
    //  Event Handlers
    // ═══════════════════════════════════════════
    private void OnCurrencyClick(object sender, RoutedEventArgs e)
    {
        var s = ExpenseService.GetSettings();
        s.DefaultCurrency = s.DefaultCurrency switch
        {
            "IRT" => "USD",
            "USD" => "EUR",
            _ => "IRT"
        };
        ExpenseService.SaveSettings();

        if (_currencyBtn != null)
            _currencyBtn.Content = s.DefaultCurrency;

        RefreshAll();
    }

    private void OnTabClick(string key)
    {
        _currentPeriod = key;
        UpdateTabs();
        RefreshList();
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        ExpenseEntryDialog.Show(null, _ =>
        {
            RefreshAll();
            App.Notifications.Show("WDesk", L("expense.added"), NotificationType.Success);
        });
    }

    // ═══════════════════════════════════════════
    //  Refresh
    // ═══════════════════════════════════════════
    private void RefreshAll()
    {
        RefreshBalance();
        RefreshList();
        DrawChart();
        UpdateTabs();
    }

    private void RefreshBalance()
    {
        if (_balValue == null) return;

        var s = ExpenseService.GetSummary(
            new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            null,
            ExpenseService.GetSettings().DefaultCurrency);

        _balValue.Text = s.DisplayBalance;

        if (_incomeText != null)
            _incomeText.Text = s.DisplayIncome;

        if (_expText != null)
            _expText.Text = s.DisplayExpense;

        _balValue.Foreground = s.Balance >= 0
            ? Res("WidgetTextPrimary")
            : Res("DangerBrush");
    }

    private void RefreshList()
    {
        if (_listStack == null) return;

        _listStack.Children.Clear();

        var currency = ExpenseService.GetSettings().DefaultCurrency;
        var items = ExpenseService.GetFiltered(_currentPeriod, currency);

        if (items.Count == 0)
        {
            var empty = new TextBlock
            {
                Text = L("expense.empty"),
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            empty.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
            _listStack.Children.Add(empty);
            return;
        }

        var grouped = items.GroupBy(e => e.Date.Date).OrderByDescending(g => g.Key);

        foreach (var group in grouped)
        {
            var headerRow = new TextBlock
            {
                Text = FormatDateHeader(group.Key),
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4)
            };
            headerRow.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
            _listStack.Children.Add(headerRow);

            foreach (var exp in group.OrderByDescending(e => e.Date))
            {
                _listStack.Children.Add(BuildItemRow(exp, _isRtl, RefreshAll));
            }
        }
    }

    private void UpdateTabs()
    {
        foreach (var kvp in _tabButtons)
        {
            var btn = kvp.Value;

            if (kvp.Key == _currentPeriod)
            {
                btn.Background = Res("AccentBrush");
                btn.Foreground = Brushes.White;
                btn.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                btn.Background = Brushes.Transparent;
                btn.Foreground = Res("WidgetTextMuted");
                btn.FontWeight = FontWeights.Normal;
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Chart — ★ حالا 7 میله‌ی خالی هم می‌کشه
    // ═══════════════════════════════════════════
    private void DrawChart()
    {
        if (_chartCanvas == null) return;

        _chartCanvas.Children.Clear();

        var currency = ExpenseService.GetSettings().DefaultCurrency;
        var data = ExpenseService.GetLast7Days(currency);
        if (data.Count == 0) return;

        double w = _chartCanvas.ActualWidth;
        double h = _chartCanvas.ActualHeight;
        if (w <= 0 || h <= 0) return;

        double maxVal = data.Max(d => Math.Max(d.Income, d.Expense));
        if (maxVal <= 0) maxVal = 1;

        int n = data.Count;
        double gap = 4;
        double barW = (w - (n - 1) * gap) / n;
        if (barW <= 0) return;

        for (int i = 0; i < n; i++)
        {
            var (date, income, expense) = data[i];
            double x = i * (barW + gap);

            // ★ Background bar (همیشه) — میله‌ی خالی روزهای بدون داده
            var bgBar = new Rectangle
            {
                Width = barW,
                Height = h - 4,
                Fill = new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF)),
                RadiusX = 4,
                RadiusY = 4
            };
            Canvas.SetLeft(bgBar, x);
            Canvas.SetTop(bgBar, 2);
            _chartCanvas.Children.Add(bgBar);

            // ── Expense bar ──
            if (expense > 0)
            {
                double barH = (expense / maxVal) * (h - 4);
                var rect = new Rectangle
                {
                    Width = barW,
                    Height = Math.Max(4, barH),
                    Fill = new SolidColorBrush(Color.FromArgb(0xCC, 0xE0, 0x52, 0x52)),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, h - barH);
                _chartCanvas.Children.Add(rect);
            }

            // ── Income bar ──
            if (income > 0)
            {
                double barH = (income / maxVal) * (h - 4);
                var rect = new Rectangle
                {
                    Width = barW / 2,
                    Height = Math.Max(4, barH),
                    Fill = new SolidColorBrush(Color.FromArgb(0xCC, 0x6F, 0xBF, 0x4A)),
                    RadiusX = 2,
                    RadiusY = 2
                };
                Canvas.SetLeft(rect, x + barW / 4);
                Canvas.SetTop(rect, h - barH);
                _chartCanvas.Children.Add(rect);
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Item Row — ★ گرد
    // ═══════════════════════════════════════════
    private static FrameworkElement BuildItemRow(Expense exp, bool isRtl, Action onChanged)
    {
        var cat = ExpenseService.GetCategory(exp.CategoryId);

        var row = new Border
        {
            CornerRadius = new CornerRadius(RadiusRow),   // ★ گرد
            Padding = new Thickness(10, 8, 10, 8),
            Margin = new Thickness(0, 2, 0, 2),
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // ── Icon ──
        Color catColor;
        try { catColor = (Color)ColorConverter.ConvertFromString(cat.Color); }
        catch { catColor = Color.FromRgb(0x88, 0x88, 0x88); }

        var iconBorder = new Border
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(Color.FromArgb(0x33, catColor.R, catColor.G, catColor.B))
        };

        iconBorder.Child = new TextBlock
        {
            Text = cat.Icon,
            FontFamily = IconFont,
            FontSize = 14,
            Foreground = new SolidColorBrush(catColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(iconBorder, 0);
        grid.Children.Add(iconBorder);

        // ── Note ──
        var noteStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 4, 0)
        };

        var noteText = new TextBlock
        {
            Text = string.IsNullOrEmpty(exp.Note) ? cat.GetName(isRtl) : exp.Note,
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 160
        };
        noteText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        noteStack.Children.Add(noteText);

        var subLine = new TextBlock
        {
            Text = $"{cat.GetName(isRtl)} · {exp.Date:HH:mm}",
            FontSize = 9,
            Margin = new Thickness(0, 1, 0, 0)
        };
        subLine.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        noteStack.Children.Add(subLine);

        Grid.SetColumn(noteStack, 2);
        grid.Children.Add(noteStack);

        // ── Amount ──
        var amountText = new TextBlock
        {
            Text = exp.DisplayAmount,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        amountText.Foreground = exp.IsIncome
            ? new SolidColorBrush(Color.FromRgb(0x6F, 0xBF, 0x4A))
            : Res("WidgetTextPrimary");
        Grid.SetColumn(amountText, 3);
        grid.Children.Add(amountText);

        row.Child = grid;

        row.MouseEnter += (_, _) =>
            row.SetResourceReference(Border.BackgroundProperty, "WidgetBgElevated");
        row.MouseLeave += (_, _) =>
            row.Background = Brushes.Transparent;

        row.MouseLeftButtonUp += (_, e) =>
        {
            e.Handled = true;
            ExpenseEntryDialog.Show(exp, _ => onChanged?.Invoke());
        };

        var cm = new ContextMenu();

        var miEdit = new MenuItem { Header = L("common.edit") };
        miEdit.Click += (_, _) => ExpenseEntryDialog.Show(exp, _ => onChanged?.Invoke());
        cm.Items.Add(miEdit);

        var miDelete = new MenuItem { Header = L("common.delete") };
        miDelete.Click += (_, _) =>
        {
            var res = MessageBox.Show(
                L("expense.confirmDelete"),
                L("common.confirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                ExpenseService.Remove(exp.Id);
                onChanged?.Invoke();
                App.Notifications.Show("WDesk", L("expense.deleted"), NotificationType.Info);
            }
        };
        cm.Items.Add(miDelete);

        row.ContextMenu = cm;

        return row;
    }

    // ═══════════════════════════════════════════
    //  Date Header
    // ═══════════════════════════════════════════
    private static string FormatDateHeader(DateTime date)
    {
        var today = DateTime.Today;
        var days = (today - date.Date).Days;

        if (days == 0) return L("common.today");
        if (days == 1) return L("common.yesterday");
        if (days < 7) return string.Format(L("common.daysAgo"), days);

        var culture = LocalizationHelper.Instance.IsRtl
            ? new CultureInfo("fa-IR")
            : CultureInfo.InvariantCulture;

        return date.ToString("yyyy/MM/dd", culture);
    }

    // ═══════════════════════════════════════════
    //  Templates — ★ Button گرد
    // ═══════════════════════════════════════════
    private static ControlTemplate BuildButtonTemplate(double radius)
    {
        var template = new ControlTemplate(typeof(Button));

        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "Bd";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
        border.SetValue(Border.BackgroundProperty,
            new TemplateBindingExtension(Button.BackgroundProperty));
        border.SetValue(Border.PaddingProperty,
            new TemplateBindingExtension(Button.PaddingProperty));

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

        border.AppendChild(presenter);
        template.VisualTree = border;

        // Hover
        var hover = new Trigger
        {
            Property = Button.IsMouseOverProperty,
            Value = true
        };
        hover.Setters.Add(new Setter
        {
            Property = Border.OpacityProperty,
            TargetName = "Bd",
            Value = 0.85
        });
        template.Triggers.Add(hover);

        // Pressed
        var pressed = new Trigger
        {
            Property = Button.IsPressedProperty,
            Value = true
        };
        pressed.Setters.Add(new Setter
        {
            Property = Border.OpacityProperty,
            TargetName = "Bd",
            Value = 0.7
        });
        template.Triggers.Add(pressed);

        return template;
    }
}