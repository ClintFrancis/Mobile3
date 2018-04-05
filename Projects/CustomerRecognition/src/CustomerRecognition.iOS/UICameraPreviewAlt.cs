using System;
using CustomerRecognition.Forms;
using UIKit;
using Foundation;
using AVFoundation;
using System.Threading.Tasks;

namespace CustomerRecognition.iOS
{
    public class UICameraPreviewAlt : UIView
    {
        CameraOptions cameraOptions;
        public bool IsPreviewing { get; set; }

        bool flashOn = false;

        public AVCaptureSession CaptureSession;
        AVCaptureDeviceInput captureDeviceInput;
        AVCaptureStillImageOutput stillImageOutput;
        AVCaptureVideoPreviewLayer videoPreviewLayer;

        public UICameraPreviewAlt(CameraOptions options)
        {
            cameraOptions = options;
            IsPreviewing = false;

            Initialise();
        }

        public async void Initialise()
        {
            await AuthorizeCameraUse();
            SetupLiveCameraStream();
        }

        public async Task<string> Capture(string filename)
        {
            var videoConnection = stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
            var sampleBuffer = await stillImageOutput.CaptureStillImageTaskAsync(videoConnection);

            var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);
            var image = UIImage.LoadFromData(jpegImageAsNsData);//.ResizeImageWithAspectRatio(.6f);

            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string jpgFilename = System.IO.Path.Combine(documentsDirectory, filename + ".jpg");

            NSError error;
            image.AsJPEG(.9f).Save(jpgFilename, NSDataWritingOptions.FileProtectionNone, out error);
            if (error != null)
            {
                throw new Exception(error.ToString());
            }

            return jpgFilename;
        }

        public void SwitchCamera()
        {
            var devicePosition = captureDeviceInput.Device.Position;
            if (devicePosition == AVCaptureDevicePosition.Front)
            {
                devicePosition = AVCaptureDevicePosition.Back;
            }
            else
            {
                devicePosition = AVCaptureDevicePosition.Front;
            }

            var device = GetCameraForOrientation(devicePosition);
            ConfigureCameraForDevice(device);

            CaptureSession.BeginConfiguration();
            CaptureSession.RemoveInput(captureDeviceInput);
            captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
            CaptureSession.AddInput(captureDeviceInput);
            CaptureSession.CommitConfiguration();
        }

        public AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

            foreach (var device in devices)
            {
                if (device.Position == orientation)
                {
                    return device;
                }
            }

            return null;
        }

        async Task AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
            }
        }

        public void SetupLiveCameraStream()
        {
            CaptureSession = new AVCaptureSession();

            var viewLayer = this.Layer;
            videoPreviewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = this.Frame
            };
            this.Layer.AddSublayer(videoPreviewLayer);

            var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);
            ConfigureCameraForDevice(captureDevice);
            captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
            CaptureSession.AddInput(captureDeviceInput);

            var dictionary = new NSMutableDictionary();
            dictionary[AVVideo.CodecKey] = new NSNumber((int)AVVideoCodec.JPEG);
            stillImageOutput = new AVCaptureStillImageOutput()
            {
                OutputSettings = new NSDictionary()
            };

            CaptureSession.AddOutput(stillImageOutput);
            CaptureSession.StartRunning();
        }

        void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }
    }
}
