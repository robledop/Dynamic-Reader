using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Dynamic_Reader.Interfaces;
using GalaSoft.MvvmLight.Threading;

namespace Dynamic_Reader.Services
{
	public class DialogService : IDialogService
	{
		public async Task ShowError(string message, string title, string buttonText,
			 Action afterHideCallback)
		{
			if (message == null) throw new ArgumentNullException("message");
			if (buttonText == null) throw new ArgumentNullException("buttonText");
			var dialog = new MessageDialog(message, title ?? string.Empty);

			dialog.Commands.Add(
				new UICommand(
					buttonText,
					c =>
					{
						if (afterHideCallback != null)
						{
							afterHideCallback();
						}
					}));

			dialog.CancelCommandIndex = 0;
			await dialog.ShowAsync();
		}

		public async Task ShowError(Exception error, string title, string buttonText,
			 Action afterHideCallback)
		{
			if (error == null) throw new ArgumentNullException("error");
			if (buttonText == null) throw new ArgumentNullException("buttonText");
			await ShowError(error.Message, title ?? string.Empty, buttonText, afterHideCallback);
		}

		public async Task ShowMessage(string message, string title)
		{
			if (message == null) throw new ArgumentNullException("message");
			var dialog = new MessageDialog(message, title ?? string.Empty);
			DispatcherHelper.CheckBeginInvokeOnUI(async() =>
			{
				await dialog.ShowAsync();
			});
		}

		public async Task ShowMessage(string message, string title, string buttonText,
			 Action afterHideCallback)
		{
			if (message == null) throw new ArgumentNullException("message");
			if (buttonText == null) throw new ArgumentNullException("buttonText");
			var dialog = new MessageDialog(message, title ?? string.Empty);
			dialog.Commands.Add(
				new UICommand(
					buttonText,
					c =>
					{
						if (afterHideCallback != null)
						{
							afterHideCallback();
						}
					}));
			dialog.CancelCommandIndex = 0;
			await dialog.ShowAsync();
		}

		public async void ShowPopUp(string message, string title)
		{
			var msg = new MessageDialog(message);
			await msg.ShowAsync();
		}

		public async Task ShowMessage(string message, string title,
			 string buttonConfirmText, string buttonCancelText,
			 Action<bool> afterHideCallback)
		{
			if (message == null) throw new ArgumentNullException("message");
			if (buttonConfirmText == null) throw new ArgumentNullException("buttonConfirmText");
			if (buttonCancelText == null) throw new ArgumentNullException("buttonCancelText");
			if (afterHideCallback == null) throw new ArgumentNullException("afterHideCallback");
			var dialog = new MessageDialog(message, title ?? string.Empty);
			dialog.Commands.Add(new UICommand(buttonConfirmText, c => afterHideCallback(true)));
			dialog.Commands.Add(new UICommand(buttonCancelText, c => afterHideCallback(false)));
			dialog.CancelCommandIndex = 1;
			await dialog.ShowAsync();
		}

		public async Task ShowMessageBox(string message, string title)
		{
			if (message == null) throw new ArgumentNullException("message");
			var dialog = new MessageDialog(message, title ?? string.Empty);
			await dialog.ShowAsync();
		}
	}
}