using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Http.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class UpdateWindow : Window
    {
        private RadioButton _rdNormal;
        private RadioButton _rdFull;
        private CheckBox _chkReload;
        private Button _btnStartUpdate;
        private Button _btnSelectMultiSports;

        private readonly ILTC2HttpProxy _ltc2HttpProxy;
        private readonly WebViewConnector _webViewConnector;
        private readonly MultiSportManager _multiSportManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ISelectActivitiesWindowFactory _selectActivitiesWindowFactory;

        public UpdateWindow()
        {
        }

        public UpdateWindow(
            ILTC2HttpProxy ltc2HttpProxy,
            WebViewConnector webViewConnector,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService,
            ISelectActivitiesWindowFactory selectActivitiesWindowFactory) : this()
        {
            _ltc2HttpProxy = ltc2HttpProxy;
            _webViewConnector = webViewConnector;
            _multiSportManager = multiSportManager;
            _translationService = translationService;
            _selectActivitiesWindowFactory = selectActivitiesWindowFactory;

            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            _rdNormal = this.FindControl<RadioButton>("RdNormal");
            _rdFull = this.FindControl<RadioButton>("RdFull");
            _chkReload = this.FindControl<CheckBox>("ChkReload");
            _btnStartUpdate = this.FindControl<Button>("BtnStartUpdate");
            _btnSelectMultiSports = this.FindControl<Button>("BtnSelectMultiSports");

            TryTranslate(this, "title.update.window");
            TryTranslate(_rdNormal, "radiobutton.update.normal");
            TryTranslate(_rdFull, "radiobutton.update.full");
            TryTranslate(_chkReload, "checkbox.update.reload");
            TryTranslate(_btnStartUpdate, "button.update.start");
            TryTranslate(_btnSelectMultiSports, "button.update.selectmultisports");

            var lblUpdateType = this.FindControl<TextBlock>("LblUpdateType");
            TryTranslate(lblUpdateType, "label.update.type");
        }

        private void TryTranslate(Control control, string key)
        {
            var message = _translationService.GetMessage(key);

            if (control is TextBlock textBlock) textBlock.Text = message;
            else if (control is Button button) button.Content = message;
            else if (control is RadioButton rb) rb.Content = message;
            else if (control is CheckBox cb) cb.Content = message;
            else if (control is Window window) window.Title = message;
        }

        public new void Show(Window owner)
        {
            AdaptFormToMultiSport();
            ShowDialog(owner);
        }

        private void AdaptFormToMultiSport()
        {
            if (_multiSportManager.RunInMultiSportMode)
            {
                _btnSelectMultiSports.IsVisible = true;
            }
        }

        private async Task<bool> IntermediateCheck(string token)
        {
            var hasIntermediate = await _ltc2HttpProxy.HasIntermediateResult(token, _multiSportManager.RunInMultiSportMode);

            if (hasIntermediate)
            {
                _rdNormal.IsChecked = true;
                _rdFull.IsEnabled = false;
            }

            return hasIntermediate;
        }

        public async void ClickHandlerStartUpdate(object sender, RoutedEventArgs e)
        {
            _btnStartUpdate.IsEnabled = false;

            var token = await _webViewConnector.Login();

            if (token != null)
            {
                await IntermediateCheck(token);

                var refresh = _rdNormal.IsChecked == true;
                var reload = _chkReload.IsChecked == true;

                if (_multiSportManager.RunInMultiSportMode)
                {
                    if (_multiSportManager.RunWithSource == "ridewithgps")
                    {
                        await _ltc2HttpProxy.UpdateMulti(
                            token,
                            new List<int>(),
                            _multiSportManager.CurrentRwGpsActivityTypes,
                            refresh,
                            false,
                            false,
                            reload,
                            _multiSportManager.RunWithSource);
                    }
                    else
                    {
                        var types = new List<int>();

                        foreach (var type in _multiSportManager.CurrentActivityTypes)
                        {
                            types.Add((int)type);
                        }

                        await _ltc2HttpProxy.UpdateMulti(
                            token,
                            types,
                            new List<string>(),
                            refresh,
                            false,
                            false,
                            reload,
                            _multiSportManager.RunWithSource);
                    }
                }
                else
                {
                    await _ltc2HttpProxy.Update(token, refresh, false, false, reload, _multiSportManager.RunWithSource);
                }
            }

            Dispatcher.UIThread.Post(() =>
            {
                _btnStartUpdate.IsEnabled = true;
                Close();
            });
        }

        public async void ClickHandlerSelectMultiSports(object sender, RoutedEventArgs e)
        {
            var selectActivitiesWindow = _selectActivitiesWindowFactory.Create();
            await selectActivitiesWindow.ShowDialog(this);
        }
    }
}
