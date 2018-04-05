using Plugin.Media;
using System;

using Xamarin.Forms;

namespace CustomerRecognition.Forms
{
    public class MainPage : TabbedPage
    {
        IStatefulContent currentStatefulPage;

        public MainPage()
        {
            CrossMedia.Current.Initialize();

            var textColor = Color.White;
            var backgroundColor = Color.FromRgb(73, 113, 175);

            // Bug where UWP crashes if the camera has been requested in two locations
            if (Device.RuntimePlatform != Device.UWP)
            {
                var captureNavigationPage = new NavigationPage(new CapturePage());
                captureNavigationPage.Title = "New";
                captureNavigationPage.Icon = "person.png";
                captureNavigationPage.BarTextColor = textColor;
                captureNavigationPage.BarBackgroundColor = backgroundColor;
                Children.Add(captureNavigationPage);
            }

            var orderNavigationPage = new NavigationPage(new OrdersPage());
            orderNavigationPage.Title = "Orders";
            orderNavigationPage.Icon = "orders.png";
            orderNavigationPage.BarTextColor = textColor;
            orderNavigationPage.BarBackgroundColor = backgroundColor;
            Children.Add(orderNavigationPage);

            var identifyNavigationPage = new NavigationPage(new IdentifyCustomersPage());
            identifyNavigationPage.Title = "Identify";
            identifyNavigationPage.Icon = "capture.png";
            identifyNavigationPage.BarTextColor = textColor;
            identifyNavigationPage.BarBackgroundColor = backgroundColor;
            Children.Add(identifyNavigationPage);

            var settingsNavigationPage = new NavigationPage(new SettingsPage());
            settingsNavigationPage.Title = "Settings";
            settingsNavigationPage.Icon = "settings.png";
            settingsNavigationPage.BarTextColor = textColor;
            settingsNavigationPage.BarBackgroundColor = backgroundColor;
            Children.Add(settingsNavigationPage);
        }

        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            var navPage = (NavigationPage)CurrentPage;
            if (currentStatefulPage != null && currentStatefulPage != navPage.CurrentPage)
            {
                currentStatefulPage.DidDisappear();
                currentStatefulPage = null;
            }

            if (navPage.CurrentPage is IStatefulContent)
            {
                var stateful = (IStatefulContent)navPage.CurrentPage;
                stateful.DidAppear();
                currentStatefulPage = stateful;
            }
        }
    }
}

