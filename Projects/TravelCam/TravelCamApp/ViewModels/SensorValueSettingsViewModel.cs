// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelCamApp.Models;

namespace TravelCamApp.ViewModels
{
    public class SensorValueSettingsViewModel
    {
        #region Fields

        // Additional properties for font size and map overlay
        private double _fontSize = 14.0;

        private bool _isMapOverlayVisible = false;

        #endregion Fields

        #region Constructors

        public SensorValueSettingsViewModel()
        {
            AvailableSensorItems = new ObservableCollection<SensorItem>();
            VisibleSensorItems = new ObservableCollection<SensorItem>();

            // Initialize with default sensor items
            InitializeDefaultSensorItems();

            // Initialize commands
            MoveToVisibleCommand = new Command<SensorItem>(MoveToVisible);
            MoveToAvailableCommand = new Command<SensorItem>(MoveToAvailable);
            MoveUpCommand = new Command<SensorItem>(MoveUp);
            MoveDownCommand = new Command<SensorItem>(MoveDown);
            SaveSettingsCommand = new Command(async () => await ExecuteSaveSettingsAsync());
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Events

        #region Properties

        public ObservableCollection<SensorItem> AvailableSensorItems { get; }

        private SensorItem? _selectedAvailableItem;
        private SensorItem? _selectedVisibleItem;

        public SensorItem? SelectedAvailableItem
        {
            get => _selectedAvailableItem;
            set
            {
                if (_selectedAvailableItem != value)
                {
                    _selectedAvailableItem = value;
                    System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] SelectedAvailableItem changed to: {value?.Name ?? "NULL"}");
                    OnPropertyChanged();
                }
            }
        }

        public SensorItem? SelectedVisibleItem
        {
            get => _selectedVisibleItem;
            set
            {
                if (_selectedVisibleItem != value)
                {
                    _selectedVisibleItem = value;
                    System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] SelectedVisibleItem changed to: {value?.Name ?? "NULL"}");
                    OnPropertyChanged();
                }
            }
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMapOverlayVisible
        {
            get => _isMapOverlayVisible;
            set
            {
                if (_isMapOverlayVisible != value)
                {
                    _isMapOverlayVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand MoveToAvailableCommand { get; }
        public ICommand MoveToVisibleCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ObservableCollection<SensorItem> VisibleSensorItems { get; }

        #endregion Properties

        #region Methods

        public async Task LoadSettingsAsync()
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsVM] LoadSettingsAsync called");
            
            // Clear existing items first to prevent duplicates
            AvailableSensorItems.Clear();
            VisibleSensorItems.Clear();
            
            var config = await Helpers.SettingsHelper.LoadSensorItemsConfigurationAsync();
            var additionalSettings = await Helpers.SettingsHelper.LoadAdditionalSettingsAsync();

            if (config != null && config.Items != null && config.Items.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsVM] Loaded config with {0} items", config.Items.Count);
                // Apply loaded configuration to both collections
                Helpers.SettingsHelper.ApplyConfigurationToSensorItemsCollections(AvailableSensorItems, VisibleSensorItems, config);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsVM] No config found, using defaults");
                // If no config exists, initialize with defaults
                InitializeDefaultSensorItems();
            }

            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsVM] After load - Available: {0}, Visible: {1}", 
                AvailableSensorItems.Count, VisibleSensorItems.Count);

            // Apply additional settings if they exist
            if (additionalSettings != null)
            {
                FontSize = additionalSettings.FontSize;
                IsMapOverlayVisible = additionalSettings.IsMapOverlayVisible;
            }
            else
            {
                // Set defaults if no additional settings exist
                FontSize = 14.0;
                IsMapOverlayVisible = false;
            }
        }

        public void MoveToAvailable(SensorItem item)
        {
            System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] MoveToAvailable called. Item: {item?.Name ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] SelectedVisibleItem: {SelectedVisibleItem?.Name ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] VisibleSensorItems count before: {VisibleSensorItems.Count}");
            
            // If item is null, try to use SelectedVisibleItem
            var itemToMove = item ?? SelectedVisibleItem;
            
            if (itemToMove == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] MoveToAvailable - No item to move");
                return;
            }
            
            // Check if item is in visible list
            var existingInVisible = VisibleSensorItems.FirstOrDefault(x => x.Name == itemToMove.Name);
            if (existingInVisible != null)
            {
                VisibleSensorItems.Remove(existingInVisible);
                existingInVisible.IsVisible = false;
                
                // Check if already in available list (prevent duplicates)
                if (!AvailableSensorItems.Any(x => x.Name == existingInVisible.Name))
                {
                    AvailableSensorItems.Add(existingInVisible);
                }
                
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] Moved {existingInVisible.Name} to available list");
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] VisibleSensorItems count after: {VisibleSensorItems.Count}");
                
                // Clear selection
                SelectedVisibleItem = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] Item {itemToMove.Name} not found in VisibleSensorItems");
            }
        }

        public void MoveUp(SensorItem item)
        {
            var index = VisibleSensorItems.IndexOf(item);
            if (index > 0)
            {
                VisibleSensorItems.Move(index, index - 1);
            }
        }

        public void MoveDown(SensorItem item)
        {
            var index = VisibleSensorItems.IndexOf(item);
            if (index < VisibleSensorItems.Count - 1)
            {
                VisibleSensorItems.Move(index, index + 1);
            }
        }

        public void MoveToVisible(SensorItem item)
        {
            System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] MoveToVisible called. Item: {item?.Name ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] SelectedAvailableItem: {SelectedAvailableItem?.Name ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] AvailableSensorItems count before: {AvailableSensorItems.Count}");
            
            // If item is null, try to use SelectedAvailableItem
            var itemToMove = item ?? SelectedAvailableItem;
            
            if (itemToMove == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] MoveToVisible - No item to move");
                return;
            }
            
            // Check if item is in available list
            var existingInAvailable = AvailableSensorItems.FirstOrDefault(x => x.Name == itemToMove.Name);
            if (existingInAvailable != null)
            {
                AvailableSensorItems.Remove(existingInAvailable);
                existingInAvailable.IsVisible = true;
                
                // Check if already in visible list (prevent duplicates)
                if (!VisibleSensorItems.Any(x => x.Name == existingInAvailable.Name))
                {
                    VisibleSensorItems.Add(existingInAvailable);
                }
                
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] Moved {existingInAvailable.Name} to visible list");
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] VisibleSensorItems count after: {VisibleSensorItems.Count}");
                
                // Clear selection
                SelectedAvailableItem = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsVM] Item {itemToMove.Name} not found in AvailableSensorItems");
            }
        }

        public async Task SaveSettingsAsync()
        {
            // Save both sensor items configuration and additional settings
            await Helpers.SettingsHelper.SaveSensorItemsConfigurationAsync(VisibleSensorItems);
            await Helpers.SettingsHelper.SaveAdditionalSettingsAsync(FontSize, IsMapOverlayVisible);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task ExecuteSaveSettingsAsync()
        {
            await SaveSettingsAsync();
        }

        private void InitializeDefaultSensorItems()
        {
            // Define all possible sensor items
            var allSensorItems = new[]
            {
                new SensorItem("City", "Jakarta", false),
                new SensorItem("Country", "Indonesia", false),
                new SensorItem("Temperature", "28°C", false),
                new SensorItem("Altitude", "12m", false),
                new SensorItem("Latitude", "-6.2088", false),
                new SensorItem("Longitude", "106.8456", false),
                new SensorItem("Date", DateTime.Now.ToString("MM/dd/yyyy"), false),
                new SensorItem("Time", DateTime.Now.ToString("HH:mm:ss"), false),
                new SensorItem("Heading", "0°", false),
                new SensorItem("Speed", "0 m/s", false),
                new SensorItem("Map", "Map View", false) // Add map overlay option
            };

            // Set default visible items
            var defaultVisibleNames = new[] { "City", "Country", "Temperature" };

            foreach (var item in allSensorItems)
            {
                if (defaultVisibleNames.Contains(item.Name))
                {
                    item.IsVisible = true;
                    VisibleSensorItems.Add(item);
                }
                else
                {
                    item.IsVisible = false;
                    AvailableSensorItems.Add(item);
                }
            }
        }

        #endregion Methods
    }
}