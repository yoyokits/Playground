namespace DeepFakeStudio.Controls
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Helpers;

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
            this.MediaElement.LoadedBehavior = MediaState.Manual;
            this.MediaElement.UnloadedBehavior = MediaState.Manual;
            this.MediaElement.MediaEnded += OnMediaElement_MediaEnded;
            this.MediaElement.MediaOpened += OnMediaElement_MediaOpened;
            this.PlayBackTimer.Tick += OnPlayBackTimer_Tick;
            this.PlayBackTimer.Interval = new TimeSpan(100);
            this.Unloaded += OnVideoPlayer_Unloaded;
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

        /// <summary>
        /// Gets or sets a value indicating whether IsPlaying.
        /// </summary>
        private bool IsPlaying { get; set; }

        /// <summary>
        /// Gets or sets the NaturalDuration.
        /// </summary>
        private TimeSpan NaturalDuration { get; set; }

        /// <summary>
        /// Gets the PlayBackTimer.
        /// </summary>
        private DispatcherTimer PlayBackTimer { get; } = new();

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
            var path = e.NewValue.ToString();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Logger.Error("Invalid video path or file doesn't exist");
                return;
            }

            try
            {
                var player = (VideoPlayer)d;
                player.MediaElement.Source = new Uri(path);
                player.OnPlayButton_Click(player, null);
            }
            catch (Exception exception)
            {
                Logger.Error($"Cannot open video: {exception}");
            }
        }

        /// <summary>
        /// The OnMediaElement_MediaEnded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.IsPlaying = false;
            this.PlayBackTimer.Stop();
            this.PlayBackTimer.Stop();
            this.UpdateSliderPosition();
        }

        /// <summary>
        /// The OnMediaElement_MediaOpened.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            this.NaturalDuration = !this.MediaElement.NaturalDuration.HasTimeSpan ? new() : this.MediaElement.NaturalDuration.TimeSpan;
            this.Slider.Maximum = this.NaturalDuration.TotalMilliseconds;
            this.UpdateSliderPosition();
        }

        /// <summary>
        /// The OnPauseButton_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnPauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.MediaElement.Pause();
            this.PlayBackTimer.Stop();
            this.UpdateSliderPosition();
            this.IsPlaying = false;
        }

        /// <summary>
        /// The OnPlayBackTimer_Tick.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void OnPlayBackTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateSliderPosition();
        }

        /// <summary>
        /// The OnPlayButton_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnPlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsPlaying = true;
            this.MediaElement.Play();
            this.PlayBackTimer.Start();
            this.UpdateSliderPosition();
        }

        /// <summary>
        /// The OnSlider_DragCompleted.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Controls.Primitives.DragCompletedEventArgs"/>.</param>
        private void OnSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.UpdatePlayBackPosition();
            if (this.IsPlaying)
            {
                this.OnPlayButton_Click(this, null);
            }
        }

        /// <summary>
        /// The OnSlider_DragStarted.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Controls.Primitives.DragStartedEventArgs"/>.</param>
        private void OnSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.MediaElement.Pause();
            this.PlayBackTimer.Stop();
            this.UpdateSliderPosition();
        }

        /// <summary>
        /// The OnStopButton_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnStopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Stop();
        }

        /// <summary>
        /// The OnVideoPlayer_Unloaded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnVideoPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MediaElement.MediaEnded -= OnMediaElement_MediaEnded;
            this.MediaElement.MediaOpened -= OnMediaElement_MediaOpened;
            this.IsPlaying = false;
            this.PlayBackTimer.Tick -= OnPlayBackTimer_Tick;
            this.PlayBackTimer.Stop();
        }

        /// <summary>
        /// The Stop.
        /// </summary>
        private void Stop()
        {
            this.IsPlaying = false;
            this.MediaElement.Stop();
            this.PlayBackTimer.Stop();
            this.UpdateSliderPosition();
        }

        /// <summary>
        /// The UpdatePlayBackPosition.
        /// </summary>
        private void UpdatePlayBackPosition()
        {
            if (this.NaturalDuration.TotalMilliseconds == 0)
            {
                return;
            }

            var milliSeconds = this.Slider.Value;
            var positionThick = (long)(milliSeconds * 10000);
            var durationTimeSpan = new TimeSpan(positionThick);
            this.MediaElement.Position = durationTimeSpan;
            ////this.MediaElement.Clock.Controller.Seek(durationTimeSpan, new System.Windows.Media.Animation.TimeSeekOrigin());
        }

        /// <summary>
        /// The UpdateSliderPosition.
        /// </summary>
        private void UpdateSliderPosition()
        {
            this.Slider.Value = this.MediaElement.Position.TotalMilliseconds;
        }

        #endregion Methods
    }
}