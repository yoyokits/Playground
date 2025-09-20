using PostAuraCore.Services;

namespace PostAuraApp.Views;

public partial class ChatPage : ContentPage
{
	public ChatPage()
	{
		InitializeComponent();
        BindingContext = new ChatViewModel(new GeminiLlmService());
	}
}
