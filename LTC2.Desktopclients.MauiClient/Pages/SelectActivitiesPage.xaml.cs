using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Domain;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiClient.Pages
{
    public class ActivitySelectionItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Value { get; set; }
        public string Description { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class SelectActivitiesPage : ContentPage
    {
        private readonly IBaseTranslationService _translationService;
        private readonly MultiSportManager _multiSportManager;
        private ObservableCollection<ActivitySelectionItem> _items;
        private readonly TaskCompletionSource<bool> _closeTcs = new TaskCompletionSource<bool>();

        public SelectActivitiesPage(IBaseTranslationService translationService, MultiSportManager multiSportManager)
        {
            _translationService = translationService;
            _multiSportManager = multiSportManager;

            InitializeComponent();

            Title = _translationService.GetMessage("title.select.multisport.window");
            BtnApply.Text = _translationService.GetMessage("button.select.activities.apply");
            BtnCancel.Text = _translationService.GetMessage("button.select.activities.cancel");

            PopulateList();
        }

        private void PopulateList()
        {
            _items = new ObservableCollection<ActivitySelectionItem>();

            if (_multiSportManager.RunWithSource == "ridewithgps")
            {
                var allTypes = _multiSportManager.GetRwGpsActivityTypes();
                var selectedTypes = _multiSportManager.CurrentRwGpsActivityTypes;
                foreach (var type in allTypes)
                    _items.Add(new ActivitySelectionItem { Value = type.Value, Description = type.Description, IsSelected = selectedTypes?.Contains(type.Value) ?? false });
            }
            else
            {
                var allTypes = _multiSportManager.GetActivityTypes();
                var selectedTypes = _multiSportManager.CurrentActivityTypes;
                foreach (var type in allTypes)
                    _items.Add(new ActivitySelectionItem { Value = type.Value, Description = type.Description, IsSelected = selectedTypes?.Contains(type.ActivityType) ?? false });
            }

            LstActivities.ItemsSource = _items;
        }

        public Task WaitForClose() => _closeTcs.Task;

        private async void OnApply(object sender, EventArgs e)
        {
            if (_multiSportManager.RunWithSource == "ridewithgps")
            {
                var selected = _items.Where(i => i.IsSelected).Select(i => i.Value).ToList();
                _multiSportManager.SaveRwGpsActivityTypesForAthlete(activityTypes: selected);
                _multiSportManager.CurrentRwGpsActivityTypes = selected;
            }
            else
            {
                var selected = _items.Where(i => i.IsSelected).Select(i => (GenericActivityType)int.Parse(i.Value)).ToList();
                _multiSportManager.SaveActivityTypesForAthlete(activityTypes: selected);
                _multiSportManager.CurrentActivityTypes = selected;
            }

            _closeTcs.TrySetResult(true);
            await Navigation.PopModalAsync();
        }

        private async void OnCancel(object sender, EventArgs e)
        {
            _closeTcs.TrySetResult(false);
            await Navigation.PopModalAsync();
        }
    }
}
