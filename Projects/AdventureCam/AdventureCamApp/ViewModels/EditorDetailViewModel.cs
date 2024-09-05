namespace AdventureCamApp.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class EditorDetailViewModel : BaseViewModel
{
	[ObservableProperty]
	SampleItem? item;
}
