using System;
using System.Collections.Generic;
using System.Globalization;

namespace WDesk.Widgets.ExpenseTracker;

// ═══════════════════════════════════════════
//  Expense Model
// ═══════════════════════════════════════════
public class Expense
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public double Amount { get; set; }              // مثبت = درآمد، منفی = خرج
    public string CategoryId { get; set; } = "other";
    public string Note { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string Currency { get; set; } = "IRT";    // IRT, USD, EUR

    public bool IsIncome => Amount > 0;

    // ── Display Helpers ──
    public string DisplayAmount
    {
        get
        {
            var abs = Math.Abs(Amount);
            var sign = Amount >= 0 ? "+" : "−";
            var isRtl = WDesk.Helpers.LocalizationHelper.Instance.IsRtl;

            return Currency switch
            {
                "USD" => $"{sign}${abs:N2}",
                "EUR" => $"{sign}€{abs:N2}",
                _ => isRtl
                    ? $"{sign}{abs.ToString("N0", new CultureInfo("fa-IR"))} تومان"
                    : $"{sign}{abs:N0} Toman"
            };
        }
    }

    public string DisplayDate
    {
        get
        {
            var now = DateTime.Now;
            var days = (now.Date - Date.Date).Days;

            return days switch
            {
                0 => "امروز",
                1 => "دیروز",
                < 7 => $"{days} روز پیش",
                _ => Date.ToString("yyyy/MM/dd", new CultureInfo("fa-IR"))
            };
        }
    }
}

// ═══════════════════════════════════════════
//  Category Model
// ═══════════════════════════════════════════
public class ExpenseCategory
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string NameFa { get; set; } = "";
    public string Icon { get; set; } = "\uE8C7";    // Segoe Fluent glyph
    public string Color { get; set; } = "#8FB339";  // hex
    public bool IsIncome { get; set; } = false;     // دسته‌بندی درآمد؟

    public string GetName(bool isRtl) =>
        isRtl && !string.IsNullOrEmpty(NameFa) ? NameFa : Name;
}

// ═══════════════════════════════════════════
//  Summary Model
// ═══════════════════════════════════════════
public class ExpenseSummary
{
    public double TotalIncome { get; set; }
    public double TotalExpense { get; set; }   // مقدار مطلق
    public double Balance => TotalIncome - TotalExpense;
    public int Count { get; set; }

    public string Currency { get; set; } = "IRT";

    public string DisplayBalance
    {
        get
        {
            var sign = Balance >= 0 ? "+" : "−";
            var abs = Math.Abs(Balance);

            return Currency switch
            {
                "USD" => $"{sign}${abs:N2}",
                "EUR" => $"{sign}€{abs:N2}",
                _ => $"{sign}{abs.ToString("N0", new CultureInfo("fa-IR"))} تومان"
            };
        }
    }

    public string DisplayIncome
    {
        get
        {
            return Currency switch
            {
                "USD" => $"${TotalIncome:N2}",
                "EUR" => $"€{TotalIncome:N2}",
                _ => $"{TotalIncome.ToString("N0", new CultureInfo("fa-IR"))} تومان"
            };
        }
    }

    public string DisplayExpense
    {
        get
        {
            return Currency switch
            {
                "USD" => $"${TotalExpense:N2}",
                "EUR" => $"€{TotalExpense:N2}",
                _ => $"{TotalExpense.ToString("N0", new CultureInfo("fa-IR"))} تومان"
            };
        }
    }
}