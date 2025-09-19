namespace DerDieDasAICore.StableDiffusion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for StableDiffusionViewer.xaml
    /// </summary>
    public partial class StableDiffusionViewer : UserControl
    {
        public StableDiffusionViewer()
        {
            InitializeComponent();
            var vm = new StableDiffusionViewModel();
            DataContext = vm;
            this.Loaded += async (_, __) => await vm.InitializeAsync();
        }
    }
}
