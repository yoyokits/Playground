namespace XamarinFormsXamlCameraTest.Droid
{
    using Android;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.Hardware.Camera2;
    using Android.Hardware.Camera2.Params;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using AndroidX.Core.Content;
    using AndroidX.Fragment.App;
    using Java.Lang;
    using Java.Util.Concurrent;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xamarin.Forms.Platform.Android;
    using XamarinFormsXamlCameraTest.Controls;

    /// <summary>
    /// Defines the <see cref="CameraFragment" />.
    /// </summary>
    public class CameraFragment : Fragment, TextureView.ISurfaceTextureListener
    {
        #region Fields

        private bool Available;

        private Handler backgroundHandler = null;

        private HandlerThread backgroundThread;

        private bool busy;

        private string cameraId;

        private bool cameraPermissionsGranted;

        private CameraTemplate cameraTemplate;

        private LensFacing cameraType;

        private Java.Util.Concurrent.Semaphore captureSessionOpenCloseLock = new Java.Util.Concurrent.Semaphore(1);

        private CameraDevice device;

        private TaskCompletionSource<CameraDevice> initTaskSource;

        private CameraManager manager;

        private TaskCompletionSource<bool> permissionsRequested;

        private Android.Util.Size previewSize;

        private bool repeatingIsRunning;

        private int sensorOrientation;

        private CameraCaptureSession session;

        private CaptureRequest.Builder sessionBuilder;

        private AutoFitTextureView texture;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraFragment"/> class.
        /// </summary>
        public CameraFragment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraFragment"/> class.
        /// </summary>
        /// <param name="javaReference">The javaReference<see cref="IntPtr"/>.</param>
        /// <param name="transfer">The transfer<see cref="JniHandleOwnership"/>.</param>
        public CameraFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Element.
        /// </summary>
        public CameraPreview Element { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsBusy.
        /// </summary>
        private bool IsBusy
        {
            get => device == null || busy;
            set
            {
                busy = value;
            }
        }

        /// <summary>
        /// Gets the Manager.
        /// </summary>
        private CameraManager Manager => manager ??= (CameraManager)Context.GetSystemService(Context.CameraService);

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnCreateView.
        /// </summary>
        /// <param name="inflater">The inflater<see cref="LayoutInflater"/>.</param>
        /// <param name="container">The container<see cref="ViewGroup"/>.</param>
        /// <param name="savedInstanceState">The savedInstanceState<see cref="Bundle"/>.</param>
        /// <returns>The <see cref="Android.Views.View"/>.</returns>
        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) => inflater.Inflate(Resource.Layout.CameraFragment, null);

        /// <summary>
        /// The OnPause.
        /// </summary>
        public override void OnPause()
        {
            CloseSession();
            StopBackgroundThread();
            base.OnPause();
        }

        /// <summary>
        /// The OnRequestPermissionsResult.
        /// </summary>
        /// <param name="requestCode">The requestCode<see cref="int"/>.</param>
        /// <param name="permissions">The permissions<see cref="string[]"/>.</param>
        /// <param name="grantResults">The grantResults<see cref="Permission[]"/>.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode != 1)
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                return;
            }

            for (int i = 0; i < permissions.Length; i++)
            {
                if (permissions[i] == Manifest.Permission.Camera)
                {
                    cameraPermissionsGranted = grantResults[i] == Permission.Granted;
                    if (!cameraPermissionsGranted)
                    {
                        Console.WriteLine("No permission to use the camera.");
                    }
                }
            }
            permissionsRequested?.TrySetResult(true);
        }

        /// <summary>
        /// The OnResume.
        /// </summary>
        public override async void OnResume()
        {
            base.OnResume();

            StartBackgroundThread();
            if (texture is null)
            {
                return;
            }
            if (texture.IsAvailable)
            {
                View?.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
                cameraTemplate = CameraTemplate.Preview;
                await RetrieveCameraDevice(force: true);
            }
            else
            {
                texture.SurfaceTextureListener = this;
            }
        }

        /// <summary>
        /// The OnViewCreated.
        /// </summary>
        /// <param name="view">The view<see cref="Android.Views.View"/>.</param>
        /// <param name="savedInstanceState">The savedInstanceState<see cref="Bundle"/>.</param>
        public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState) => texture = view.FindViewById<AutoFitTextureView>(Resource.Id.cameratexture);

        /// <summary>
        /// The RetrieveCameraDevice.
        /// </summary>
        /// <param name="force">The force<see cref="bool"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task RetrieveCameraDevice(bool force = false)
        {
            if (Context == null || (!force && initTaskSource != null))
            {
                return;
            }

            if (device != null)
            {
                CloseDevice();
            }

            await RequestCameraPermissions();
            if (!cameraPermissionsGranted)
            {
                return;
            }

            if (!captureSessionOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
            {
                throw new RuntimeException("Timeout waiting to lock camera opening.");
            }

            IsBusy = true;
            cameraId = GetCameraId();

            if (string.IsNullOrEmpty(cameraId))
            {
                IsBusy = false;
                captureSessionOpenCloseLock.Release();
                Console.WriteLine("No camera found");
            }
            else
            {
                try
                {
                    CameraCharacteristics characteristics = Manager.GetCameraCharacteristics(cameraId);
                    StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

                    previewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))),
                        texture.Width, texture.Height, GetMaxSize(map.GetOutputSizes((int)ImageFormatType.Jpeg)));
                    sensorOrientation = (int)characteristics.Get(CameraCharacteristics.SensorOrientation);
                    cameraType = (LensFacing)(int)characteristics.Get(CameraCharacteristics.LensFacing);

                    if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
                    {
                        texture.SetAspectRatio(previewSize.Width, previewSize.Height);
                    }
                    else
                    {
                        texture.SetAspectRatio(previewSize.Height, previewSize.Width);
                    }

                    initTaskSource = new TaskCompletionSource<CameraDevice>();
                    Manager.OpenCamera(cameraId, new CameraStateListener
                    {
                        OnOpenedAction = device => initTaskSource?.TrySetResult(device),
                        OnDisconnectedAction = device =>
                        {
                            initTaskSource?.TrySetResult(null);
                            CloseDevice(device);
                        },
                        OnErrorAction = (device, error) =>
                        {
                            initTaskSource?.TrySetResult(device);
                            Console.WriteLine($"Camera device error: {error}");
                            CloseDevice(device);
                        },
                        OnClosedAction = device =>
                        {
                            initTaskSource?.TrySetResult(null);
                            CloseDevice(device);
                        }
                    }, backgroundHandler);

                    captureSessionOpenCloseLock.Release();
                    device = await initTaskSource.Task;
                    initTaskSource = null;
                    if (device != null)
                    {
                        await PrepareSession();
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    Console.WriteLine("Failed to open camera.", ex);
                    Available = false;
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// The UpdateRepeatingRequest.
        /// </summary>
        public void UpdateRepeatingRequest()
        {
            if (session == null || sessionBuilder == null)
            {
                return;
            }

            IsBusy = true;
            try
            {
                if (repeatingIsRunning)
                {
                    session.StopRepeating();
                }

                sessionBuilder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);
                sessionBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
                session.SetRepeatingRequest(sessionBuilder.Build(), listener: null, backgroundHandler);
                repeatingIsRunning = true;
            }
            catch (Java.Lang.Exception error)
            {
                Console.WriteLine("Update preview exception.", error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// The Dispose.
        /// </summary>
        /// <param name="disposing">The disposing<see cref="bool"/>.</param>
        protected override void Dispose(bool disposing)
        {
            CloseDevice();
            base.Dispose(disposing);
        }

        /// <summary>
        /// The ChooseOptimalSize.
        /// </summary>
        /// <param name="choices">The choices<see cref="Android.Util.Size[]"/>.</param>
        /// <param name="width">The width<see cref="int"/>.</param>
        /// <param name="height">The height<see cref="int"/>.</param>
        /// <param name="aspectRatio">The aspectRatio<see cref="Android.Util.Size"/>.</param>
        /// <returns>The <see cref="Android.Util.Size"/>.</returns>
        private Android.Util.Size ChooseOptimalSize(Android.Util.Size[] choices, int width, int height, Android.Util.Size aspectRatio)
        {
            List<Android.Util.Size> bigEnough = new List<Android.Util.Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;

            foreach (Android.Util.Size option in choices)
            {
                if (option.Height == option.Width * h / w && option.Width >= width && option.Height >= height)
                {
                    bigEnough.Add(option);
                }
            }

            if (bigEnough.Count > 0)
            {
                int minArea = bigEnough.Min(s => s.Width * s.Height);
                return bigEnough.First(s => s.Width * s.Height == minArea);
            }
            else
            {
                Console.WriteLine("Couldn't find any suitable preview size.");
                return choices[0];
            }
        }

        /// <summary>
        /// The CloseDevice.
        /// </summary>
        private void CloseDevice()
        {
            CloseSession();

            try
            {
                if (sessionBuilder != null)
                {
                    sessionBuilder.Dispose();
                    sessionBuilder = null;
                }
                if (device != null)
                {
                    device.Close();
                    device = null;
                }
            }
            catch (Java.Lang.Exception error)
            {
                Console.WriteLine("Error closing device.", error);
            }
        }

        /// <summary>
        /// The CloseDevice.
        /// </summary>
        /// <param name="inputDevice">The inputDevice<see cref="CameraDevice"/>.</param>
        private void CloseDevice(CameraDevice inputDevice)
        {
            if (inputDevice == device)
            {
                CloseDevice();
            }
        }

        /// <summary>
        /// The CloseSession.
        /// </summary>
        private void CloseSession()
        {
            repeatingIsRunning = false;
            if (session == null)
            {
                return;
            }

            try
            {
                session.StopRepeating();
                session.AbortCaptures();
                session.Close();
                session.Dispose();
                session = null;
            }
            catch (CameraAccessException ex)
            {
                Console.WriteLine("Camera access error.", ex);
            }
            catch (Java.Lang.Exception ex)
            {
                Console.WriteLine("Error closing device.", ex);
            }
        }

        /// <summary>
        /// The ConfigureTransform.
        /// </summary>
        /// <param name="viewWidth">The viewWidth<see cref="int"/>.</param>
        /// <param name="viewHeight">The viewHeight<see cref="int"/>.</param>
        private void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (texture == null || previewSize == null || previewSize.Width == 0 || previewSize.Height == 0)
            {
                return;
            }

            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, previewSize.Height, previewSize.Width);
            var centerX = viewRect.CenterX();
            var centerY = viewRect.CenterY();
            bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
            matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
            matrix.PostRotate(GetCaptureOrientation(), centerX, centerY);
            texture.SetTransform(matrix);
        }

        /// <summary>
        /// The GetCameraId.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetCameraId()
        {
            string[] cameraIdList = Manager.GetCameraIdList();
            if (cameraIdList.Length == 0)
            {
                return null;
            }

            string FilterCameraByLens(LensFacing lensFacing)
            {
                foreach (string id in cameraIdList)
                {
                    CameraCharacteristics characteristics = Manager.GetCameraCharacteristics(id);
                    if (lensFacing == (LensFacing)(int)characteristics.Get(CameraCharacteristics.LensFacing))
                    {
                        return id;
                    }
                }
                return null;
            }

            return (Element.Camera == CameraOptions.Front) ? FilterCameraByLens(LensFacing.Front) : FilterCameraByLens(LensFacing.Back);
        }

        /// <summary>
        /// The GetCaptureOrientation.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetCaptureOrientation()
        {
            int frontOffset = cameraType == LensFacing.Front ? 90 : -90;
            return (360 + sensorOrientation - GetDisplayRotationDegrees() + frontOffset) % 360;
        }

        /// <summary>
        /// The GetDisplayRotation.
        /// </summary>
        /// <returns>The <see cref="SurfaceOrientation"/>.</returns>
        private SurfaceOrientation GetDisplayRotation() => Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay.Rotation;

        /// <summary>
        /// The GetDisplayRotationDegrees.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetDisplayRotationDegrees() =>
            GetDisplayRotation() switch
            {
                SurfaceOrientation.Rotation90 => 90,
                SurfaceOrientation.Rotation180 => 180,
                SurfaceOrientation.Rotation270 => 270,
                _ => 0
            };

        /// <summary>
        /// The GetMaxSize.
        /// </summary>
        /// <param name="imageSizes">The imageSizes<see cref="Android.Util.Size[]"/>.</param>
        /// <returns>The <see cref="Android.Util.Size"/>.</returns>
        private Android.Util.Size GetMaxSize(Android.Util.Size[] imageSizes)
        {
            Android.Util.Size maxSize = null;
            long maxPixels = 0;
            for (int i = 0; i < imageSizes.Length; i++)
            {
                long currentPixels = imageSizes[i].Width * imageSizes[i].Height;
                if (currentPixels > maxPixels)
                {
                    maxSize = imageSizes[i];
                    maxPixels = currentPixels;
                }
            }
            return maxSize;
        }

        /// <summary>
        /// The PrepareSession.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task PrepareSession()
        {
            IsBusy = true;
            try
            {
                CloseSession();
                sessionBuilder = device.CreateCaptureRequest(cameraTemplate);

                List<Surface> surfaces = new List<Surface>();
                if (texture.IsAvailable && previewSize != null)
                {
                    var texture = this.texture.SurfaceTexture;
                    texture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
                    Surface previewSurface = new Surface(texture);
                    surfaces.Add(previewSurface);
                    sessionBuilder.AddTarget(previewSurface);
                }

                TaskCompletionSource<CameraCaptureSession> tcs = new TaskCompletionSource<CameraCaptureSession>();
                device.CreateCaptureSession(surfaces, new CameraCaptureStateListener
                {
                    OnConfigureFailedAction = captureSession =>
                    {
                        tcs.SetResult(null);
                        Console.WriteLine("Failed to create capture session.");
                    },
                    OnConfiguredAction = captureSession => tcs.SetResult(captureSession)
                }, null);

                session = await tcs.Task;
                if (session != null)
                {
                    UpdateRepeatingRequest();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Available = false;
                Console.WriteLine("Capture error.", ex);
            }
            finally
            {
                Available = session != null;
                IsBusy = false;
            }
        }

        /// <summary>
        /// The RequestCameraPermissions.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task RequestCameraPermissions()
        {
            if (permissionsRequested != null)
            {
                await permissionsRequested.Task;
            }

            List<string> permissionsToRequest = new List<string>();
            cameraPermissionsGranted = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera) == Permission.Granted;
            if (!cameraPermissionsGranted)
            {
                permissionsToRequest.Add(Manifest.Permission.Camera);
            }

            if (permissionsToRequest.Count > 0)
            {
                permissionsRequested = new TaskCompletionSource<bool>();
                RequestPermissions(permissionsToRequest.ToArray(), requestCode: 1);
                await permissionsRequested.Task;
                permissionsRequested = null;
            }
        }

        /// <summary>
        /// The StartBackgroundThread.
        /// </summary>
        private void StartBackgroundThread()
        {
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.Start();
            backgroundHandler = new Handler(backgroundThread.Looper);
        }

        /// <summary>
        /// The StopBackgroundThread.
        /// </summary>
        private void StopBackgroundThread()
        {
            if (backgroundThread == null)
            {
                return;
            }

            backgroundThread.QuitSafely();
            try
            {
                backgroundThread.Join();
                backgroundThread = null;
                backgroundHandler = null;
            }
            catch (InterruptedException ex)
            {
                Console.WriteLine("Error stopping background thread.", ex);
            }
        }

        /// <summary>
        /// The OnSurfaceTextureAvailable.
        /// </summary>
        /// <param name="surface">The surface<see cref="SurfaceTexture"/>.</param>
        /// <param name="width">The width<see cref="int"/>.</param>
        /// <param name="height">The height<see cref="int"/>.</param>
        async void TextureView.ISurfaceTextureListener.OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            View?.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            cameraTemplate = CameraTemplate.Preview;
            await RetrieveCameraDevice();
        }

        /// <summary>
        /// The OnSurfaceTextureDestroyed.
        /// </summary>
        /// <param name="surface">The surface<see cref="SurfaceTexture"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        bool TextureView.ISurfaceTextureListener.OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            CloseDevice();
            return true;
        }

        /// <summary>
        /// The OnSurfaceTextureSizeChanged.
        /// </summary>
        /// <param name="surface">The surface<see cref="SurfaceTexture"/>.</param>
        /// <param name="width">The width<see cref="int"/>.</param>
        /// <param name="height">The height<see cref="int"/>.</param>
        void TextureView.ISurfaceTextureListener.OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) => ConfigureTransform(width, height);

        /// <summary>
        /// The OnSurfaceTextureUpdated.
        /// </summary>
        /// <param name="surface">The surface<see cref="SurfaceTexture"/>.</param>
        void TextureView.ISurfaceTextureListener.OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        #endregion Methods
    }
}