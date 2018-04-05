using System;
using System.IO;
using CustomerRecognition.Forms.Models;
using Plugin.Media;
using Xamarin.Forms;

namespace CustomerRecognition.Forms
{
    public class CapturePage : ContentPage, IStatefulContent
    {
        ICreateOrderModel customerModel;
        CameraPreview cameraPreview;
        ActivityIndicator activityIndicator;

        public CapturePage()
        {
            BindingContext = customerModel = ServiceContainer.Resolve<ICreateOrderModel>();
            Title = "Identify";

            // Activity Indicator
            activityIndicator = new ActivityIndicator();
            activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;

            this.ToolbarItems.Add(
                new ToolbarItem("Pick Photo", null, () => PickPhoto()) { Icon = "folder.png" }
            );

            if (Device.RuntimePlatform == Device.UWP || CrossMedia.Current.IsCameraAvailable)
            {
                BackgroundColor = Color.Black;

                // Camera Preview
                cameraPreview = new CameraPreview();
                cameraPreview.Filename = "capture";
                cameraPreview.CameraOption = Settings.CameraOption;
                cameraPreview.CapturePathCallback = new Action<string>(ProcessCameraPhoto);
                cameraPreview.CameraReady += (s, e) => StartCamera();

                AbsoluteLayout.SetLayoutBounds(cameraPreview, new Rectangle(1, 1, 1, 1));
                AbsoluteLayout.SetLayoutFlags(cameraPreview, AbsoluteLayoutFlags.All);

                // Capture Button
                var buttonSize = 60;
                var captureButton = new Button();
                captureButton.Clicked += CaptureButton_Clicked;
                captureButton.BackgroundColor = Color.White;
                captureButton.WidthRequest = buttonSize;
                captureButton.HeightRequest = buttonSize;
                captureButton.BorderRadius = buttonSize / 2;
                captureButton.BorderWidth = 1;
                captureButton.BorderColor = Color.Black;
                captureButton.HorizontalOptions = LayoutOptions.Center;

                AbsoluteLayout.SetLayoutBounds(captureButton, new Rectangle(.5, .9, buttonSize, buttonSize));
                AbsoluteLayout.SetLayoutFlags(captureButton, AbsoluteLayoutFlags.PositionProportional);

                activityIndicator.Color = Color.White;
                AbsoluteLayout.SetLayoutBounds(activityIndicator, new Rectangle(.5, .5, buttonSize, buttonSize));
                AbsoluteLayout.SetLayoutFlags(activityIndicator, AbsoluteLayoutFlags.PositionProportional);

                var layout = new AbsoluteLayout();
                layout.Children.Add(cameraPreview);
                layout.Children.Add(captureButton);
                layout.Children.Add(activityIndicator);

                Content = layout;

                this.ToolbarItems.Add(
                    new ToolbarItem("Toggle Camera", null, () => ToggleCamera()) { Icon = "toggle.png" }
                );
            }

            else
            {
                var label = new Label()
                {
                    Text = "Camera Not Available",
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                activityIndicator.Color = Color.Black;
                activityIndicator.HorizontalOptions = LayoutOptions.CenterAndExpand;

                var centerLayout = new StackLayout();
                centerLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
                centerLayout.VerticalOptions = LayoutOptions.CenterAndExpand;
                centerLayout.Children.Add(label);
                centerLayout.Children.Add(activityIndicator);

                Content = centerLayout;
            }
        }

        void ShowActivityIndicator(bool value)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                activityIndicator.IsVisible = value;
                activityIndicator.IsRunning = value;
                if (cameraPreview != null) cameraPreview.Opacity = value ? .6 : 1;
            });
        }

        async void ProcessCameraPhoto(string path)
        {
            byte[] imageBytes = File.ReadAllBytes(path);

            ShowActivityIndicator(true);
            var result = await customerModel.IdentifyCustomer(imageBytes);
            ShowActivityIndicator(false);

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (result)
                    await Navigation.PushAsync(new CreateOrderPage());
                else
                    await DisplayAlert("Uh Oh", "Something went wrong!", "OK");
            });
        }

        async void PickPhoto()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            });

            if (file == null)
                return;

            byte[] imageBytes = null;

            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                imageBytes = memoryStream.ToArray();
            }

            ShowActivityIndicator(true);
            var result = await customerModel.IdentifyCustomer(imageBytes);
            ShowActivityIndicator(false);

            if (result)
                await Navigation.PushAsync(new CreateOrderPage());
            else
                await DisplayAlert("Uh Oh", "Something went wrong!", "OK");
        }

        void CaptureButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable)
            {
                DisplayAlert("Camera Not Availalble", ":( Permission not granted to camera.", "OK");
                return;
            }

            if (cameraPreview != null && cameraPreview.Capture != null)
                cameraPreview.Capture.Execute(null);
        }

        void ToggleCamera()
        {
            if (!CrossMedia.Current.IsCameraAvailable)
            {
                DisplayAlert("Camera Not Availalble", ":( Permission not granted to camera.", "OK");
                return;
            }

            if (cameraPreview != null && cameraPreview.Capture != null)
                cameraPreview.CameraOption = (cameraPreview.CameraOption == CameraOptions.Rear) ? CameraOptions.Front : CameraOptions.Rear;
        }

        void StopCamera()
        {
            if (cameraPreview != null && cameraPreview.StopCamera != null)
                cameraPreview.StopCamera.Execute(null);
        }

        void StartCamera()
        {
            if (cameraPreview != null && cameraPreview.StartCamera != null)
            {
                cameraPreview.CameraOption = Settings.CameraOption;
                cameraPreview.StartCamera.Execute(null);
            }
        }

        public void DidAppear()
        {
            StartCamera();
        }

        public void DidDisappear()
        {
            StopCamera();
        }
    }
}

