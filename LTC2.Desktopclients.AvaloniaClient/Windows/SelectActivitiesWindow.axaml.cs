using Avalonia.Controls;
using Avalonia.Interactivity;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Utilities;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Domain;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public class ActivitySelectionItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Value { get; set; }
        public string Description { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class SelectActivitiesWindow : Window
    {
        private ListBox _lstActivities;
        private Button _btnApply;
        private Button _btnCancel;

        private readonly IBaseTranslationService _translationService;
        private readonly MultiSportManager _multiSportManager;
        private readonly AppSettings _appSettings;

        private ObservableCollection<ActivitySelectionItem> _items;

        public SelectActivitiesWindow()
        {
        }

        public SelectActivitiesWindow(
            IBaseTranslationService translationService,
            MultiSportManager multiSportManager,
            AppSettings appSettings) : this()
        {
            _translationService = translationService;
            _multiSportManager = multiSportManager;
            _appSettings = appSettings;

            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            _lstActivities = this.FindControl<ListBox>("LstActivities");
            _btnApply = this.FindControl<Button>("BtnApply");
            _btnCancel = this.FindControl<Button>("BtnCancel");

            TryTranslate(this, "title.select.multisport.window");
            TryTranslate(_btnApply, "button.select.activities.apply");
            TryTranslate(_btnCancel, "button.select.activities.cancel");

            PopulateList();
        }

        private void TryTranslate(Control control, string key)
        {
            var message = _translationService.GetMessage(key);

            if (control is TextBlock textBlock) textBlock.Text = message;
            else if (control is Button button) button.Content = message;
            else if (control is Window window) window.Title = message;
        }

        private void PopulateList()
        {
            _items = new ObservableCollection<ActivitySelectionItem>();

            if (_multiSportManager.RunWithSource == "ridewithgps")
            {
                var allTypes = _multiSportManager.GetRwGpsActivityTypes();
                var selectedTypes = _multiSportManager.CurrentRwGpsActivityTypes;

                foreach (var type in allTypes)
                {
                    _items.Add(new ActivitySelectionItem
                    {
                        Value = type.Value,
                        Description = type.Description,
                        IsSelected = selectedTypes?.Contains(type.Value) ?? false
                    });
                }
            }
            else
            {
                var allTypes = _multiSportManager.GetActivityTypes();
                var selectedTypes = _multiSportManager.CurrentActivityTypes;

                foreach (var type in allTypes)
                {
                    _items.Add(new ActivitySelectionItem
                    {
                        Value = type.Value,
                        Description = type.Description,
                        IsSelected = selectedTypes?.Contains(type.ActivityType) ?? false
                    });
                }
            }

            _lstActivities.ItemsSource = _items;
        }

        public void ClickHandlerApply(object sender, RoutedEventArgs e)
        {
            if (_multiSportManager.RunWithSource == "ridewithgps")
            {
                var selectedTypes = _items
                    .Where(i => i.IsSelected)
                    .Select(i => i.Value)
                    .ToList();

                _multiSportManager.SaveRwGpsActivityTypesForAthlete(activityTypes: selectedTypes);
                _multiSportManager.CurrentRwGpsActivityTypes = selectedTypes;
            }
            else
            {
                var selectedTypes = _items
                    .Where(i => i.IsSelected)
                    .Select(i => (GenericActivityType)int.Parse(i.Value))
                    .ToList();

                _multiSportManager.SaveActivityTypesForAthlete(activityTypes: selectedTypes);
                _multiSportManager.CurrentActivityTypes = selectedTypes;
            }

            Close();
        }

        public void ClickHandlerCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            WindowUtilities.RemoveMinimizeButton(this);
        }
    }
}
