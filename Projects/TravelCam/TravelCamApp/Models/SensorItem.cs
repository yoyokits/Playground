using System;
using System.ComponentModel;

namespace TravelCamApp.Models
{
    /// <summary>
    /// Represents a single sensor value item for visualization
    /// </summary>
    public class SensorItem : INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private bool _isVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorItem"/> class.
        /// </summary>
        /// <param name="name">The name of the sensor (e.g., "Temperature", "City")</param>
        /// <param name="value">The current value of the sensor</param>
        /// <param name="isVisible">Whether the sensor item should be visible</param>
        public SensorItem(string name, string value, bool isVisible = true)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _value = value ?? "";
            _isVisible = isVisible;
        }

        /// <summary>
        /// Gets or sets the name of the sensor (e.g., "Temperature", "City")
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current value of the sensor
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the sensor item should be visible
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        /// <summary>
        /// Gets the combined name and value string for display
        /// </summary>
        public string NameAndValue => $"{Name}: {Value}";

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Notify dependent properties
            if (propertyName == nameof(Name) || propertyName == nameof(Value))
            {
                OnPropertyChanged(nameof(NameAndValue));
            }
        }
    }
}