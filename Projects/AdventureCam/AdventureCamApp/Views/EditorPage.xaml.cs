namespace AdventureCamApp.Views;

public partial class EditorPage : ContentPage
{
	EditorViewModel ViewModel;

	public EditorPage(EditorViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = ViewModel = viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		await ViewModel.LoadDataAsync();
	}
}
