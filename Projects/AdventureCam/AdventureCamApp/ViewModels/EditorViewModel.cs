﻿namespace AdventureCamApp.ViewModels;

public partial class EditorViewModel : BaseViewModel
{
	readonly SampleDataService dataService;

	[ObservableProperty]
	bool isRefreshing;

	[ObservableProperty]
	ObservableCollection<SampleItem>? items;

	public EditorViewModel(SampleDataService service)
	{
		dataService = service;
	}

	[RelayCommand]
	private async Task OnRefreshing()
	{
		IsRefreshing = true;

		try
		{
			await LoadDataAsync();
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	[RelayCommand]
	public async Task LoadMore()
	{
		if (Items is null)
		{
			return;
		}

		var moreItems = await dataService.GetItems();

		foreach (var item in moreItems)
		{
			Items.Add(item);
		}
	}

	public async Task LoadDataAsync()
	{
		Items = new ObservableCollection<SampleItem>(await dataService.GetItems());
	}

	[RelayCommand]
	private async Task GoToDetails(SampleItem item)
	{
		await Shell.Current.GoToAsync(nameof(EditorDetailPage), true, new Dictionary<string, object>
		{
			{ "Item", item }
		});
	}
}
