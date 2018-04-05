using Plugin.Media;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace CustomerRecognition.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var size = new Size(980, 720);
            ApplicationView.PreferredLaunchViewSize = size;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            CustomerRecognition.App.ScreenWidth = (int)size.Width;
            CustomerRecognition.App.ScreenHeight = (int)size.Height;

            CrossMedia.Current.Initialize();

            LoadApplication(new CustomerRecognition.App());
        }
    }
}
