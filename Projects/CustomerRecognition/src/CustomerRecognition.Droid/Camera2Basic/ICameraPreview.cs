using System;
using System.Threading.Tasks;
using Android.Hardware.Camera2;
using Android.OS;
using Camera2Basic.Listeners;
using Java.IO;
using Java.Util.Concurrent;

namespace Camera2Basic
{
    public interface ICameraPreview
    {
        File File { get; }
        CameraState State { get; set; }
        CameraDevice Device { get; set; }
        CameraCaptureSession CaptureSession { get; set; }
        CaptureRequest PreviewRequest { get; set; }
        CaptureRequest.Builder PreviewRequestBuilder { get; set; }
        CameraCaptureListener CaptureCallback { get; }
        HandlerThread BackgroundThread { get; }
        Handler BackgroundHandler { get; }
        Semaphore CameraOpenCloseLock { get; }

        void OpenCamera(int width, int height);
        void CloseCamera();
        void ConfigureTransform(int viewWidth, int viewHeight);
        void CaptureStillPicture();
        void OnCaptureResult(CameraResult result);
        void RunPrecaptureSequence();
        void LockFocus();
        void UnlockFocus();
        void CreateCameraPreviewSession();
        void SetAutoFlash(CaptureRequest.Builder requestBuilder);
    }
}
