// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //

using CommunityToolkit.Maui.Views;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class ImageViewerView : ContentView
    {
        public ImageViewerView()
        {
            InitializeComponent();

            // Stop the shared video player automatically when the gallery is closed.
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IsVisible) && !IsVisible)
                    StopSharedVideoPlayer();
            };
        }

        private void OnThumbnailSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0) return;

            var selected = e.CurrentSelection[0] as string;
            if (string.IsNullOrEmpty(selected)) return;

            if (BindingContext is MainPageViewModel vm)
            {
                var index = vm.GalleryImagePaths.IndexOf(selected);
                if (index >= 0)
                    vm.CurrentImageIndex = index;
            }
        }

        private void OnCarouselPositionChanged(object? sender, EventArgs e)
        {
            // Scroll the thumbnail strip to keep it in sync
            if (BindingContext is MainPageViewModel vm && MainCarousel != null && ThumbnailStrip != null)
            {
                var newIndex = MainCarousel.Position;
                if (newIndex >= 0 && newIndex < vm.GalleryImagePaths.Count)
                    ThumbnailStrip.ScrollTo(newIndex, position: ScrollToPosition.Center, animate: true);
            }

            // Hide the shared video player when navigating to another item
            StopSharedVideoPlayer();
        }

        /// <summary>
        /// Tapping the play overlay: assigns source to the single shared MediaElement
        /// and makes it visible. Only one ExoPlayer instance is ever active at a time,
        /// which prevents the resource exhaustion crash that happens when per-item
        /// MediaElements accumulate during rapid swiping.
        /// </summary>
        private void OnVideoPlayTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Border playOverlay) return;
            if (playOverlay.Parent is not Grid parentGrid) return;
            if (parentGrid.BindingContext is not string filePath) return;

            try
            {
                SharedVideoPlayer.ShouldAutoPlay = true;
                SharedVideoPlayer.Source = new FileMediaSource { Path = filePath };
                SharedVideoPlayer.IsVisible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ImageViewerView] OnVideoPlayTapped error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops and hides the shared MediaElement, freeing the ExoPlayer resource.
        /// Called on swipe, thumbnail tap, and gallery close.
        /// </summary>
        private void StopSharedVideoPlayer()
        {
            if (!SharedVideoPlayer.IsVisible) return;

            try
            {
                SharedVideoPlayer.Stop();
                SharedVideoPlayer.Source = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ImageViewerView] StopSharedVideoPlayer error: {ex.Message}");
            }

            SharedVideoPlayer.IsVisible = false;
        }
    }
}
