using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Virtion.Util
{
    class UIHelper
    {
        public Button GetChildUIElement(DependencyObject ui)
        {
            Button grandChild = null;
            int count = VisualTreeHelper.GetChildrenCount(ui);
            for (int i = 0; i <= count - 1; i++)
            {
                var child = VisualTreeHelper.GetChild(ui, i);
                if (child is Button)
                {
                    return (child as Button);
                }
                else
                {
                    grandChild = GetChildUIElement(child);
                    if (grandChild != null)
                        return grandChild;
                }
            }
            return null;
        }

    }
}
