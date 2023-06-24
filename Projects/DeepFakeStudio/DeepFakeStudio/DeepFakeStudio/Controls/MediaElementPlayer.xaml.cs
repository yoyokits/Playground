namespace DeepFakeStudio.Controls
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Interaction logic for MediaElementPlayer.xaml.
    /// </summary>
    public partial class MediaElementPlayer : UserControl
    {
        #region Fields

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(MediaElementPlayer), new PropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged)));

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElementPlayer"/> class.
        /// </summary>
        public MediaElementPlayer()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnSourceChanged.
        /// </summary>
        /// <param name="d">The d<see cref="DependencyObject"/>.</param>
        /// <param name="e">The e<see cref="DependencyPropertyChangedEventArgs"/>.</param>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var path = e.NewValue.ToString();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Logger.Error("Invalid video path or file doesn't exist");
                return;
            }

            try
            {
                var player = (MediaElementPlayer)d;
                player.MediaElement.Source = new Uri(path);
            }
            catch (Exception exception)
            {
                Logger.Error($"Cannot open video: {exception}");
            }
        }

        /// <summary>
        /// The OnMediaFailed.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="ExceptionRoutedEventArgs"/>.</param>
        private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _ = MessageBox.Show(e.ErrorException.Message, "Media failed");
        }

        #endregion Methods

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }
    }
}