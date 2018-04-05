using System.Drawing;
using CustomerRecognition.Forms;
using CustomerRecognition.Forms.Models;
using Xamarin.Forms;

namespace CustomerRecognition
{
    public partial class App : Application
    {
        public static int ScreenWidth;
        public static int ScreenHeight;
        public static int CameraRatio;

        public App()
        {
            InitializeComponent();

            ServiceContainer.Register<ICreateOrderModel>(new CreateOrderModel());
            ServiceContainer.Register<IOrdersModel>(new OrdersModel());

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
