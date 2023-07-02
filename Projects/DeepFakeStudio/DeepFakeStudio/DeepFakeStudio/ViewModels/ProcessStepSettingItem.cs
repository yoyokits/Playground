namespace DeepFakeStudio.ViewModels
{
    using System.Collections.Generic;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="ProcessStepSettingItem" />.
    /// Translate settings to command line inputs like GPU or option CPU selection.
    /// </summary>
    public class ProcessStepSettingItem : NotifyPropertyChanged
    {
        #region Fields

        private string _selectedOption;

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
            get { return _selectedOption; }
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}