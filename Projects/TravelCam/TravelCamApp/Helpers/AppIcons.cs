// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //

namespace TravelCamApp.Helpers
{
    /// <summary>
    /// Shared SVG path data strings for icons used across the app.
    /// Reference in XAML via: Data="{x:Static helpers:AppIcons.OverlaySettings}"
    /// </summary>
    public static class AppIcons
    {
        /// <summary>Material tune/sliders icon — used on the overlay settings button
        /// in both the camera view and the gallery bottom bar.</summary>
        public const string OverlaySettings =
            "M3 17v2h6v-2H3zM3 5v2h10V5H3zm10 16v-2h8v-2h-8v-2h-2v6h2zM7 9v2H3v2h4v2h2V9H7zm14 4v-2H11v2h10zm-6-4h2V7h4V5h-4V3h-2v6z";
    }
}
