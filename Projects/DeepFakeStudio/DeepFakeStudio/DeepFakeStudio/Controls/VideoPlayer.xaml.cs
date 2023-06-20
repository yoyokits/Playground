namespace DeepFakeStudio.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for VideoPlayer.xaml.
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        #region Fields

        public static readonly DependencyProperty PathProperty =
         DependencyProperty.Register(nameof(Path), typeof(string), typeof(VideoPlayer), new
            PropertyMetadata(null, new PropertyChangedCallback(OnPathChanged)));

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayer"/> class.
        /// </summary>
        public VideoPlayer()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnPathChanged.
        /// </summary>
        /// <param name="d">The d<see cref="DependencyObject"/>.</param>
        /// <param name="e">The e<see cref="DependencyPropertyChangedEventArgs"/>.</param>
        private static void OnPathChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            var player = (VideoPlayer)d;
        }

        /// <summary>
        /// The OnPauseButton_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnPauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.MediaElement.Pause();
        }

        /// <summary>
        /// The OnPlayButton_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnPlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.MediaElement.Play();
        }

        #endregion Methods
    }
}