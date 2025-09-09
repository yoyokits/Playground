// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Rendering
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Windows;
    using System.Windows.Media;
    using WpfPath = System.Windows.Shapes.Path;

    /// <summary>
    /// Builds WPF paths from GeoJSON geometry.
    /// </summary>
    public class CountryPathBuilder
    {
        #region Methods

        /// <summary>
        /// The BuildMultiPolygonPaths.
        /// </summary>
        /// <param name="geometry">The geometry<see cref="JsonNode?"/>.</param>
        /// <param name="mapWidth">The mapWidth<see cref="double"/>.</param>
        /// <param name="mapHeight">The mapHeight<see cref="double"/>.</param>
        /// <returns>The <see cref="IEnumerable{WpfPath}"/>.</returns>
        public IEnumerable<WpfPath> BuildMultiPolygonPaths(JsonNode? geometry, double mapWidth, double mapHeight)
        {
            var coordinates = geometry?["coordinates"] as JsonArray;
            if (coordinates == null) yield break;

            foreach (var polygonNode in coordinates)
            {
                var path = BuildPathFromRings(polygonNode as JsonArray, mapWidth, mapHeight);
                if (path != null) yield return path;
            }
        }

        /// <summary>
        /// The BuildPolygonPath.
        /// </summary>
        /// <param name="geometry">The geometry<see cref="JsonNode?"/>.</param>
        /// <param name="mapWidth">The mapWidth<see cref="double"/>.</param>
        /// <param name="mapHeight">The mapHeight<see cref="double"/>.</param>
        /// <returns>The <see cref="WpfPath?"/>.</returns>
        public WpfPath? BuildPolygonPath(JsonNode? geometry, double mapWidth, double mapHeight)
        {
            var coordinates = geometry?["coordinates"] as JsonArray;
            return BuildPathFromRings(coordinates, mapWidth, mapHeight);
        }

        /// <summary>
        /// The ProjectEquirectangular.
        /// </summary>
        /// <param name="lon">The lon<see cref="double"/>.</param>
        /// <param name="lat">The lat<see cref="double"/>.</param>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        /// <returns>The <see cref="Point"/>.</returns>
        private static Point ProjectEquirectangular(double lon, double lat, double width, double height)
        {
            var x = (lon + 180.0) / 360.0 * width;
            var y = (90.0 - lat) / 180.0 * height;
            return new Point(x, y);
        }

        /// <summary>
        /// The BuildPathFromRings.
        /// </summary>
        /// <param name="ringsArray">The ringsArray<see cref="JsonArray?"/>.</param>
        /// <param name="mapWidth">The mapWidth<see cref="double"/>.</param>
        /// <param name="mapHeight">The mapHeight<see cref="double"/>.</param>
        /// <returns>The <see cref="WpfPath?"/>.</returns>
        private WpfPath? BuildPathFromRings(JsonArray? ringsArray, double mapWidth, double mapHeight)
        {
            if (ringsArray == null) return null;

            var pathGeometry = new PathGeometry { FillRule = FillRule.EvenOdd };

            foreach (var ringNode in ringsArray)
            {
                if (ringNode is not JsonArray ring) continue;

                var figure = CreatePathFigure(ring, mapWidth, mapHeight);
                if (figure != null) pathGeometry.Figures.Add(figure);
            }

            if (pathGeometry.Figures.Count == 0) return null;

            if (pathGeometry.CanFreeze) pathGeometry.Freeze();

            return new WpfPath { Data = pathGeometry };
        }

        /// <summary>
        /// The CreatePathFigure.
        /// </summary>
        /// <param name="ring">The ring<see cref="JsonArray"/>.</param>
        /// <param name="mapWidth">The mapWidth<see cref="double"/>.</param>
        /// <param name="mapHeight">The mapHeight<see cref="double"/>.</param>
        /// <returns>The <see cref="PathFigure?"/>.</returns>
        private PathFigure? CreatePathFigure(JsonArray ring, double mapWidth, double mapHeight)
        {
            var figure = new PathFigure();
            bool firstPoint = true;

            foreach (var coordPairNode in ring)
            {
                if (coordPairNode is not JsonArray lonLat || lonLat.Count < 2) continue;

                var lon = lonLat[0]?.GetValue<double>() ?? 0;
                var lat = lonLat[1]?.GetValue<double>() ?? 0;

                var point = ProjectEquirectangular(lon, lat, mapWidth, mapHeight);

                if (firstPoint)
                {
                    figure.StartPoint = point;
                    firstPoint = false;
                }
                else
                {
                    figure.Segments.Add(new LineSegment(point, true));
                }
            }

            if (firstPoint) return null; // No valid points found

            figure.IsClosed = true;
            figure.IsFilled = true;
            return figure;
        }

        #endregion Methods
    }
}