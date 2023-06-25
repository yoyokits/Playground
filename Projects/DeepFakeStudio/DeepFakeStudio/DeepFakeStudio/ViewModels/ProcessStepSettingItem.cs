namespace DeepFakeStudio.ViewModels
{
    using System.Collections.Generic;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="ProcessStepSettingItem" />.
    /// </summary>
    public class ProcessStepSettingItem : NotifyPropertyChanged
    {
        #region Fields

        private string selectedOption;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the AvailableOptions.
        /// </summary>
        public IDictionary<string, string> AvailableOptions { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the SelectedOption.
        /// </summary>
        public string SelectedOption
        {
            get { return selectedOption; }
            set
            {
                if (selectedOption != value)
                {
                    selectedOption = value;
                    OnPropertyChanged(nameof(SelectedOption));
                }
            }
        }

        #endregion Properties
    }
}