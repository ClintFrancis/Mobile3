using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Camera2Basic;
using Camera2Basic.Listeners;
using CustomerRecognition.Forms;
using Java.IO;
using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;

namespace CustomerRecognition.Droid
{
    public class ImageCaptureEventArgs : EventArgs
    {
        public string Filepath { get; private set; }

        public ImageCaptureEventArgs(string filePath)
        {
            this.Filepath = filePath;
        }
    }

    public class CameraPreview : ViewGroup, Camera2Basic.ICameraPreview
    {
        public event EventHandler<ImageCaptureEventArgs> ImageCaptured;

        public CameraPreview(Context context, CameraOptions option) : base(context)
        {
            CameraOption = option;

            // Camera Setup
            File = new Java.IO.File(Context.GetExternalFilesDir(null), "pic.jpg");
            CaptureCallback = new CameraCaptureListener(this);
            mOnImageAvailableListener = new ImageAvailableListener(this, File);
            mStateCallback = new CameraStateListener(this);
            mSurfaceTextureListener = new Camera2BasicSurfaceTextureListener(this);

            mTextureView = new Camera2Basic.AutoFitTextureView(context);
            AddView(mTextureView);

            // fill ORIENTATIONS list
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
        }

        // bool changed, int left, int top, int right, int bottom
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            mTextureView.Measure(msw, msh);
            mTextureView.Layout(0, 0, r - l, b - t);
        }

        public void UpdateCameraOption(CameraOptions option)
        {
            if (CameraOption == option)
                return;

            CameraOption = option;

            // TODO Implement Camera switching on Android
            Log.WriteLine(LogPriority.Warn, "CameraPreview", "Switching Cameras is not yet supported on Android");
            
        }

        // Max preview width that is guaranteed by Camera2 API
        private static readonly int MAX_PREVIEW_WIDTH = 1920;

        // Max preview height that is guaranteed by Camera2 API
        private static readonly int MAX_PREVIEW_HEIGHT = 1080;

        // The size of the camera preview
        private Size mPreviewSize;

        // ID of the current {@link CameraDevice}.
        private string mCameraId;

        // An AutoFitTextureView for camera preview
        private Camera2Basic.AutoFitTextureView mTextureView;

        // Whether the current camera device supports Flash or not.
        private bool mFlashSupported;

        public Camera2Basic.CameraState State { get; set; }

        // CameraDevice.StateListener is called when a CameraDevice changes its state
        private CameraStateListener mStateCallback;

        // TextureView.ISurfaceTextureListener handles several lifecycle events on a TextureView
        private Camera2BasicSurfaceTextureListener mSurfaceTextureListener;

        // This a callback object for the {@link ImageReader}. "onImageAvailable" will be called when a
        // still image is ready to be saved.
        private ImageAvailableListener mOnImageAvailableListener;

        // An {@link ImageReader} that handles still image capture.
        private ImageReader mImageReader;

        // Orientation of the camera sensor
        private int mSensorOrientation;

        // An additional thread for running tasks that shouldn't block the UI.
        private HandlerThread mBackgroundThread;

        public CameraOptions CameraOption { get; private set; }

        // This is the output file for our picture.
        public Java.IO.File File { get; private set; }

        public CameraDevice Device { get; set; }
        public CameraCaptureSession CaptureSession { get; set; }
        public CaptureRequest PreviewRequest { get; set; }
        public CaptureRequest.Builder PreviewRequestBuilder { get; set; }
        private CaptureRequest.Builder stillCaptureBuilder;

        // A {@link CameraCaptureSession.CaptureCallback} that handles events related to JPEG capture.
        public CameraCaptureListener CaptureCallback { get; set; }

        public HandlerThread BackgroundThread { get; set; }

        public Handler BackgroundHandler { get; set; }

        public Semaphore CameraOpenCloseLock { get; } = new Semaphore(1);

        // Starts a background thread and its {@link Handler}.
        private void StartBackgroundThread()
        {
            mBackgroundThread = new HandlerThread("CameraBackground");
            mBackgroundThread.Start();
            BackgroundHandler = new Handler(mBackgroundThread.Looper);
        }

        // Stops the background thread and its {@link Handler}.
        private void StopBackgroundThread()
        {
            if (mBackgroundThread == null)
                return;

            mBackgroundThread.QuitSafely();
            try
            {
                mBackgroundThread.Join();
                mBackgroundThread = null;
                BackgroundHandler = null;
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }

        // Sets up member variables related to camera.
        private void SetUpCameraOutputs(int width, int height)
        {
            var cameraManager = (CameraManager)Context.GetSystemService(Context.CameraService);
            try
            {
                for (var i = 0; i < cameraManager.GetCameraIdList().Length; i++)
                {
                    var cameraId = cameraManager.GetCameraIdList()[i];
                    CameraCharacteristics characteristics = cameraManager.GetCameraCharacteristics(cameraId);

                    // We don't use a front facing camera in this sample.
                    var facing = (Integer)characteristics.Get(CameraCharacteristics.LensFacing);
                    if (facing != null && facing == (Integer.ValueOf((int)LensFacing.Front)))
                    {
                        continue;
                    }

                    var map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                    if (map == null)
                    {
                        continue;
                    }

                    // For still image captures, we use the largest available size.
                    Size largest = (Size)Collections.Max(Arrays.AsList(map.GetOutputSizes((int)ImageFormatType.Jpeg)),
                                                         new Camera2Basic.CompareSizesByArea());
                    mImageReader = ImageReader.NewInstance(largest.Width, largest.Height, ImageFormatType.Jpeg, 2);
                    mImageReader.SetOnImageAvailableListener(mOnImageAvailableListener, BackgroundHandler);

                    var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

                    // Find out if we need to swap dimension to get the preview size relative to sensor
                    // coordinate.
                    var displayRotation = windowManager.DefaultDisplay.Rotation;

                    //noinspection ConstantConditions
                    mSensorOrientation = (int)characteristics.Get(CameraCharacteristics.SensorOrientation);
                    bool swappedDimensions = false;
                    switch (displayRotation)
                    {
                        case SurfaceOrientation.Rotation0:
                        case SurfaceOrientation.Rotation180:
                            if (mSensorOrientation == 90 || mSensorOrientation == 270)
                            {
                                swappedDimensions = true;
                            }
                            break;
                        case SurfaceOrientation.Rotation90:
                        case SurfaceOrientation.Rotation270:
                            if (mSensorOrientation == 0 || mSensorOrientation == 180)
                            {
                                swappedDimensions = true;
                            }
                            break;
                        default:
                            Log.Error("CameraPreview", "Display rotation is invalid: " + displayRotation);
                            break;
                    }

                    Point displaySize = new Point();

                    windowManager.DefaultDisplay.GetSize(displaySize);

                    var rotatedPreviewWidth = width;
                    var rotatedPreviewHeight = height;
                    var maxPreviewWidth = displaySize.X;
                    var maxPreviewHeight = displaySize.Y;

                    if (swappedDimensions)
                    {
                        rotatedPreviewWidth = height;
                        rotatedPreviewHeight = width;
                        maxPreviewWidth = displaySize.Y;
                        maxPreviewHeight = displaySize.X;
                    }

                    if (maxPreviewWidth > MAX_PREVIEW_WIDTH)
                    {
                        maxPreviewWidth = MAX_PREVIEW_WIDTH;
                    }

                    if (maxPreviewHeight > MAX_PREVIEW_HEIGHT)
                    {
                        maxPreviewHeight = MAX_PREVIEW_HEIGHT;
                    }

                    // Danger, W.R.! Attempting to use too large a preview size could  exceed the camera
                    // bus' bandwidth limitation, resulting in gorgeous previews but the storage of
                    // garbage capture data.
                    mPreviewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))),
                        rotatedPreviewWidth, rotatedPreviewHeight, maxPreviewWidth,
                        maxPreviewHeight, largest);

                    // We fit the aspect ratio of TextureView to the size of preview we picked.
                    var orientation = Resources.Configuration.Orientation;
                    if (orientation == Android.Content.Res.Orientation.Landscape)
                    {
                        mTextureView.SetAspectRatio(mPreviewSize.Width, mPreviewSize.Height);
                    }
                    else
                    {
                        mTextureView.SetAspectRatio(mPreviewSize.Height, mPreviewSize.Width);
                    }

                    // Check if the flash is supported.
                    var available = characteristics.Get(CameraCharacteristics.FlashInfoAvailable).JavaCast<Java.Lang.Boolean>();
                    if (available == null)
                    {
                        mFlashSupported = false;
                    }
                    else
                    {
                        mFlashSupported = (bool)available;
                    }

                    mCameraId = cameraId;
                    return;
                }
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (NullPointerException e)
            {
                // Currently an NPE is thrown when the Camera2API is used but not supported on the
                // device this code runs.
                //ErrorDialog.NewInstance(GetString(Resource.String.camera_error)).Show(ChildFragmentManager, FRAGMENT_DIALOG);
                Log.Error("CameraPreviewAlt", "SetUpCameraOutputs: " + e.ToString());
            }
        }

        private static Size ChooseOptimalSize(Size[] choices, int textureViewWidth,
            int textureViewHeight, int maxWidth, int maxHeight, Size aspectRatio)
        {
            // Collect the supported resolutions that are at least as big as the preview Surface
            var bigEnough = new List<Size>();
            // Collect the supported resolutions that are smaller than the preview Surface
            var notBigEnough = new List<Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;

            for (var i = 0; i < choices.Length; i++)
            {
                Size option = choices[i];
                if ((option.Width <= maxWidth) && (option.Height <= maxHeight) &&
                       option.Height == option.Width * h / w)
                {
                    if (option.Width >= textureViewWidth &&
                        option.Height >= textureViewHeight)
                    {
                        bigEnough.Add(option);
                    }
                    else
                    {
                        notBigEnough.Add(option);
                    }
                }
            }

            // Pick the smallest of those big enough. If there is no one big enough, pick the largest of those not big enough.
            if (bigEnough.Count > 0)
            {
                return (Size)Collections.Min(bigEnough, new Camera2Basic.CompareSizesByArea());
            }
            else if (notBigEnough.Count > 0)
            {
                return (Size)Collections.Max(notBigEnough, new Camera2Basic.CompareSizesByArea());
            }
            else
            {
                Log.Error("CameraPreview", "Couldn't find any suitable preview size");
                return choices[0];
            }
        }

        public void StartPreviewing()
        {
            Log.Error("CameraPreview", "StartPreviewing");

            StartBackgroundThread();

            // When the screen is turned off and turned back on, the SurfaceTexture is already
            // available, and "onSurfaceTextureAvailable" will not be called. In that case, we can open
            // a camera and start preview from here (otherwise, we wait until the surface is ready in
            // the SurfaceTextureListener).
            if (mTextureView.IsAvailable)
            {
                OpenCamera(mTextureView.Width, mTextureView.Height);
            }
            else
            {
                mTextureView.SurfaceTextureListener = mSurfaceTextureListener;
            }
        }

        public void StopPreviewing()
        {
            Log.Error("CameraPreview", "StopPreviewing");
            CloseCamera();
            StopBackgroundThread(); ;
        }

        // Opens the camera specified by {@link Camera2BasicFragment#mCameraId}.
        public void OpenCamera(int width, int height)
        {
            SetUpCameraOutputs(width, height);
            ConfigureTransform(width, height);

            var manager = (CameraManager)Context.GetSystemService(Context.CameraService);

            try
            {
                if (!CameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                {
                    throw new RuntimeException("Time out waiting to lock camera opening.");
                }
                manager.OpenCamera(mCameraId, mStateCallback, BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.", e);
            }
        }

        // Closes the current {@link CameraDevice}.
        public void CloseCamera()
        {
            try
            {
                CameraOpenCloseLock.Acquire();
                if (null != CaptureSession)
                {
                    CaptureSession.Close();
                    CaptureSession = null;
                }
                if (null != Device)
                {
                    Device.Close();
                    Device = null;
                }
                if (null != mImageReader)
                {
                    mImageReader.Close();
                    mImageReader = null;
                }
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera closing.", e);
            }
            finally
            {
                CameraOpenCloseLock.Release();
            }
        }

        public void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (null == mTextureView || null == mPreviewSize)
                return;

            var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            var rotation = (int)windowManager.DefaultDisplay.Rotation;
            Matrix matrix = new Matrix();
            RectF viewRect = new RectF(0, 0, viewWidth, viewHeight);
            RectF bufferRect = new RectF(0, 0, mPreviewSize.Height, mPreviewSize.Width);
            float centerX = viewRect.CenterX();
            float centerY = viewRect.CenterY();

            if ((int)SurfaceOrientation.Rotation90 == rotation || (int)SurfaceOrientation.Rotation270 == rotation)
            {
                bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                float scale = System.Math.Max((float)viewHeight / mPreviewSize.Height, (float)viewWidth / mPreviewSize.Width);
                matrix.PostScale(scale, scale, centerX, centerY);
                matrix.PostRotate(90 * (rotation - 2), centerX, centerY);
            }
            else if ((int)SurfaceOrientation.Rotation180 == rotation)
            {
                matrix.PostRotate(180, centerX, centerY);
            }
            mTextureView.SetTransform(matrix);
        }

        public void CreateCameraPreviewSession()
        {
            try
            {
                SurfaceTexture texture = mTextureView.SurfaceTexture;
                if (texture == null)
                    throw new IllegalStateException("texture is null");

                // We configure the size of default buffer to be the size of camera preview we want.
                texture.SetDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);

                // This is the output Surface we need to start preview.
                Surface surface = new Surface(texture);

                // We set up a CaptureRequest.Builder with the output Surface.
                PreviewRequestBuilder = Device.CreateCaptureRequest(CameraTemplate.Preview);
                PreviewRequestBuilder.AddTarget(surface);

                // Here, we create a CameraCaptureSession for camera preview.
                List<Surface> surfaces = new List<Surface>();
                surfaces.Add(surface);
                surfaces.Add(mImageReader.Surface);
                Device.CreateCaptureSession(surfaces, new CameraCaptureSessionCallback(this), null);

            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void SetAutoFlash(CaptureRequest.Builder requestBuilder)
        {
            if (mFlashSupported)
                requestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
        }

        private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray();

        // Retrieves the JPEG orientation from the specified screen rotation.
        private int GetOrientation(int rotation)
        {
            // Sensor orientation is 90 for most devices, or 270 for some devices (eg. Nexus 5X)
            // We have to take that into account and rotate JPEG properly.
            // For devices with orientation of 90, we simply return our mapping from ORIENTATIONS.
            // For devices with orientation of 270, we need to rotate the JPEG 180 degrees.
            return (ORIENTATIONS.Get(rotation) + mSensorOrientation + 270) % 360;
        }

        public void Capture(string filename)
        {
            if (State != CameraState.Preview)
                return;

            LockFocus();
        }

        public void CaptureStillPicture()
        {
            try
            {
                if (null == Device)
                    return;

                // This is the CaptureRequest.Builder that we use to take a picture.
                if (stillCaptureBuilder == null)
                    stillCaptureBuilder = Device.CreateCaptureRequest(CameraTemplate.StillCapture);

                stillCaptureBuilder.AddTarget(mImageReader.Surface);

                // Use the same AE and AF modes as the preview.
                stillCaptureBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
                SetAutoFlash(stillCaptureBuilder);

                // Orientation
                var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                int rotation = (int)windowManager.DefaultDisplay.Rotation;
                stillCaptureBuilder.Set(CaptureRequest.JpegOrientation, GetOrientation(rotation));

                CaptureSession.StopRepeating();
                CaptureSession.Capture(stillCaptureBuilder.Build(), new CameraCaptureStillPictureSessionCallback(this), null);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void RunPrecaptureSequence()
        {
            try
            {
                // This is how to tell the camera to trigger.
                PreviewRequestBuilder.Set(CaptureRequest.ControlAePrecaptureTrigger, (int)ControlAEPrecaptureTrigger.Start);
                // Tell #mCaptureCallback to wait for the precapture sequence to be set.
                State = Camera2Basic.CameraState.WaitingPrecapture; ;
                CaptureSession.Capture(PreviewRequestBuilder.Build(), CaptureCallback, BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void OnCaptureResult(CameraResult result)
        {
            UnlockFocus();

            if (result == CameraResult.Completed)
            {
                // Resize and compress the image
                var bmp = BitmapUtils.LoadAndResizeBitmap(this.File.Path, 600, 600); // TODO make dynamic

                var stream = new FileStream(this.File.Path, FileMode.Create);
                bmp.Compress(Bitmap.CompressFormat.Jpeg, 80, stream);
                stream.Close();

                ImageCaptured?.Invoke(this, new ImageCaptureEventArgs(this.File.Path));
            }
        }

        public void LockFocus()
        {
            try
            {
                // This is how to tell the camera to lock focus.

                PreviewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Start);
                // Tell #mCaptureCallback to wait for the lock.
                State = Camera2Basic.CameraState.WaitingLock;
                CaptureSession.Capture(PreviewRequestBuilder.Build(), CaptureCallback, BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void UnlockFocus()
        {
            try
            {
                // Reset the auto-focus trigger
                PreviewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
                SetAutoFlash(PreviewRequestBuilder);
                CaptureSession.Capture(PreviewRequestBuilder.Build(), CaptureCallback, BackgroundHandler);

                // After this, the camera will go back to the normal state of preview.
                State = Camera2Basic.CameraState.Preview;
                CaptureSession.SetRepeatingRequest(PreviewRequest, CaptureCallback, BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}
