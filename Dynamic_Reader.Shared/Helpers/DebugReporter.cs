using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Dynamic_Reader.Helpers
{
	public static class DebugReporter
	{
		public static async Task ReportError(Exception exception, [CallerMemberName] string method = "")
		{
			Debug.WriteLine("Error on method " + method + ": " + exception.Message);   
			var msg = new MessageDialog(method + ": " + exception.Message);
			await msg.ShowAsync();
		}
	}
}
