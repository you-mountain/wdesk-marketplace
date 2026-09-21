using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using WDesk.Core;

namespace WDesk.Widgets.ExpenseTracker;

public static class ExpenseService
{
    private static readonly object _lock = new();
    private static List<Expense> _expenses = new();
    private static List<ExpenseCategory> _categories = new();
    private static ExpenseSettings _settings = new();
    private static bool _initialized = false;

    // ═══════════════════════════════════════════
    //  Paths
    // ═══════════════════════════════════════════
    private static string BaseDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WDesk");

    private static string ExpensesPath => Path.Combine(BaseDir, "expenses.json");
    private static string CategoriesPath => Path.Combine(BaseDir, "expense_categories.json");
    private static string SettingsPath => Path.Combine(BaseDir, "expense_settings.json");

    // ═══════════════════════════════════════════
    //  Init
    // ═══════════════════════════════════════════
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            Directory.CreateDirectory(BaseDir);

            LoadExpenses();
            LoadCategories();
            LoadSettings();

            App.Logger?.Info($"ExpenseTracker: loaded {_expenses.Count} expenses, {_categories.Count} categories");
        }
        catch (Exception ex)
        {
            App.Logger?.Error("ExpenseTracker: init failed", ex);
        }
    }

    // ═══════════════════════════════════════════
    //  Public Accessors
    // ═══════════════════════════════════════════
    public static IReadOnlyList<Expense> GetExpenses()
    {
        lock (_lock) return _expenses.ToList();
    }

    public static IReadOnlyList<ExpenseCategory> GetCategories()
    {
        lock (_lock) return _categories.ToList();
    }

    public static ExpenseSettings GetSettings() => _settings;

    public static ExpenseCategory GetCategory(string id)
    {
        lock (_lock)
            return _categories.FirstOrDefault(c => c.Id == id)
                   ?? _categories.FirstOrDefault(c => c.Id == "other")
                   ?? new ExpenseCategory { Id = "other", Name = "Other", NameFa = "متفرقه" };
    }

    // ═══════════════════════════════════════════
    //  CRUD
    // ═══════════════════════════════════════════
    public static void Add(Expense expense)
    {
        lock (_lock)
        {
            _expenses.Add(expense);
            SaveExpenses();
        }
        App.Logger?.Info($"ExpenseTracker: added {expense.DisplayAmount} ({expense.CategoryId})");
    }

    public static void Update(Expense expense)
    {
        lock (_lock)
        {
            var idx = _expenses.FindIndex(e => e.Id == expense.Id);
            if (idx >= 0)
            {
                _expenses[idx] = expense;
                SaveExpenses();
            }
        }
    }

    public static void Remove(string id)
    {
        lock (_lock)
        {
            _expenses.RemoveAll(e => e.Id == id);
            SaveExpenses();
        }
        App.Logger?.Info($"ExpenseTracker: removed {id}");
    }

    public static void ClearAll()
    {
        lock (_lock)
        {
            _expenses.Clear();
            SaveExpenses();
        }
    }

    // ═══════════════════════════════════════════
    //  Summary
    // ═══════════════════════════════════════════
    public static ExpenseSummary GetSummary(DateTime? from = null, DateTime? to = null, string currency = null)
    {
        lock (_lock)
        {
            var q = _expenses.AsEnumerable();

            if (from.HasValue) q = q.Where(e => e.Date >= from.Value);
            if (to.HasValue) q = q.Where(e => e.Date < to.Value);

            currency ??= _settings.DefaultCurrency;

            // ★ فیلتر ارز: فقط هزینه‌هایی که ارزشون با currency می‌خونه
            q = q.Where(e => e.Currency == currency);

            var list = q.ToList();

            return new ExpenseSummary
            {
                TotalIncome = list.Where(e => e.Amount > 0).Sum(e => e.Amount),
                TotalExpense = list.Where(e => e.Amount < 0).Sum(e => Math.Abs(e.Amount)),
                Count = list.Count,
                Currency = currency
            };
        }
    }

    /// <summary>خلاصه‌ی 7 روز اخیر (برای نمودار)</summary>
    public static List<(DateTime Date, double Income, double Expense)> GetLast7Days(string currency = null)
    {
        lock (_lock)
        {
            currency ??= _settings.DefaultCurrency;

            var today = DateTime.Today;
            var result = new List<(DateTime, double, double)>();

            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                var next = d.AddDays(1);

                var dayExpenses = _expenses
                    .Where(e => e.Date >= d && e.Date < next && e.Currency == currency)
                    .ToList();

                result.Add((
                    d,
                    dayExpenses.Where(e => e.Amount > 0).Sum(e => e.Amount),
                    dayExpenses.Where(e => e.Amount < 0).Sum(e => Math.Abs(e.Amount))
                ));
            }

            return result;
        }
    }

    // ═══════════════════════════════════════════
    //  Filter Helpers
    // ═══════════════════════════════════════════
    public static List<Expense> GetFiltered(string period, string currency = null)
    {
        lock (_lock)
        {
            currency ??= _settings.DefaultCurrency;

            var today = DateTime.Today;
            DateTime? from = period switch
            {
                "today" => today,
                "week" => today.AddDays(-(int)today.DayOfWeek),
                "month" => new DateTime(today.Year, today.Month, 1),
                _ => null
            };

            var q = _expenses.Where(e => e.Currency == currency);
            if (from.HasValue) q = q.Where(e => e.Date >= from.Value);

            return q.OrderByDescending(e => e.Date).ToList();
        }
    }

    // ═══════════════════════════════════════════
    //  Save / Load
    // ═══════════════════════════════════════════
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static void LoadExpenses()
    {
        if (!File.Exists(ExpensesPath)) { _expenses = new(); return; }
        try
        {
            var json = File.ReadAllText(ExpensesPath);
            _expenses = JsonSerializer.Deserialize<List<Expense>>(json, _jsonOpts) ?? new();
        }
        catch (Exception ex)
        {
            App.Logger?.Error("ExpenseTracker: load expenses failed", ex);
            _expenses = new();
        }
    }

    private static void SaveExpenses()
    {
        try
        {
            var json = JsonSerializer.Serialize(_expenses, _jsonOpts);
            File.WriteAllText(ExpensesPath, json);
        }
        catch (Exception ex)
        {
            App.Logger?.Error("ExpenseTracker: save expenses failed", ex);
        }
    }

    private static void LoadCategories()
    {
        if (!File.Exists(CategoriesPath))
        {
            _categories = ExpenseSettings.GetDefaultCategories();
            SaveCategories();
            return;
        }
        try
        {
            var json = File.ReadAllText(CategoriesPath);
            _categories = JsonSerializer.Deserialize<List<ExpenseCategory>>(json, _jsonOpts)
                          ?? ExpenseSettings.GetDefaultCategories();
        }
        catch
        {
            _categories = ExpenseSettings.GetDefaultCategories();
        }
    }

    private static void SaveCategories()
    {
        try
        {
            var json = JsonSerializer.Serialize(_categories, _jsonOpts);
            File.WriteAllText(CategoriesPath, json);
        }
        catch { }
    }

    private static void LoadSettings()
    {
        if (!File.Exists(SettingsPath)) { _settings = new(); return; }
        try
        {
            var json = File.ReadAllText(SettingsPath);
            _settings = JsonSerializer.Deserialize<ExpenseSettings>(json, _jsonOpts) ?? new();
        }
        catch { _settings = new(); }
    }

    public static void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, _jsonOpts);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}