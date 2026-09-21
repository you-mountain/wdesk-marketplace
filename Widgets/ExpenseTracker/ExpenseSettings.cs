using System.Collections.Generic;

namespace WDesk.Widgets.ExpenseTracker;

public class ExpenseSettings
{
    /// <summary>ارز پیش‌فرض: IRT, USD, EUR</summary>
    public string DefaultCurrency { get; set; } = "IRT";

    /// <summary>آیا از راست‌به‌چپ استفاده کنه؟</summary>
    public bool AutoRtl { get; set; } = true;

    /// <summary>دسته‌بندی‌های سفارشی کاربر</summary>
    public List<ExpenseCategory> Categories { get; set; } = new();

    // ═══════════════════════════════════════════
    //  Default Categories
    // ═══════════════════════════════════════════
    public static List<ExpenseCategory> GetDefaultCategories()
    {
        return new List<ExpenseCategory>
        {
            // ── خرج ──
            new() { Id = "food",      Name = "Food",      NameFa = "خوراک",     Icon = "\uE8CB", Color = "#E05252" },
            new() { Id = "transport", Name = "Transport", NameFa = "حمل‌ونقل",  Icon = "\uE804", Color = "#5A9FE0" },
            new() { Id = "shopping",  Name = "Shopping",  NameFa = "خرید",      Icon = "\uE7BF", Color = "#F0A040" },
            new() { Id = "bills",     Name = "Bills",     NameFa = "قبض",       Icon = "\uE9F9", Color = "#9B59B6" },
            new() { Id = "fun",       Name = "Fun",       NameFa = "سرگرمی",    Icon = "\uE7FC", Color = "#E91E63" },
            new() { Id = "health",    Name = "Health",    NameFa = "سلامت",     Icon = "\uE95E", Color = "#4CAF50" },
            new() { Id = "education", Name = "Education", NameFa = "آموزش",     Icon = "\uE7BE", Color = "#00BCD4" },
            new() { Id = "rent",      Name = "Rent",      NameFa = "اجاره",     Icon = "\uE80F", Color = "#795548" },
            new() { Id = "other",     Name = "Other",     NameFa = "متفرقه",    Icon = "\uE8C7", Color = "#888888" },

            // ── درآمد ──
            new() { Id = "salary",    Name = "Salary",    NameFa = "حقوق",      Icon = "\uE8C8", Color = "#6FBF4A", IsIncome = true },
            new() { Id = "gift",      Name = "Gift",      NameFa = "هدیه",      Icon = "\uE8F4", Color = "#8FB339", IsIncome = true },
            new() { Id = "other_inc", Name = "Other Inc", NameFa = "درآمد دیگر", Icon = "\uE8C7", Color = "#4CAF50", IsIncome = true },
        };
    }
}