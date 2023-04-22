using System.Net.NetworkInformation;
using System.Net.Sockets;
using EmbedIO;
using EmbedIO.WebApi;

namespace FileShareApp;

public partial class MainPage : ContentPage
{
    private WebServer Server { get; set; }

    private bool IsOnline { get; set; }

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOnlineClicked(object sender, EventArgs e)
    {
        if (this.IsOnline)
        {
            this.Server?.Dispose();
        }
        else
        {
            // Server must be started, before WebView is initialized,
            // because we have no reload implemented in this sample.
            await Task.Factory.StartNew(async () =>
            {
                var url = "http://*:8080";
                this.Server = new WebServer(HttpListenerMode.EmbedIO, url);
                var assembly = typeof(App).Assembly;
                this.Server.WithLocalSessionManager();
                this.Server.WithWebApi("/api", m => m.WithController(() => new ApiController()));
                this.Server.WithEmbeddedResources("/", assembly, "EmbedIO.Forms.Sample.html");
                await this.Server.RunAsync();
            });

            var ips = GetLocalIPv4();
            this.Webview.Reload();
        }

        this.IsOnline = !this.IsOnline;
        this.OnlineStatusLabel.Text = this.IsOnline ? "Online" : "Offline";
        SemanticScreenReader.Announce(this.OnlineStatusLabel.Text);
    }

    public static string GetLocalIPv4()
    {
        var ipAddrList = new List<string>();
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface item in interfaces)
        {
            var type = item.NetworkInterfaceType;
            if (type == NetworkInterfaceType.Ethernet && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddrList.Add(ip.Address.ToString());
                    }
                }
            }
        }
        return ipAddrList.First();
    }
}