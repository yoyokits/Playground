// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.StableDiffusion
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using DerDieDasAICore.StableDiffusion;
    using FluentAssertions;

    [TestClass]
    public class StableDiffusionViewerUiTest
    {
        #region Methods

        [TestMethod]
        public void CanCreateWindowWithViewer()
        {
            var tcs = new TaskCompletionSource<Exception?>();
            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current == null)
                    {
                        _ = new Application();
                    }

                    var window = new Window { Width = 800, Height = 600, ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow };
                    var viewer = new StableDiffusionViewer();
                    window.Content = viewer;
                    window.Show();

                    viewer.DataContext.Should().BeOfType<StableDiffusionViewModel>();
                    var vm = (StableDiffusionViewModel)viewer.DataContext;
                    vm.PositivePrompt = "Test prompt";
                    vm.NegativePrompt = "bad, low quality";
                    vm.CfgScale.Should().BeGreaterThan(0);

                    Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        window.Close();
                        tcs.SetResult(null);
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    }, DispatcherPriority.ApplicationIdle);

                    Dispatcher.Run();
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(ex);
                    try { Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background); } catch { }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            var exception = tcs.Task.GetAwaiter().GetResult();
            exception.Should().BeNull();
        }

        #endregion Methods
    }
}