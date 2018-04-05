using CustomerRecognition.Forms;
using CustomerRecognition.UWP;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]
namespace CustomerRecognition.UWP
{
    public class CameraPreviewRenderer : ViewRenderer<CameraPreview, Windows.UI.Xaml.Controls.CaptureElement>
    {
        readonly DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
        readonly SimpleOrientationSensor orientationSensor = SimpleOrientationSensor.GetDefault();
        readonly DisplayRequest displayRequest = new DisplayRequest();
        SimpleOrientation deviceOrientation = SimpleOrientation.NotRotated;
        DisplayOrientations displayOrientation = DisplayOrientations.Portrait;

        // Rotation metadata to apply to preview stream (https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx)
        static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1"); // (MF_MT_VIDEO_ROTATION)

        CameraPreview element;
        MediaCapture mediaCapture;
        CaptureElement captureElement;
        bool isInitialized;
        bool isPreviewing;
        bool externalCamera;
        bool mirroringPreview;
        bool autoPreview;
        string captureFilename;

        Windows.UI.Xaml.Application app;
        CameraOptions cameraOptions;
        Action<string> capturePathCallbackAction;

        protected override void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                app = Windows.UI.Xaml.Application.Current;
                app.Suspending += OnAppSuspending;
                app.Resuming += OnAppResuming;

                element = e.NewElement;
                cameraOptions = element.Camera;
                captureElement = new CaptureElement();
                captureElement.Stretch = Stretch.UniformToFill;

                SetupCamera();
                SetNativeControl(captureElement);
            }
            if (e.OldElement != null)
            {
                capturePathCallbackAction = null;
                element.Capture = null;
                element.StartCamera = null;
                element.StopCamera = null;
                captureFilename = "temp";
            }
            if (e.NewElement != null)
            {
                capturePathCallbackAction = element.CapturePathCallback;
                element.Capture = new Command(() => CaptureToFile());
                element.StartCamera = new Command(async () => await StartPreviewAsync());
                element.StopCamera = new Command(async () => await StopPreviewAsync());
                captureFilename = element.Filename;
            }
        }

        async void SetupCamera()
        {
            await SetupUIAsync();
            await InitializeCameraAsync();
        }

        private async void CaptureToFile()
        {
            if (capturePathCallbackAction == null || mediaCapture == null)
                return;

            var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
            var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

            using (var frame = await mediaCapture.GetPreviewFrameAsync(videoFrame))
            {
                var frameBitmap = frame.SoftwareBitmap;
                WriteableBitmap bitmap = new WriteableBitmap(frameBitmap.PixelWidth, frameBitmap.PixelHeight);
                frameBitmap.CopyToBuffer(bitmap.PixelBuffer);

                var file = await ImageUtils.WriteableBitmapToStorageFile(bitmap, ImageUtils.FileFormat.Png, captureFilename);
                capturePathCallbackAction(file.Path);
            }
        }

        #region Event Handlers

        void OnOrientationSensorOrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            // Only update orientation if the device is not parallel to the ground
            if (args.Orientation != SimpleOrientation.Faceup && args.Orientation != SimpleOrientation.Facedown)
            {
                deviceOrientation = args.Orientation;
            }
        }

        async void OnDisplayInformationOrientationChanged(DisplayInformation sender, object args)
        {
            displayOrientation = sender.CurrentOrientation;

            if (isPreviewing)
            {
                await SetPreviewRotationAsync();
            }
        }

        #endregion

        #region Camera

        async Task InitializeCameraAsync()
        {
            if (mediaCapture == null)
            {
                DeviceInformation cameraDevice = null;
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                if (cameraOptions == CameraOptions.Rear)
                {
                    cameraDevice = devices.FirstOrDefault(c => c.EnclosureLocation != null && c.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);
                }
                else
                {
                    cameraDevice = devices.FirstOrDefault(c => c.EnclosureLocation != null && c.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                }

                if (cameraDevice == null)
                {
                    if (devices.Count> 0)
                        cameraDevice = devices.FirstOrDefault();

                    Debug.WriteLine("No camera found");
                    return;
                }

                mediaCapture = new MediaCapture();

                try
                {
                    await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                    {
                        VideoDeviceId = cameraDevice.Id,
                        AudioDeviceId = string.Empty,
                        StreamingCaptureMode = StreamingCaptureMode.Video
                    });
                    isInitialized = true;
                }

                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("Camera access denied");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception initializing MediaCapture - {0}: {1}", cameraDevice.Id, ex.ToString());
                }

                if (isInitialized)
                {
                    if (cameraDevice.EnclosureLocation == null || cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
                    {
                        await SetMaxResolution();
                        externalCamera = true;
                    }
                    else
                    {
                        // Camera is on device
                        externalCamera = false;

                        // Mirror preview if camera is on front panel
                        mirroringPreview = (cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                    }
                    await StartPreviewAsync();
                }
            }

            // TODO look at using an enum for specific resolution
            async Task SetMaxResolution()
            {
                var res = this.mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
                int maxResolution = 0;
                int indexMaxResolution = 0;
                //string s = "";

                if (res.Count >= 1)
                {
                    for (int i = 0; i < res.Count; i++)
                    {
                        VideoEncodingProperties vp = (VideoEncodingProperties)res[i];
                        var frameRate = (vp.FrameRate.Numerator / vp.FrameRate.Denominator);

                        //s = i + ") " + vp.Subtype + ", ";
                        //s = s + vp.Width;
                        //s = s + " x " + vp.Height;
                        //s = s + " , Frame/s: " + frameRate;

                        if (vp.Width > maxResolution && vp.Subtype.Equals("YUY2"))
                        {
                            indexMaxResolution = i;
                            maxResolution = (int)vp.Width;
                        }

                        //Debug.WriteLine(s);
                        //s = "";
                    }

                    //setting resolution
                    await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, res[indexMaxResolution]);
                }
            }
        }

        public async Task CaptureFrame()
        {
            if (!isPreviewing)
                return;

            var frame = await mediaCapture.GetPreviewFrameAsync();
            var bitmap = frame.SoftwareBitmap;

        }

        async Task StartPreviewAsync()
        {
            if (!isInitialized || isPreviewing)
                return;

            // Prevent the device from sleeping while the preview is running
            displayRequest.RequestActive();

            // Setup preview source in UI and mirror if required
            captureElement.Source = mediaCapture;
            captureElement.FlowDirection = mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Start preview
            await mediaCapture.StartPreviewAsync();

            isPreviewing = true;

            if (isPreviewing)
            {
                await SetPreviewRotationAsync();
            }
        }

        async Task StopPreviewAsync()
        {
            if (!isPreviewing)
                return;

            isPreviewing = false;
            await mediaCapture.StopPreviewAsync();

            // Use dispatcher because sometimes this method is called from non-UI threads
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Allow device screen to sleep now preview is stopped
                displayRequest.RequestRelease();
            });
        }

        async Task SetPreviewRotationAsync()
        {
            // Only update the orientation if the camera is mounted on the device
            if (externalCamera)
            {
                return;
            }

            // Derive the preview rotation
            int rotation = ConvertDisplayOrientationToDegrees(displayOrientation);

            // Invert if mirroring
            if (mirroringPreview)
            {
                rotation = (360 - rotation) % 360;
            }

            // Add rotation metadata to preview stream
            var props = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotation);
            await mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        async Task CleanupCameraAsync()
        {
            if (isInitialized)
            {
                if (isPreviewing)
                {
                    await StopPreviewAsync();
                }
                isInitialized = false;
            }
            if (captureElement != null)
            {
                captureElement.Source = null;
            }
            if (mediaCapture != null)
            {
                mediaCapture.Dispose();
                mediaCapture = null;
            }
        }

        #endregion

        #region Helpers

        async Task SetupUIAsync()
        {
            // Lock page to landscape to prevent the capture element from rotating
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            // Hide status bar
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }

            displayOrientation = displayInformation.CurrentOrientation;
            if (orientationSensor != null)
            {
                deviceOrientation = orientationSensor.GetCurrentOrientation();
            }

            RegisterEventHandlers();
        }

        async Task CleanupUIAsync()
        {
            UnregisterEventHandlers();

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
            }

            // Revert orientation preferences
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
        }

        void RegisterEventHandlers()
        {
            if (orientationSensor != null)
            {
                orientationSensor.OrientationChanged += OnOrientationSensorOrientationChanged;
            }

            displayInformation.OrientationChanged += OnDisplayInformationOrientationChanged;
        }

        void UnregisterEventHandlers()
        {
            if (orientationSensor != null)
            {
                orientationSensor.OrientationChanged -= OnOrientationSensorOrientationChanged;
            }

            displayInformation.OrientationChanged -= OnDisplayInformationOrientationChanged;
        }

        static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        #endregion

        #region Lifecycle

        async void OnAppSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await CleanupCameraAsync();
            await CleanupUIAsync();
            deferral.Complete();
        }

        async void OnAppResuming(object sender, object o)
        {
            await SetupUIAsync();
            await InitializeCameraAsync();
        }

        #endregion
    }
}
