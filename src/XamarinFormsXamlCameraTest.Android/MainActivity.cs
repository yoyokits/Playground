namespace XamarinFormsXamlCameraTest.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Runtime;

    /// <summary>
    /// Defines the <see cref="MainActivity" />.
    /// </summary>
    [Activity(Label = "XamarinFormsXamlCameraTest", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        #region Methods

        /// <summary>
        /// The OnRequestPermissionsResult.
        /// </summary>
        /// <param name="requestCode">The requestCode<see cref="int"/>.</param>
        /// <param name="permissions">The permissions<see cref="string[]"/>.</param>
        /// <param name="grantResults">The grantResults<see cref="Android.Content.PM.Permission[]"/>.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// The OnCreate.
        /// </summary>
        /// <param name="savedInstanceState">The savedInstanceState<see cref="Bundle"/>.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        #endregion Methods
    }
}