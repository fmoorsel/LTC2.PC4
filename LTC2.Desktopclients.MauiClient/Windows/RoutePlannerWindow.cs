using LTC2.Desktopclients.MauiClient.Pages;
using Microsoft.Maui.Controls;

namespace LTC2.Desktopclients.MauiClient.Windows
{
    public class RoutePlannerWindow : Window
    {
        public RoutePlannerWindow(RoutePlannerPage page) : base(page)
        {
            Title = "LTC2 Route Planner";
            MinimumWidth = 1200;
            MinimumHeight = 380;
        }
    }
}
