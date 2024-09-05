namespace AdventureCamApp.Views;

public partial class GalleryPage : ContentPage
{
	public GalleryPage(GalleryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
