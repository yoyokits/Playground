using TravelCamApp.ViewModels;
using TravelCamApp.Models;

namespace TravelCamApp.Views
{
    public partial class SensorValueSettingsView : ContentView
    {
        private SensorValueSettingsViewModel? _viewModel;

        public SensorValueSettingsView()
        {
            InitializeComponent();
            _viewModel = new SensorValueSettingsViewModel();
            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Created with ViewModel");
        }

        private SensorValueSettingsViewModel ViewModel
        {
            get
            {
                if (_viewModel == null && BindingContext is SensorValueSettingsViewModel vm)
                {
                    _viewModel = vm;
                }
                return _viewModel ?? throw new InvalidOperationException("ViewModel is not set");
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext is SensorValueSettingsViewModel viewModel)
            {
                _viewModel = viewModel;
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] BindingContext changed to SensorValueSettingsViewModel");
            }
        }

        private void OnVisibleListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as SensorItem;
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Visible list selection changed: {0}", selectedItem?.Name ?? "NULL");
            
            // Clear available selection when visible is selected
            if (selectedItem != null)
            {
                AvailableSensorsList.SelectedItem = null;
            }
        }

        private void OnAvailableListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as SensorItem;
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Available list selection changed: {0}", selectedItem?.Name ?? "NULL");
            
            // Clear visible selection when available is selected
            if (selectedItem != null)
            {
                VisibleSensorsList.SelectedItem = null;
            }
        }

        private void OnAddButtonClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Add button clicked");
            var selectedItem = AvailableSensorsList.SelectedItem as SensorItem;
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] SelectedAvailableItem from list: {0}", selectedItem?.Name ?? "NULL");
            
            if (selectedItem != null)
            {
                ViewModel.MoveToVisible(selectedItem);
                AvailableSensorsList.SelectedItem = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] No item selected to add");
            }
        }

        private void OnRemoveButtonClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Remove button clicked");
            var selectedItem = VisibleSensorsList.SelectedItem as SensorItem;
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] SelectedVisibleItem from list: {0}", selectedItem?.Name ?? "NULL");
            
            if (selectedItem != null)
            {
                ViewModel.MoveToAvailable(selectedItem);
                VisibleSensorsList.SelectedItem = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] No item selected to remove");
            }
        }

        private void OnVisibleListReorderCompleted(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Reorder completed");
            
            // Log the new order
            var items = ViewModel.VisibleSensorItems;
            for (int i = 0; i < items.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Item {0}: {1}", i, items[i].Name);
            }
        }

        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Save button clicked");
            await ViewModel.SaveSettingsAsync();
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Settings saved");
        }
    }
}