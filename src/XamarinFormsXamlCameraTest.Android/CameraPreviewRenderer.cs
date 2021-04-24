using Xamarin.Forms;
using XamarinFormsXamlCameraTest.Controls;
using XamarinFormsXamlCameraTest.Droid;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]

namespace XamarinFormsXamlCameraTest.Droid
{
    using Android.Content;
    using Android.Views;
    using Android.Widget;
    using AndroidX.Fragment.App;
    using System;
    using System.ComponentModel;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.Android;
    using Xamarin.Forms.Platform.Android.FastRenderers;
    using XamarinFormsXamlCameraTest.Controls;

    /// <summary>
    /// Defines the <see cref="CameraPreviewRenderer" />.
    /// </summary>
    public class CameraPreviewRenderer : FrameLayout, IVisualElementRenderer, IViewRenderer
    {
        #region Fields

        private CameraFragment cameraFragment;

        private int? defaultLabelFor;

        private bool disposed;

        private CameraPreview element;

        private FragmentManager fragmentManager;

        private VisualElementRenderer visualElementRenderer;

        private VisualElementTracker visualElementTracker;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraPreviewRenderer"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Context"/>.</param>
        public CameraPreviewRenderer(Context context) : base(context)
        {
            visualElementRenderer = new VisualElementRenderer(this);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Defines the ElementChanged.
        /// </summary>
        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        /// <summary>
        /// Defines the ElementPropertyChanged.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the Element.
        /// </summary>
        private CameraPreview Element
        {
            get => element;
            set
            {
                if (element == value)
                {
                    return;
                }

                var oldElement = element;
                element = value;
                OnElementChanged(new ElementChangedEventArgs<CameraPreview>(oldElement, element));
            }
        }

        /// <summary>
        /// Gets the FragmentManager.
        /// </summary>
        private FragmentManager FragmentManager => fragmentManager ??= Context.GetFragmentManager();

        /// <summary>
        /// Gets the Element.
        /// </summary>
        VisualElement IVisualElementRenderer.Element => Element;

        /// <summary>
        /// Gets the Tracker.
        /// </summary>
        VisualElementTracker IVisualElementRenderer.Tracker => visualElementTracker;

        /// <summary>
        /// Gets the View.
        /// </summary>
        Android.Views.View IVisualElementRenderer.View => this;

        /// <summary>
        /// Gets the ViewGroup.
        /// </summary>
        ViewGroup IVisualElementRenderer.ViewGroup => null;

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Dispose.
        /// </summary>
        /// <param name="disposing">The disposing<see cref="bool"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            cameraFragment.Dispose();
            disposed = true;

            if (disposing)
            {
                SetOnClickListener(null);
                SetOnTouchListener(null);

                if (visualElementTracker != null)
                {
                    visualElementTracker.Dispose();
                    visualElementTracker = null;
                }

                if (visualElementRenderer != null)
                {
                    visualElementRenderer.Dispose();
                    visualElementRenderer = null;
                }

                if (Element != null)
                {
                    Element.PropertyChanged -= OnElementPropertyChanged;

                    if (Platform.GetRenderer(Element) == this)
                    {
                        Platform.SetRenderer(Element, null);
                    }
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// The MeasureExactly.
        /// </summary>
        /// <param name="control">The control<see cref="Android.Views.View"/>.</param>
        /// <param name="element">The element<see cref="VisualElement"/>.</param>
        /// <param name="context">The context<see cref="Context"/>.</param>
        private static void MeasureExactly(Android.Views.View control, VisualElement element, Context context)
        {
            if (control == null || element == null)
            {
                return;
            }

            double width = element.Width;
            double height = element.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            int realWidth = (int)context.ToPixels(width);
            int realHeight = (int)context.ToPixels(height);

            int widthMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realWidth, MeasureSpecMode.Exactly);
            int heightMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realHeight, MeasureSpecMode.Exactly);

            control.Measure(widthMeasureSpec, heightMeasureSpec);
        }

        /// <summary>
        /// The OnElementChanged.
        /// </summary>
        /// <param name="e">The e<see cref="ElementChangedEventArgs{CameraPreview}"/>.</param>
        private void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            CameraFragment newFragment = null;

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;
                cameraFragment.Dispose();
            }
            if (e.NewElement != null)
            {
                this.EnsureId();

                e.NewElement.PropertyChanged += OnElementPropertyChanged;

                ElevationHelper.SetElevation(this, e.NewElement);
                newFragment = new CameraFragment { Element = element };
            }

            FragmentManager.BeginTransaction()
                .Replace(Id, cameraFragment = newFragment, "camera")
                .Commit();
            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
        }

        /// <summary>
        /// The OnElementPropertyChanged.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="PropertyChangedEventArgs"/>.</param>
        private async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ElementPropertyChanged?.Invoke(this, e);

            switch (e.PropertyName)
            {
                case "Width":
                    await cameraFragment.RetrieveCameraDevice();
                    break;
            }
        }

        /// <summary>
        /// The GetDesiredSize.
        /// </summary>
        /// <param name="widthConstraint">The widthConstraint<see cref="int"/>.</param>
        /// <param name="heightConstraint">The heightConstraint<see cref="int"/>.</param>
        /// <returns>The <see cref="SizeRequest"/>.</returns>
        SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            Measure(widthConstraint, heightConstraint);
            SizeRequest result = new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size(Context.ToPixels(20), Context.ToPixels(20)));
            return result;
        }

        /// <summary>
        /// The MeasureExactly.
        /// </summary>
        void IViewRenderer.MeasureExactly() => MeasureExactly(this, Element, Context);

        /// <summary>
        /// The SetElement.
        /// </summary>
        /// <param name="element">The element<see cref="VisualElement"/>.</param>
        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            if (!(element is CameraPreview camera))
            {
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(CameraPreview)}");
            }

            if (visualElementTracker == null)
            {
                visualElementTracker = new VisualElementTracker(this);
            }
            Element = camera;
        }

        /// <summary>
        /// The SetLabelFor.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        void IVisualElementRenderer.SetLabelFor(int? id)
        {
            if (defaultLabelFor == null)
            {
                defaultLabelFor = LabelFor;
            }
            LabelFor = (int)(id ?? defaultLabelFor);
        }

        /// <summary>
        /// The UpdateLayout.
        /// </summary>
        void IVisualElementRenderer.UpdateLayout() => visualElementTracker?.UpdateLayout();

        #endregion Methods

        /// <summary>
        /// Defines the <see cref="MeasureSpecFactory" />.
        /// </summary>
        private static class MeasureSpecFactory
        {
            #region Methods

            /// <summary>
            /// The GetSize.
            /// </summary>
            /// <param name="measureSpec">The measureSpec<see cref="int"/>.</param>
            /// <returns>The <see cref="int"/>.</returns>
            public static int GetSize(int measureSpec)
            {
                const int modeMask = 0x3 << 30;
                return measureSpec & ~modeMask;
            }

            /// <summary>
            /// The MakeMeasureSpec.
            /// </summary>
            /// <param name="size">The size<see cref="int"/>.</param>
            /// <param name="mode">The mode<see cref="MeasureSpecMode"/>.</param>
            /// <returns>The <see cref="int"/>.</returns>
            public static int MakeMeasureSpec(int size, MeasureSpecMode mode) => size + (int)mode;

            #endregion Methods
        }
    }
}