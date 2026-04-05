// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class ImageViewerView : ContentView
    {
        public ImageViewerView()
        {
            InitializeComponent();
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
    }
}
