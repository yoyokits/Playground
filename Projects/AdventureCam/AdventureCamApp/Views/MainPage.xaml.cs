// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace AdventureCamApp.Views
{
    using Camera.MAUI;

    public partial class MainPage : ContentPage
    {
        #region Constructors

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        #endregion Constructors
    }
}