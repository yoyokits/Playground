// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// SensorValueSettingsViewModel manages two lists:
// - VisibleSensorItems: shown on the camera overlay
// - AvailableSensorItems: hidden sensors the user can enable
//
// The ViewModel receives sensor items from MainPageViewModel,
// lets the user reorder/enable/disable, then MainPageViewModel
// pulls the result back.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelCamApp.Helpers;
using TravelCamApp.Models;

namespace TravelCamApp.ViewModels
{
    public class SensorValueSettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<SensorItem> _visibleSensorItems = new();
        private ObservableCollection<SensorItem> _availableSensorItems = new();
        private SensorItem? _selectedVisibleItem;
        private SensorItem? _selectedAvailableItem;
        private float _fontSize = 12f;
        private bool _isMapOverlayVisible;

        // Reference to the source list in MainPageViewModel
        private List<SensorItem>? _allSensorItems;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SensorItem> VisibleSensorItems
        {
            get => _visibleSensorItems;
            set { _visibleSensorItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SensorItem> AvailableSensorItems
        {
            get => _availableSensorItems;
            set { _availableSensorItems = value; OnPropertyChanged(); }
        }

        public SensorItem? SelectedVisibleItem
        {
            get => _selectedVisibleItem;
            set { _selectedVisibleItem = value; OnPropertyChanged(); }
        }

        public SensorItem? SelectedAvailableItem
        {
            get => _selectedAvailableItem;
            set { _selectedAvailableItem = value; OnPropertyChanged(); }
        }

        public float FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); }
        }

        public bool IsMapOverlayVisible
        {
            get => _isMapOverlayVisible;
            set { _isMapOverlayVisible = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand MoveToVisibleCommand { get; }
        public ICommand MoveToAvailableCommand { get; }

        #endregion

        #region Constructor

        public SensorValueSettingsViewModel()
        {
            MoveToVisibleCommand = new Command<SensorItem>(MoveToVisible);
            MoveToAvailableCommand = new Command<SensorItem>(MoveToAvailable);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Populates the visible/available lists from the source sensor items.
        /// Called when the settings overlay opens.
        /// </summary>
        public void LoadFromSensorItems(ObservableCollection<SensorItem> source)
        {
            _allSensorItems = source.ToList(); // shallow copy of references
            VisibleSensorItems.Clear();
            AvailableSensorItems.Clear();

            foreach (var item in _allSensorItems)
            {
                if (item.IsVisible)
                    VisibleSensorItems.Add(item);
                else
                    AvailableSensorItems.Add(item);
            }
        }

        /// <summary>
        /// Writes the visibility state back to the source list.
        /// Called when the settings overlay closes.
        /// </summary>
        public void ApplyToSensorItems(ObservableCollection<SensorItem> source)
        {
            var visibleNames = new HashSet<string>(VisibleSensorItems.Select(i => i.Name));

            foreach (var item in source)
            {
                item.IsVisible = visibleNames.Contains(item.Name);
            }
        }

        /// <summary>
        /// Saves current settings to persistent storage.
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            try
            {
                await SettingsHelper.SaveSensorItemsConfigurationAsync(VisibleSensorItems);
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsViewModel] Settings saved");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SensorValueSettingsViewModel] Save error: {ex.Message}");
            }
        }

        #endregion

        #region Commands

        public async Task MoveToVisibleAsync(SensorItem item)
        {
            if (item == null) return;

            if (AvailableSensorItems.Contains(item))
            {
                AvailableSensorItems.Remove(item);
            }

            if (!VisibleSensorItems.Contains(item))
            {
                VisibleSensorItems.Add(item);
            }

            SelectedAvailableItem = null;
        }

        public void MoveToAvailable(SensorItem item)
        {
            if (item == null) return;

            if (VisibleSensorItems.Contains(item))
            {
                VisibleSensorItems.Remove(item);
            }

            if (!AvailableSensorItems.Contains(item))
            {
                AvailableSensorItems.Add(item);
            }

            SelectedVisibleItem = null;
        }

        #endregion

        #region Event Handlers

        private void OnVisibleListReorderCompleted(object? sender, EventArgs e)
        {
            // The order is already reflected in the collection.
            // When settings are saved, the current order is persisted.
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsViewModel] Reorder completed");
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
