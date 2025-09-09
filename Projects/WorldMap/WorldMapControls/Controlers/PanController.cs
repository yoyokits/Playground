// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Handles mouse drag panning for the map view.
    /// </summary>
    public class PanController
    {
        #region Fields

        /// <summary>
        /// Defines the _scrollViewer
        /// </summary>
        private readonly ScrollViewer _scrollViewer;

        /// <summary>
        /// Defines the _isPanning
        /// </summary>
        private bool _isPanning = false;

        /// <summary>
        /// Defines the _lastPanPoint
        /// </summary>
        private Point _lastPanPoint;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PanController"/> class.
        /// </summary>
        /// <param name="scrollViewer">The scrollViewer<see cref="ScrollViewer"/>.</param>
        public PanController(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer ?? throw new ArgumentNullException(nameof(scrollViewer));
            SetupMouseEvents();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Defines the PanEnded.
        /// </summary>
        public event EventHandler<PanEventArgs>? PanEnded;

        /// <summary>
        /// Defines the PanStarted.
        /// </summary>
        public event EventHandler<PanEventArgs>? PanStarted;

        /// <summary>
        /// Defines the PanUpdated.
        /// </summary>
        public event EventHandler<PanEventArgs>? PanUpdated;

        #endregion Events

        #region Methods

        /// <summary>
        /// The EndPan.
        /// </summary>
        public void EndPan()
        {
            if (!_isPanning) return;

            _isPanning = false;
            _scrollViewer.ReleaseMouseCapture();
            _scrollViewer.Cursor = Cursors.Arrow;

            PanEnded?.Invoke(this, new PanEventArgs(_lastPanPoint, 0, 0));
        }

        /// <summary>
        /// The StartPan.
        /// </summary>
        /// <param name="startPoint">The startPoint<see cref="Point"/>.</param>
        public void StartPan(Point startPoint)
        {
            if (_isPanning) return;

            _isPanning = true;
            _lastPanPoint = startPoint;
            _scrollViewer.CaptureMouse();
            _scrollViewer.Cursor = Cursors.Hand;

            PanStarted?.Invoke(this, new PanEventArgs(startPoint, 0, 0));
        }

        /// <summary>
        /// The UpdatePan.
        /// </summary>
        /// <param name="currentPoint">The currentPoint<see cref="Point"/>.</param>
        public void UpdatePan(Point currentPoint)
        {
            if (!_isPanning) return;

            var deltaX = _lastPanPoint.X - currentPoint.X;
            var deltaY = _lastPanPoint.Y - currentPoint.Y;

            // Apply panning by adjusting scroll position
            var newHorizontalOffset = _scrollViewer.HorizontalOffset + deltaX;
            var newVerticalOffset = _scrollViewer.VerticalOffset + deltaY;

            _scrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            _scrollViewer.ScrollToVerticalOffset(newVerticalOffset);

            _lastPanPoint = currentPoint;

            PanUpdated?.Invoke(this, new PanEventArgs(currentPoint, deltaX, deltaY));
        }

        /// <summary>
        /// The OnMouseLeave.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseEventArgs"/>.</param>
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                EndPan();
            }
        }

        /// <summary>
        /// The OnMouseLeftButtonDown.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(_scrollViewer);
            StartPan(position);
            e.Handled = true;
        }

        /// <summary>
        /// The OnMouseLeftButtonUp.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndPan();
            e.Handled = true;
        }

        /// <summary>
        /// The OnMouseMove.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseEventArgs"/>.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(_scrollViewer);
                UpdatePan(position);
                e.Handled = true;
            }
        }

        /// <summary>
        /// The SetupMouseEvents.
        /// </summary>
        private void SetupMouseEvents()
        {
            _scrollViewer.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _scrollViewer.MouseMove += OnMouseMove;
            _scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            _scrollViewer.MouseLeave += OnMouseLeave;
        }

        #endregion Methods
    }

    /// <summary>
    /// Event arguments for pan operations.
    /// </summary>
    public class PanEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PanEventArgs"/> class.
        /// </summary>
        /// <param name="position">The position<see cref="Point"/>.</param>
        /// <param name="deltaX">The deltaX<see cref="double"/>.</param>
        /// <param name="deltaY">The deltaY<see cref="double"/>.</param>
        public PanEventArgs(Point position, double deltaX, double deltaY)
        {
            Position = position;
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the DeltaX.
        /// </summary>
        public double DeltaX { get; }

        /// <summary>
        /// Gets the DeltaY.
        /// </summary>
        public double DeltaY { get; }

        /// <summary>
        /// Gets the Position.
        /// </summary>
        public Point Position { get; }

        #endregion Properties
    }
}