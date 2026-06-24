using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LTC2.Shared.BaseMessages.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class AboutBoxWindow : Window
    {
        private TextBlock _lblAppName;
        private TextBlock _lblVersion;
        private TextBlock _lblCopyright;
        private Button _btnClose;

        private readonly IBaseTranslationService _translationService;

        public AboutBoxWindow()
        {
        }

        public AboutBoxWindow(IBaseTranslationService translationService) : this()
        {
            _translationService = translationService;

            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            _lblAppName = this.FindControl<TextBlock>("LblAppName");
            _lblVersion = this.FindControl<TextBlock>("LblVersion");
            _lblCopyright = this.FindControl<TextBlock>("LblCopyright");
            _btnClose = this.FindControl<Button>("BtnClose");

            TryTranslate(this, "title.about.window");
            TryTranslate(_btnClose, "button.about.close");

            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            if (_lblAppName != null) _lblAppName.Text = _translationService.GetMessage("label.about.appname");
            if (_lblVersion != null) _lblVersion.Text = $"{_translationService.GetMessage("label.about.version")} {version}";
            if (_lblCopyright != null) _lblCopyright.Text = _translationService.GetMessage("label.about.copyright");
        }

        private void TryTranslate(Control control, string key)
        {
            var message = _translationService.GetMessage(key);

            if (control is Button button) button.Content = message;
            else if (control is Window window) window.Title = message;
        }

        public void ClickHandlerClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
