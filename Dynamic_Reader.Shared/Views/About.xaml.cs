using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Dynamic_Reader.Views
{
    public sealed partial class About : SettingsFlyout
    {
        public About()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string AppVersion
        {
            get
            {
                PackageVersion ver = Package.Current.Id.Version;
                return ver.Major + "." + ver.Minor + "." + ver.Build;
            }
        }

        private async void Feedback_Click(object sender, TappedRoutedEventArgs e)
        {
            var mailto = new Uri("mailto:?to=robledo@gmail.comcom&subject=Dynamic Reader Feedback");
            await Launcher.LaunchUriAsync(mailto);
        }
    }
}