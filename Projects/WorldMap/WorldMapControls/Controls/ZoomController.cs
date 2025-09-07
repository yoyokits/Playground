// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Controls
{
    using System;
    using System.Windows;
    using WorldMapApp.Models;

    /// <summary>
    /// Handles zoom control logic.
    /// </summary>
    public class ZoomController
    {
        #region Fields

        /// <summary>
        /// Defines the _maxZoom
        /// </summary>
        private readonly double _maxZoom;

        /// <summary>
        /// Defines the _minZoom
        /// </summary>
        private readonly double _minZoom;

        /// <summary>
        /// Defines the _zoomStep
        /// </summary>
        private readonly double _zoomStep;

        /// <summary>
        /// Defines the _currentZoom
        /// </summary>
        private double _currentZoom = 1.0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomController"/> class.
        /// </summary>
        /// <param name="minZoom">The minZoom<see cref="double"/>.</param>
        /// <param name="maxZoom">The maxZoom<see cref="double"/>.</param>
        /// <param name="zoomStep">The zoomStep<see cref="double"/>.</param>
        public ZoomController(double minZoom, double maxZoom, double zoomStep)
        {
            _minZoom = minZoom;
            _maxZoom = maxZoom;
            _zoomStep = zoomStep;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// The HandleZoom.
        /// </summary>
        /// <param name="wheelDelta">The wheelDelta<see cref="int"/>.</param>
        /// <param name="mousePosition">The mousePosition<see cref="Point"/>.</param>
        /// <returns>The <see cref="ZoomResult"/>.</returns>
        public ZoomResult HandleZoom(int wheelDelta, Point mousePosition)
        {
            var delta = wheelDelta > 0 ? _zoomStep : -_zoomStep;
            var newZoom = Math.Clamp(_currentZoom + delta, _minZoom, _maxZoom);

            var zoomChanged = Math.Abs(newZoom - _currentZoom) >= 0.0001;
            var previousZoom = _currentZoom;

            if (zoomChanged)
                _currentZoom = newZoom;

            return new ZoomResult(zoomChanged, newZoom, previousZoom);
        }

        #endregion Methods
    }
}