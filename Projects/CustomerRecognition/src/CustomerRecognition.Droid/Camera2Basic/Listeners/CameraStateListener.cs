using Android.App;
using Android.Hardware.Camera2;

namespace Camera2Basic.Listeners
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        private readonly ICameraPreview owner;

        public CameraStateListener(ICameraPreview owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
        }

        public override void OnOpened(CameraDevice camera)
        {
            // This method is called when the camera is opened.  We start camera preview here.
            owner.CameraOpenCloseLock.Release();
            owner.Device = camera;
            owner.CreateCameraPreviewSession();
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            owner.CameraOpenCloseLock.Release();
            camera.Close();
            owner.Device = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            owner.CameraOpenCloseLock.Release();
            camera.Close();
            owner.Device = null;
            if (owner == null)
                return;

            //Activity activity = owner.Activity;
            //if (activity != null)
            //{
            //    activity.Finish();
            //}
        }
    }
}