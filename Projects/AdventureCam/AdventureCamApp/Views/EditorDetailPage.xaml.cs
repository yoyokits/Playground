namespace AdventureCamApp.Views;

public partial class EditorDetailPage : ContentPage
{
	public EditorDetailPage(EditorDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
