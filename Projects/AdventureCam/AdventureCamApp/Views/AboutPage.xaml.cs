﻿namespace AdventureCamApp.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage(AboutViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
