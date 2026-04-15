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

        /// <summary>Height for the Visible CollectionView: 46 dp × item count, min 48 dp.</summary>
        public double VisibleListHeight   => Math.Max(48.0, _visibleOverlayItems.Count   * 46.0);

        /// <summary>Height for the Available CollectionView: 46 dp × item count, min 48 dp.</summary>
        public double AvailableListHeight => Math.Max(48.0, _availableOverlayItems.Count * 46.0);

        #endregion

        #region Commands

        public ICommand MoveToVisibleCommand { get; }
        public ICommand MoveToAvailableCommand { get; }

        #endregion

        #region Constructor

        public OverlaySettingsViewModel()
        {
            MoveToVisibleCommand   = new Command<OverlayItem>(MoveToVisible);
            MoveToAvailableCommand = new Command<OverlayItem>(MoveToAvailable);

            // Notify height properties whenever either list changes
            _visibleOverlayItems.CollectionChanged   += (_, _) => OnPropertyChanged(nameof(VisibleListHeight));
            _availableOverlayItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(AvailableListHeight));
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
        /// Writes visibility state and order back to the source list.
        /// Visible items come first (in the order the user set in settings),
        /// followed by available items.
        /// Called when the settings overlay closes.
        /// </summary>
        public void ApplyToOverlayItems(ObservableCollection<OverlayItem> source)
        {
            // 1. Update IsVisible flags
            var visibleNames = new HashSet<string>(VisibleOverlayItems.Select(i => i.Name));
            foreach (var item in source)
                item.IsVisible = visibleNames.Contains(item.Name);

            // 2. Reorder source to match: visible items first (settings order), then available.
            //    Use Move() so bindings receive targeted CollectionChanged notifications
            //    rather than a full reset.
            var orderedItems = VisibleOverlayItems.Concat(AvailableOverlayItems).ToList();
            for (int i = 0; i < orderedItems.Count; i++)
            {
                var target = orderedItems[i];
                // Search from position i onward (everything before i is already in place)
                for (int j = i; j < source.Count; j++)
                {
                    if (source[j].Name == target.Name)
                    {
                        if (j != i) source.Move(j, i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Saves current settings to persistent storage.
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            try
            {
                // Visible items first (in settings order), then available — preserves display order.
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

                // Pass FontSize so the unified CekliCamSettings.json includes it.
                await SettingsHelper.SaveOverlayItemsConfigurationAsync(allItems, FontSize);
                System.Diagnostics.Debug.WriteLine("[OverlaySettingsViewModel] Settings saved to CekliCamSettings.json");
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
