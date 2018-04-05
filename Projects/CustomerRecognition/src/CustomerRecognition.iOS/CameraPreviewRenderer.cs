using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CustomerRecognition.Forms;
using CustomerRecognition.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]
namespace CustomerRecognition.iOS
{
    public class CameraPreviewRenderer : ViewRenderer<CameraPreview, UICameraPreview>
    {
        CameraPreview element;
        UICameraPreview uiCameraPreview;
        Action<string> capturePathCallbackAction;
        string captureFilename;

        protected override void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                uiCameraPreview = new UICameraPreview(e.NewElement.CameraOption);
                SetNativeControl(uiCameraPreview);
            }
            if (e.OldElement != null)
            {
                // Unsubscribe
                capturePathCallbackAction = null;
                element.Capture = null;
                element.StartCamera = null;
                element.StopCamera = null;
                captureFilename = "temp";
            }
            if (e.NewElement != null)
            {
                // Subscribe
                element = e.NewElement;
                capturePathCallbackAction = element.CapturePathCallback;
                element.Capture = new Command(async () => await CaptureToFile());
                element.StartCamera = new Command(() => uiCameraPreview.StartPreviewing());
                element.StopCamera = new Command(() => uiCameraPreview.StopPreviewing());
                captureFilename = element.Filename;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Camera")
            {
                var view = (CameraPreview)sender;
                uiCameraPreview.UpdateCameraOption(view.CameraOption);
            }
        }

        async Task CaptureToFile()
        {
            if (capturePathCallbackAction == null)
                return;

            var result = await uiCameraPreview.Capture(captureFilename);
            capturePathCallbackAction(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control.CaptureSession.Dispose();
                Control.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
