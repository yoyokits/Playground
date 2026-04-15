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
        private CancellationTokenSource? _scrollDebounceCancel;
        // Prevents re-entrant calls: setting CurrentImageIndex notifies CurrentImageItem,
        // which updates SelectedItem via binding, which fires SelectionChanged again.
        private bool _isSyncingThumbnail = false;

        public ImageViewerView()
        {
            InitializeComponent();

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IsVisible)) return;
                if (!IsVisible)
                {
                    StopSharedVideoPlayer();
                }
                else
                {
                    UpdateOverlayPosition();
                    _ = LoadCurrentImageOverlayAsync();
                }
            };
        }

        private void OnThumbnailSelected(object? sender, SelectionChangedEventArgs e)
        {
            // Guard 1: ignore binding-triggered events when the gallery is not yet visible
            // (OpenImageViewer sets GalleryImagePaths/CurrentImageIndex before IsVisible=true,
            // which drives binding updates that fire SelectionChanged on a hidden/unlaid CarouselView).
            // Guard 2: prevent re-entrancy from the CurrentImageIndex→CurrentImageItem→SelectedItem loop.
            if (!IsVisible || _isSyncingThumbnail || e.CurrentSelection.Count == 0) return;

            var selected = e.CurrentSelection[0] as string;
            if (string.IsNullOrEmpty(selected)) return;

            if (BindingContext is MainPageViewModel vm)
            {
                var index = vm.GalleryImagePaths.IndexOf(selected);
                if (index < 0) return;

                _isSyncingThumbnail = true;
                try
                {
                    vm.CurrentImageIndex = index;
                    MainCarousel.ScrollTo(index, position: ScrollToPosition.Center, animate: true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[ImageViewerView] OnThumbnailSelected scroll error: {ex.Message}");
                }
                finally
                {
                    _isSyncingThumbnail = false;
                }
                StopSharedVideoPlayer();
            }
        }

        private async void OnCarouselPositionChanged(object? sender, EventArgs e)
        {
            // Debounce: cancel previous timer and start a new one.
            _scrollDebounceCancel?.Cancel();
            _scrollDebounceCancel = new CancellationTokenSource();

            try
            {
                await Task.Delay(50, _scrollDebounceCancel.Token);

                // Debounce settled — sync thumbnail strip.
                if (BindingContext is MainPageViewModel vm && MainCarousel != null && ThumbnailStrip != null)
                {
                    var newIndex = MainCarousel.Position;
                    if (newIndex >= 0 && newIndex < vm.GalleryImagePaths.Count)
                        ThumbnailStrip.ScrollTo(newIndex, position: ScrollToPosition.Center, animate: false);
                }

                StopSharedVideoPlayer();

                if (BindingContext is MainPageViewModel viewModel && viewModel.IsMediaInfoVisible)
                    viewModel.IsMediaInfoVisible = false;

                UpdateOverlayPosition();
                await LoadCurrentImageOverlayAsync();
            }
            catch (OperationCanceledException)
            {
                // Debounce cancelled by a newer position change — expected, do nothing.
            }
            finally
            {
            }
        }

        private void OnInfoBackdropTapped(object? sender, TappedEventArgs e)
        {
            if (BindingContext is MainPageViewModel vm)
                vm.IsMediaInfoVisible = false;
        }

        /// <summary>Absorb taps on the info card so they don't bubble to the backdrop.</summary>
        private void OnInfoCardTapped(object? sender, TappedEventArgs e)
        {
            // Intentionally empty — prevents backdrop tap from firing.
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

        // ── Sensor overlay position ────────────────────────────────────────

        private void OnImageAreaSizeChanged(object? sender, EventArgs e) => UpdateOverlayPosition();

        /// <summary>
        /// Asks the ViewModel to load EXIF overlay items for the currently displayed image.
        /// Called on gallery open and after each swipe settles.
        /// </summary>
        private async Task LoadCurrentImageOverlayAsync()
        {
            if (BindingContext is not MainPageViewModel vm) return;
            if (vm.GalleryImagePaths == null || vm.GalleryImagePaths.Count == 0) return;

            var index = MainCarousel?.Position ?? 0;
            if (index < 0 || index >= vm.GalleryImagePaths.Count) return;

            await vm.LoadGalleryOverlayItemsAsync(vm.GalleryImagePaths[index]);
        }

        /// <summary>
        /// Adjusts SensorOverlayPill's bottom/right margin so the pill sits at the
        /// bottom-right corner of the DISPLAYED image, not the Row 1 container.
        /// AspectFit centers the image with letterbox/pillarbox bars that vary per image,
        /// so the margin must be computed per image.
        ///
        /// Image raw dimensions come from BitmapFactory header-only decode (no pixels loaded).
        /// EXIF orientation is read to correct swapped W/H on rotated photos.
        /// </summary>
        private void UpdateOverlayPosition()
        {
            const double EdgePad = 12;

            try
            {
                if (BindingContext is not MainPageViewModel vm) return;
                if (vm.GalleryImagePaths == null || vm.GalleryImagePaths.Count == 0) return;

                var index = MainCarousel?.Position ?? 0;
                if (index < 0 || index >= vm.GalleryImagePaths.Count) return;

                // Use ImageAreaGrid (the named Row-1 Grid) for reliable container dimensions.
                double containerW = ImageAreaGrid.Width;
                double containerH = ImageAreaGrid.Height;
                if (containerW <= 0 || containerH <= 0) return;

                double imageW = 0, imageH = 0;

#if ANDROID
                var filePath = vm.GalleryImagePaths[index];
                if (File.Exists(filePath))
                {
                    // Header-only decode — reads just a few bytes, does not load pixels.
                    var opts = new Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
                    Android.Graphics.BitmapFactory.DecodeFile(filePath, opts);
                    imageW = opts.OutWidth;
                    imageH = opts.OutHeight;

                    // BitmapFactory returns raw stored dimensions, ignoring EXIF orientation.
                    // Correct for 90°/270° rotations so the aspect ratio reflects what is DISPLAYED.
                    try
                    {
                        using var exif = new Android.Media.ExifInterface(filePath);
                        int orientation = exif.GetAttributeInt(
                            Android.Media.ExifInterface.TagOrientation,
                            (int)Android.Media.Orientation.Normal);
                        // Values 5–8 indicate a 90° or 270° rotation → swap W and H.
                        if (orientation >= 5)
                            (imageW, imageH) = (imageH, imageW);
                    }
                    catch { /* EXIF unreadable — use raw dimensions */ }
                }
#endif

                if (imageW <= 0 || imageH <= 0)
                {
                    SensorOverlayPill.Margin = new Thickness(0, 0, EdgePad, EdgePad);
                    return;
                }

                double containerAspect = containerW / containerH;
                double imageAspect    = imageW / imageH;

                double displayW, displayH;
                if (containerAspect > imageAspect)
                {
                    // Pillarbox: image fits full height, black bars left and right.
                    displayH = containerH;
                    displayW = containerH * imageAspect;
                }
                else
                {
                    // Letterbox: image fits full width, black bars top and bottom.
                    displayW = containerW;
                    displayH = containerW / imageAspect;
                }

                double rightMargin  = (containerW - displayW) / 2.0 + EdgePad;
                double bottomMargin = (containerH - displayH) / 2.0 + EdgePad;

                SensorOverlayPill.Margin = new Thickness(0, 0, rightMargin, bottomMargin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageViewerView] UpdateOverlayPosition error: {ex.Message}");
            }
        }

        // ── Sensor settings overlay ────────────────────────────────────────

        /// <summary>
        /// Called once from MainPage constructor to wire the shared OverlaySettingsViewModel
        /// into this view's settings overlay.
        /// IsVisible is driven from code (not XAML binding) because setting BindingContext
        /// to OverlaySettingsViewModel would break any MainPageViewModel XAML bindings.
        /// </summary>
        public void WireSensorSettings(OverlaySettingsViewModel vm, MainPageViewModel mainVm)
        {
            SensorSettingsOverlay.BindingContext = vm;

            // Drive IsVisible from code so it isn't affected by the BindingContext override.
            mainVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(MainPageViewModel.IsGallerySettingsVisible)) return;

                SensorSettingsOverlay.IsVisible = mainVm.IsGallerySettingsVisible;
                if (mainVm.IsGallerySettingsVisible)
                {
                    vm.LoadFromOverlayItems(mainVm.OverlayItems);
                    vm.FontSize = mainVm.DataOverlayViewModel.FontSize;
                }
            };

            SensorSettingsOverlay.CloseRequested += async (s, e) =>
            {
                try { await HideGallerySettingsAsync(vm, mainVm); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[ImageViewerView] HideSettings error: {ex.Message}");
                }
            };
        }

        private async Task HideGallerySettingsAsync(OverlaySettingsViewModel vm, MainPageViewModel mainVm)
        {
            mainVm.DataOverlayViewModel.FontSize = vm.FontSize;
            await vm.SaveSettingsAsync();
            vm.ApplyToOverlayItems(mainVm.OverlayItems);
            mainVm.DataOverlayViewModel.RefreshVisibleItems();
            mainVm.IsGallerySettingsVisible = false;
        }
    }
}
