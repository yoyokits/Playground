// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using CommunityToolkit.Maui.Views;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class ImageViewerView : ContentView
    {
        private CancellationTokenSource? _scrollDebounceCancel;
        // Unified sync guard: prevents feedback loops between CarouselView.PositionChanged
        // and CollectionView.SelectionChanged. Both handlers check this flag. It blocks
        // the synchronous PropertyChanged→Binding→Event propagation chain that would
        // otherwise cause infinite re-entrancy and carousel jumping.
        private bool _isSyncing = false;

        public ImageViewerView()
        {
            InitializeComponent();

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IsVisible)) return;
                if (!IsVisible)
                    StopSharedVideoPlayer();
                else
                    _ = UpdateOverlayPositionAsync();
            };
        }

        private void OnThumbnailSelected(object? sender, SelectionChangedEventArgs e)
        {
            // Guard 1: ignore binding-triggered events when the gallery is not yet visible.
            // Guard 2: prevent re-entrancy from the CurrentImageIndex→CurrentImageItem→SelectedItem
            //          loop and from carousel PositionChanged propagation.
            if (!IsVisible || _isSyncing || e.CurrentSelection.Count == 0) return;

            var selected = e.CurrentSelection[0] as string;
            if (string.IsNullOrEmpty(selected)) return;

            if (BindingContext is not MainPageViewModel vm) return;

            var index = vm.GalleryImagePaths.IndexOf(selected);
            if (index < 0 || index == MainCarousel.Position) return;

            _isSyncing = true;
            try
            {
                vm.CurrentImageIndex = index;
                // The OneWay Position binding jumps the carousel to this index.
                // DO NOT call MainCarousel.ScrollTo() here — it fires animated
                // intermediate PositionChanged events that feed back through
                // CurrentImageIndex→CurrentImageItem→SelectedItem→SelectionChanged,
                // causing the carousel to fight its own animation (jumping + crash).
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ImageViewerView] OnThumbnailSelected error: {ex.Message}");
            }
            finally
            {
                _isSyncing = false;
            }
            StopSharedVideoPlayer();
        }

        private async void OnCarouselPositionChanged(object? sender, EventArgs e)
        {
            // Skip events fired by our own programmatic updates (thumbnail tap, gallery open).
            if (_isSyncing) return;

            // Debounce: cancel previous timer and start a new one.
            // 150 ms lets fast swipe sequences settle before we do any work.
            _scrollDebounceCancel?.Cancel();
            _scrollDebounceCancel = new CancellationTokenSource();

            try
            {
                await Task.Delay(150, _scrollDebounceCancel.Token);

                // Debounce settled — sync ViewModel and thumbnail strip.
                // Position binding is OneWay (VM→Carousel), so user swipes do NOT
                // automatically push back to CurrentImageIndex. We do it here.
                _isSyncing = true;
                try
                {
                    if (BindingContext is MainPageViewModel vm && MainCarousel != null && ThumbnailStrip != null)
                    {
                        var newIndex = MainCarousel.Position;
                        if (newIndex >= 0 && newIndex < vm.GalleryImagePaths.Count)
                        {
                            vm.CurrentImageIndex = newIndex;
                            ThumbnailStrip.ScrollTo(newIndex, position: ScrollToPosition.Center, animate: false);
                        }
                    }
                }
                finally
                {
                    _isSyncing = false;
                }

                StopSharedVideoPlayer();

                if (BindingContext is MainPageViewModel viewModel && viewModel.IsMediaInfoVisible)
                    viewModel.IsMediaInfoVisible = false;

                await UpdateOverlayPositionAsync();
            }
            catch (OperationCanceledException)
            {
                // Debounce cancelled by a newer position change — expected, do nothing.
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

        private void OnImageAreaSizeChanged(object? sender, EventArgs e) => _ = UpdateOverlayPositionAsync();

        /// <summary>
        /// Adjusts DataOverlayPill's bottom/right margin so the pill sits at the
        /// bottom-right corner of the DISPLAYED image, not the Row 1 container.
        /// AspectFit centers the image with letterbox/pillarbox bars that vary per image,
        /// so the margin must be computed per image.
        ///
        /// Image dimensions come from the ExifHelper cache (populated on first read,
        /// served from memory on subsequent calls). Disk I/O runs off the main thread.
        /// </summary>
        private async Task UpdateOverlayPositionAsync()
        {
            const double EdgePad = 12;

            try
            {
                if (BindingContext is not MainPageViewModel vm) return;
                // Skip expensive dimension lookup when the overlay pill is hidden.
                if (!vm.IsGalleryDataOverlayVisible) return;
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
                // Use the ExifHelper cache — disk I/O only on first access per image,
                // subsequent calls are a dictionary lookup. Runs off main thread.
                var (cachedW, cachedH) = await Task.Run(
                    () => Helpers.ExifHelper.GetImageDimensions(filePath));
                imageW = cachedW;
                imageH = cachedH;
#endif

                if (imageW <= 0 || imageH <= 0)
                {
                    DataOverlayPill.Margin = new Thickness(0, 0, EdgePad, EdgePad);
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

                DataOverlayPill.Margin = new Thickness(0, 0, rightMargin, bottomMargin);
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
            DataOverlaySettingsPanel.BindingContext = vm;

            // Drive IsVisible from code so it isn't affected by the BindingContext override.
            mainVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(MainPageViewModel.IsGallerySettingsVisible)) return;

                DataOverlaySettingsPanel.IsVisible = mainVm.IsGallerySettingsVisible;
                if (mainVm.IsGallerySettingsVisible)
                {
                    vm.LoadFromOverlayItems(mainVm.OverlayItems);
                    vm.FontSize = mainVm.DataOverlayViewModel.FontSize;
                }
            };

            DataOverlaySettingsPanel.CloseRequested += async (s, e) =>
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
