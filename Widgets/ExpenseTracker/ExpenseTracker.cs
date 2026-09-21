using System.Windows;
using WDesk.Core;
using WDesk.Widgets.ExpenseTracker.Style;

namespace WDesk.Widgets.ExpenseTracker;

public class ExpenseTrackerWidget : WidgetBase
{
    public ExpenseTrackerWidget()
    {
        ExpenseService.Initialize();
    }

    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "expensetracker",
        NameKey = "widget.expensetracker.name",
        DescriptionKey = "widget.expensetracker.desc",
        Category = WidgetCategory.Productivity,
        Icon = "\uE8C7",
        DefaultWidth = 340,
        DefaultHeight = 420,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}