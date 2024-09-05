using System;

namespace AdventureCamApp.Views;

public partial class PreviewPage : ContentPage
{
	public PreviewPage(PreviewViewModel viewModel)
	{
		// TODO: change the source and add your own controls for playback as necessary.
		InitializeComponent();
		BindingContext = viewModel;
	}

	void Page_Unloaded(object sender, EventArgs e)
	{
		// Stop and cleanup MediaElement when we navigate away
		mediaElement.Handler?.DisconnectHandler();
	}
}
