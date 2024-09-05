namespace AdventureCamApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(EditorDetailPage), typeof(EditorDetailPage));
	}
}
