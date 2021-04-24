namespace XamarinFormsXamlCameraTest.Droid
{
    using Android.Content;
    using Android.Runtime;
    using Android.Util;
    using Android.Views;
    using System;

    /// <summary>
    /// Defines the <see cref="AutoFitTextureView" />.
    /// </summary>
    public class AutoFitTextureView : TextureView
    {
        #region Fields

        private readonly object locker = new object();

        private int mRatioHeight = 0;

        private int mRatioWidth = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Context"/>.</param>
        public AutoFitTextureView(Context context) : this(context, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Context"/>.</param>
        /// <param name="attrs">The attrs<see cref="IAttributeSet"/>.</param>
        public AutoFitTextureView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="Context"/>.</param>
        /// <param name="attrs">The attrs<see cref="IAttributeSet"/>.</param>
        /// <param name="defStyle">The defStyle<see cref="int"/>.</param>
        public AutoFitTextureView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="javaReference">The javaReference<see cref="IntPtr"/>.</param>
        /// <param name="transfer">The transfer<see cref="JniHandleOwnership"/>.</param>
        protected AutoFitTextureView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// The ClearCanvas.
        /// </summary>
        /// <param name="color">The color<see cref="Android.Graphics.Color"/>.</param>
        public void ClearCanvas(Android.Graphics.Color color)
        {
            using var canvas = LockCanvas(null);
            lock (locker)
            {
                try
                {
                    canvas.DrawColor(color);
                }
                finally
                {
                    UnlockCanvasAndPost(canvas);
                }

                Invalidate();
            }
        }

        /// <summary>
        /// The SetAspectRatio.
        /// </summary>
        /// <param name="width">The width<see cref="int"/>.</param>
        /// <param name="height">The height<see cref="int"/>.</param>
        public void SetAspectRatio(int width, int height)
        {
            if (width == 0 || height == 0)
            {
                throw new ArgumentException("Size can't be negative.");
            }

            mRatioWidth = width;
            mRatioHeight = height;
            RequestLayout();
        }

        /// <summary>
        /// The OnMeasure.
        /// </summary>
        /// <param name="widthMeasureSpec">The widthMeasureSpec<see cref="int"/>.</param>
        /// <param name="heightMeasureSpec">The heightMeasureSpec<see cref="int"/>.</param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int width = MeasureSpec.GetSize(widthMeasureSpec);
            int height = MeasureSpec.GetSize(heightMeasureSpec);

            if (mRatioWidth == 0 || mRatioHeight == 0)
            {
                SetMeasuredDimension(width, height);
            }
            else
            {
                if (width < (float)height * mRatioWidth / mRatioHeight)
                {
                    SetMeasuredDimension(width, width * mRatioHeight / mRatioWidth);
                }
                else
                {
                    SetMeasuredDimension(height * mRatioWidth / mRatioHeight, height);
                }
            }
        }

        #endregion Methods
    }
}