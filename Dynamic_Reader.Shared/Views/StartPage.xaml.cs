using Windows.System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls.Primitives;
using System;
using Dynamic_Reader.Services;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Microsoft.Practices.ServiceLocation;
#if WINDOWS_PHONE_APP
using Dynamic_Reader.WindowsPhone.Common;
#endif

namespace Dynamic_Reader.Views
{
#if WINDOWS_PHONE_APP
	public sealed partial class StartPage : StorageFileOpenPickerContinuable
#else
	public sealed partial class StartPage
#endif
	{
		private readonly IImporter _importer;
		public static StartPage Current;

		public StartPage()
		{
			_importer = ServiceLocator.Current.GetInstance<IImporter>();
			InitializeComponent();
			Current = this;
		}



		private void BookList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((GridView)sender).SelectedItem != null)
			{
				if (BottomAppBar != null) BottomAppBar.IsOpen = true;
			}
			else
			{
				if (BottomAppBar != null) BottomAppBar.IsOpen = false;
			}
			App.MainViewModel.OpenBookCommand.RaiseCanExecuteChanged();
		}
#if WINDOWS_PHONE_APP
		public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
		{
			await _importer.ContinueImportBooksAsync(args.Files);
			await App.MainViewModel.LoadBooks();
		}
#endif


		private void BookItem_OnHolding(object sender, HoldingRoutedEventArgs e)
		{
			DisplayContextMenu(sender, e);
		}

		private void BookItem_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			DisplayContextMenu(sender, e);
		}

		private void DisplayContextMenu(object sender, RoutedEventArgs e)
		{
			FrameworkElement senderElement = sender as FrameworkElement;
			var frameworkElement = e.OriginalSource as FrameworkElement;
			if (frameworkElement != null)
			{
				var selectedBook = (Book)frameworkElement.DataContext;
				SetSelectedBook(selectedBook);
				FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
				flyoutBase.ShowAt(senderElement);
			}
		}

		private void SetSelectedBook(Book book)
		{
			App.MainViewModel.SelectedBooks.Clear();
			App.MainViewModel.SelectedBooks.Add(book);
		}

		private void BookItem_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			var tappedItem = e.OriginalSource as FrameworkElement;
			if (tappedItem != null)
			{
				var selectedBook = (Book) tappedItem.DataContext;
				SetSelectedBook(selectedBook);
				App.MainViewModel.OpenBookCommand.Execute(null);
			}
		}

		private void Gutenberg_OnClick(object sender, RoutedEventArgs e)
		{
			Launcher.LaunchUriAsync(new System.Uri("http://m.gutenberg.org",UriKind.Absolute));
		}
	}
}