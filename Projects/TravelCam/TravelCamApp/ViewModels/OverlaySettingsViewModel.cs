// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// OverlaySettingsViewModel manages two lists:
// - VisibleOverlayItems: shown on the camera overlay
// - AvailableOverlayItems: hidden sensors the user can enable
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
    public class OverlaySettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<OverlayItem> _visibleOverlayItems = new();
        private ObservableCollection<OverlayItem> _availableOverlayItems = new();
        private OverlayItem? _selectedVisibleItem;
        private OverlayItem? _selectedAvailableItem;
        private float _fontSize = 12f;
        private bool _isMapOverlayVisible;

        // Reference to the source list in MainPageViewModel
        private List<OverlayItem>? _allOverlayItems;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<OverlayItem> VisibleOverlayItems
        {
            get => _visibleOverlayItems;
            set { _visibleOverlayItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OverlayItem> AvailableOverlayItems
        {
            get => _availableOverlayItems;
            set { _availableOverlayItems = value; OnPropertyChanged(); }
        }

        public OverlayItem? SelectedVisibleItem
        {
            get => _selectedVisibleItem;
            set { _selectedVisibleItem = value; OnPropertyChanged(); }
        }

        public OverlayItem? SelectedAvailableItem
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

        public OverlaySettingsViewModel()
        {
            MoveToVisibleCommand = new Command<OverlayItem>(MoveToVisible);
            MoveToAvailableCommand = new Command<OverlayItem>(MoveToAvailable);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Populates the visible/available lists from the source sensor items.
        /// Called when the settings overlay opens.
        /// </summary>
        public void LoadFromOverlayItems(ObservableCollection<OverlayItem> source)
        {
            _allOverlayItems = source.ToList(); // shallow copy of references
            VisibleOverlayItems.Clear();
            AvailableOverlayItems.Clear();

            foreach (var item in _allOverlayItems)
            {
                if (item.IsVisible)
                    VisibleOverlayItems.Add(item);
                else
                    AvailableOverlayItems.Add(item);
            }
        }

        /// <summary>
        /// Writes the visibility state back to the source list.
        /// Called when the settings overlay closes.
        /// </summary>
        public void ApplyToOverlayItems(ObservableCollection<OverlayItem> source)
        {
            var visibleNames = new HashSet<string>(VisibleOverlayItems.Select(i => i.Name));

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
                // Save ALL items (visible + available) so load preserves the full list
                var allItems = new List<OverlayItem>();
                foreach (var item in VisibleOverlayItems)
                {
                    item.IsVisible = true;
                    allItems.Add(item);
                }
                foreach (var item in AvailableOverlayItems)
                {
                    item.IsVisible = false;
                    allItems.Add(item);
                }

                await SettingsHelper.SaveOverlayItemsConfigurationAsync(allItems);
                System.Diagnostics.Debug.WriteLine("[OverlaySettingsViewModel] Settings saved");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OverlaySettingsViewModel] Save error: {ex.Message}");
            }
        }

        #endregion

        #region Move Operations

        public void MoveToVisible(OverlayItem item)
        {
            if (item == null) return;

            if (AvailableOverlayItems.Contains(item))
            {
                AvailableOverlayItems.Remove(item);
            }

            if (!VisibleOverlayItems.Contains(item))
            {
                VisibleOverlayItems.Add(item);
            }

            SelectedAvailableItem = null;
        }

        public void MoveToAvailable(OverlayItem item)
        {
            if (item == null) return;

            if (VisibleOverlayItems.Contains(item))
            {
                VisibleOverlayItems.Remove(item);
            }

            if (!AvailableOverlayItems.Contains(item))
            {
                AvailableOverlayItems.Add(item);
            }

            SelectedVisibleItem = null;
        }

        #endregion

        #region Event Handlers

        private void OnVisibleListReorderCompleted(object? sender, EventArgs e)
        {
            // The order is already reflected in the collection.
            // When settings are saved, the current order is persisted.
            System.Diagnostics.Debug.WriteLine("[OverlaySettingsViewModel] Reorder completed");
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
