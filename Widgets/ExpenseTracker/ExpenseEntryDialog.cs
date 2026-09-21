using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WDesk.Helpers;

namespace WDesk.Widgets.ExpenseTracker;

public static class ExpenseEntryDialog
{
    // ═══════════════════════════════════════════
    //  Palette
    // ═══════════════════════════════════════════
    private static Brush Res(string key) =>
        (Brush)Application.Current.FindResource(key);

    private static readonly FontFamily IconFont =
        new("Segoe Fluent Icons, Segoe MDL2 Assets");

    // ═══════════════════════════════════════════
    //  Show
    // ═══════════════════════════════════════════
    public static void Show(Expense? expense, Action<Expense> onSave)
    {
        var isEdit = expense != null;
        var categories = ExpenseService.GetCategories().ToList();
        var settings = ExpenseService.GetSettings();
        var isRtl = LocalizationHelper.Instance.IsRtl;

        // ═══ Window ═══
        var win = new Window
        {
            Title = L(isEdit ? "expense.dialog.edit" : "expense.dialog.add"),
            Width = 480,
            Height = 660,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current?.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow,
            ShowInTaskbar = false,
            Background = Res("BgBase"),
            FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI, Tahoma")
        };

        var root = new StackPanel { Margin = new Thickness(24) };

        // ═══ Header ═══
        var header = new TextBlock
        {
            Text = L(isEdit ? "expense.dialog.edit" : "expense.dialog.add"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = Res("TextPrimary"),
            Margin = new Thickness(0, 0, 0, 20)
        };
        root.Children.Add(header);

        // ═══════════════════════════════════════
        //  Amount + Currency Chips
        // ═══════════════════════════════════════
        root.Children.Add(MakeLabel(L("expense.field.amount")));

        var amountBox = MakeTextBox(
            isEdit ? Math.Abs(expense!.Amount).ToString(CultureInfo.InvariantCulture) : "",
            big: true);
        amountBox.Margin = new Thickness(0, 0, 0, 10);
        root.Children.Add(amountBox);

        // ★★ Currency Chips (به جای ComboBox)
        var currencyWrap = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };

        var currentCurrency = isEdit ? expense!.Currency : settings.DefaultCurrency;
        string selectedCurrency = currentCurrency;

        var currencyChips = new Dictionary<string, Button>();

        var currencyOptions = new (string Code, string Key)[]
        {
            ("IRT", "expense.currency.irt"),
            ("USD", "expense.currency.usd"),
            ("EUR", "expense.currency.eur"),
        };

        foreach (var (code, key) in currencyOptions)
        {
            var chip = MakeCurrencyChip(L(key));

            if (code == selectedCurrency)
            {
                chip.Background = Res("AccentBrush");
                chip.Foreground = Brushes.White;
            }

            var capturedCode = code;
            chip.Click += (_, _) =>
            {
                selectedCurrency = capturedCode;
                foreach (var kvp in currencyChips)
                {
                    if (kvp.Key == selectedCurrency)
                    {
                        kvp.Value.Background = Res("AccentBrush");
                        kvp.Value.Foreground = Brushes.White;
                    }
                    else
                    {
                        kvp.Value.Background = Res("BgElevated");
                        kvp.Value.Foreground = Res("TextSecondary");
                    }
                }
            };

            currencyChips[code] = chip;
            currencyWrap.Children.Add(chip);
        }

        root.Children.Add(currencyWrap);

        // ═══════════════════════════════════════
        //  Type
        // ═══════════════════════════════════════
        root.Children.Add(MakeLabel(L("expense.field.type")));

        var typeGrid = new Grid { Margin = new Thickness(0, 0, 0, 16) };
        typeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        typeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        typeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var expenseBtn = MakeTypeButton("⬇  " + L("expense.type.expense"));
        var incomeBtn = MakeTypeButton("⬆  " + L("expense.type.income"));

        Grid.SetColumn(expenseBtn, 0);
        Grid.SetColumn(incomeBtn, 2);
        typeGrid.Children.Add(expenseBtn);
        typeGrid.Children.Add(incomeBtn);

        root.Children.Add(typeGrid);

        bool isIncome = isEdit && expense!.Amount > 0;

        void UpdateTypeButtons()
        {
            if (isIncome)
            {
                expenseBtn.Background = Res("BgElevated");
                expenseBtn.Foreground = Res("TextSecondary");
                incomeBtn.Background = Res("SuccessBrush");
                incomeBtn.Foreground = Brushes.White;
            }
            else
            {
                expenseBtn.Background = Res("DangerBrush");
                expenseBtn.Foreground = Brushes.White;
                incomeBtn.Background = Res("BgElevated");
                incomeBtn.Foreground = Res("TextSecondary");
            }
        }

        // ═══════════════════════════════════════
        //  Category
        // ═══════════════════════════════════════
        root.Children.Add(MakeLabel(L("expense.field.category")));

        var catScroll = new ScrollViewer
        {
            MaxHeight = 140,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var catWrap = new WrapPanel { Orientation = Orientation.Horizontal };
        catScroll.Content = catWrap;
        root.Children.Add(catScroll);

        string selectedCatId = isEdit ? expense!.CategoryId : "food";

        void RefreshCategories()
        {
            catWrap.Children.Clear();

            var filtered = categories.Where(c => c.IsIncome == isIncome).ToList();
            if (filtered.Count == 0) filtered = categories.Where(c => !c.IsIncome).ToList();

            if (!filtered.Any(c => c.Id == selectedCatId))
                selectedCatId = filtered.FirstOrDefault()?.Id ?? "other";

            foreach (var cat in filtered)
            {
                var btn = MakeCategoryChip(cat, isRtl);

                if (cat.Id == selectedCatId)
                {
                    btn.Background = Res("AccentBrush");
                    btn.Foreground = Brushes.White;
                }

                var capturedId = cat.Id;
                btn.Click += (_, _) =>
                {
                    selectedCatId = capturedId;
                    RefreshCategories();
                };

                catWrap.Children.Add(btn);
            }
        }

        expenseBtn.Click += (_, _) => { isIncome = false; UpdateTypeButtons(); RefreshCategories(); };
        incomeBtn.Click += (_, _) => { isIncome = true; UpdateTypeButtons(); RefreshCategories(); };
        UpdateTypeButtons();
        RefreshCategories();

        // ═══════════════════════════════════════
        //  Note
        // ═══════════════════════════════════════
        root.Children.Add(MakeLabel(L("expense.field.note")));

        var noteBox = MakeTextBox(isEdit ? expense!.Note : "", big: false);
        noteBox.Margin = new Thickness(0, 0, 0, 16);
        root.Children.Add(noteBox);

        // ═══════════════════════════════════════
        //  Date
        // ═══════════════════════════════════════
        root.Children.Add(MakeLabel(L("expense.field.date")));

        var dateBox = MakeTextBox(
            (isEdit ? expense!.Date : DateTime.Now).ToString("yyyy/MM/dd HH:mm"),
            big: false);
        dateBox.Margin = new Thickness(0, 0, 0, 20);
        root.Children.Add(dateBox);

        // ═══════════════════════════════════════
        //  Buttons
        // ═══════════════════════════════════════
        var btnRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = isRtl ? HorizontalAlignment.Left : HorizontalAlignment.Right
        };

        var cancelBtn = new Button
        {
            Content = L("common.cancel"),
            Padding = new Thickness(24, 10, 24, 10),
            Margin = new Thickness(0, 0, 8, 0),
            Background = Res("BgElevated"),
            Foreground = Res("TextPrimary"),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            FontSize = 13,
            Template = BuildButtonTemplate(14)
        };
        cancelBtn.Click += (_, _) => win.Close();

        var okBtn = new Button
        {
            Content = L(isEdit ? "common.save" : "common.add"),
            Padding = new Thickness(24, 10, 24, 10),
            Background = Res("AccentBrush"),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand,
            FontSize = 13,
            IsDefault = true,
            Template = BuildButtonTemplate(14)
        };

        Expense? result = null;

        okBtn.Click += (_, _) =>
        {
            var raw = amountBox.Text?.Trim().Replace(",", "").Replace("،", "") ?? "";
            if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
                || amount <= 0)
            {
                MessageBox.Show(L("expense.error.amount"), L("common.error"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                amountBox.Focus();
                return;
            }

            DateTime date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(dateBox.Text))
                if (!DateTime.TryParse(dateBox.Text, out date))
                    date = DateTime.Now;

            var exp = isEdit ? expense! : new Expense();
            exp.Amount = isIncome ? amount : -amount;
            exp.CategoryId = selectedCatId;
            exp.Note = noteBox.Text?.Trim() ?? "";
            exp.Date = date;
            exp.Currency = selectedCurrency;

            result = exp;
            win.Close();
        };

        win.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) win.Close();
        };

        btnRow.Children.Add(cancelBtn);
        btnRow.Children.Add(okBtn);
        root.Children.Add(btnRow);

        win.Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = root
        };

        win.ShowDialog();

        if (result != null)
        {
            if (isEdit) ExpenseService.Update(result);
            else ExpenseService.Add(result);

            onSave?.Invoke(result);
        }
    }

    // ═══════════════════════════════════════════
    //  Helpers — Localization
    // ═══════════════════════════════════════════
    private static string L(string key) => LocalizationHelper.T(key);

    // ═══════════════════════════════════════════
    //  Helpers — UI Elements
    // ═══════════════════════════════════════════
    private static TextBlock MakeLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = Res("TextSecondary"),
            Margin = new Thickness(0, 0, 0, 6)
        };
    }

    private static TextBox MakeTextBox(string text, bool big)
    {
        return new TextBox
        {
            Text = text,
            Padding = new Thickness(14, 12, 14, 12),
            FontSize = big ? 18 : 13,
            FontWeight = big ? FontWeights.SemiBold : FontWeights.Normal,
            BorderThickness = new Thickness(0),
            Background = Res("BgElevated"),
            Foreground = Res("TextPrimary"),
            CaretBrush = Res("AccentBrush"),
            SelectionBrush = Res("AccentBrush"),
            FlowDirection = FlowDirection.LeftToRight,
            Template = BuildTextBoxTemplate()
        };
    }

    // ★ Currency Chip
    private static Button MakeCurrencyChip(string text)
    {
        return new Button
        {
            Content = text,
            Padding = new Thickness(16, 8, 16, 8),
            Margin = new Thickness(0, 0, 8, 0),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Background = Res("BgElevated"),
            Foreground = Res("TextSecondary"),
            Template = BuildButtonTemplate(18)
        };
    }

    private static Button MakeTypeButton(string text)
    {
        return new Button
        {
            Content = text,
            Padding = new Thickness(16, 12, 16, 12),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Template = BuildButtonTemplate(14)
        };
    }

    private static Button MakeCategoryChip(ExpenseCategory cat, bool isRtl)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(new TextBlock
        {
            Text = cat.Icon,
            FontFamily = IconFont,
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });

        panel.Children.Add(new TextBlock
        {
            Text = cat.GetName(isRtl),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        });

        return new Button
        {
            Content = panel,
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 0, 6, 6),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Background = Res("BgElevated"),
            Foreground = Res("TextSecondary"),
            Template = BuildButtonTemplate(18)
        };
    }

    // ═══════════════════════════════════════════
    //  Templates
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

    private static ControlTemplate BuildTextBoxTemplate()
    {
        var template = new ControlTemplate(typeof(TextBox));

        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "Bd";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(14));
        border.SetValue(Border.BackgroundProperty,
            new TemplateBindingExtension(TextBox.BackgroundProperty));
        border.SetValue(Border.PaddingProperty,
            new TemplateBindingExtension(TextBox.PaddingProperty));

        var scroll = new FrameworkElementFactory(typeof(ScrollViewer));
        scroll.Name = "PART_ContentHost";
        scroll.SetValue(ScrollViewer.VerticalAlignmentProperty, VerticalAlignment.Center);
        scroll.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        scroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);

        border.AppendChild(scroll);
        template.VisualTree = border;

        var focus = new Trigger
        {
            Property = TextBox.IsFocusedProperty,
            Value = true
        };
        focus.Setters.Add(new Setter
        {
            Property = Border.BorderBrushProperty,
            TargetName = "Bd",
            Value = Res("AccentBrush")
        });
        focus.Setters.Add(new Setter
        {
            Property = Border.BorderThicknessProperty,
            TargetName = "Bd",
            Value = new Thickness(1.5)
        });
        template.Triggers.Add(focus);

        return template;
    }
}