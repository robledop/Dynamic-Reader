using Windows.UI.Xaml.Controls;


namespace Dynamic_Reader.Controls
{
	public sealed partial class SettingsControl
	{
		public SettingsControl()
		{
			InitializeComponent();
		}

		private void Orp_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			var toggleSwitch = (ToggleSwitch)sender;
			if (!toggleSwitch.IsOn)
			{
				DisplayGuidesToggleSwitch.IsOn = false;
			}
		}
	}
}
