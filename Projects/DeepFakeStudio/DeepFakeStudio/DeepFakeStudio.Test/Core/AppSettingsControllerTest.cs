namespace DeepFakeStudio.Test.Core
{
    using System.Windows;
    using DeepFakeStudio.Core;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="AppSettingsControllerTest" />.
    /// </summary>
    [TestClass]
    public class AppSettingsControllerTest
    {
        #region Methods

        /// <summary>
        /// The Load.
        /// </summary>
        [TestMethod]
        public void Load()
        {
            var controller = new AppSettingsController();
            var settings = controller.AppSettings;
            settings.WindowWidth = 800;
            settings.WindowHeight = 600;
            settings.WindowState = WindowState.Maximized;
            controller.Save();
            controller.AppSettings = new AppSettings();
            controller.Load();

            settings = controller.AppSettings;
            settings.WindowWidth.Should().Be(800);
            settings.WindowHeight.Should().Be(600);
            settings.WindowState.Should().Be(WindowState.Maximized);
        }

        #endregion Methods
    }
}