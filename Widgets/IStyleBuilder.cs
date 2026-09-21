using System.Windows;
using WDesk.Core;

namespace WDesk.Widgets;

public interface IStyleBuilder
{
    FrameworkElement Build(PlacedWidget instance);
}