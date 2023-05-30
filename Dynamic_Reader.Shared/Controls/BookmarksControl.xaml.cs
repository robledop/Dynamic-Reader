using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Dynamic_Reader.Model;
using Dynamic_Reader.Readers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Dynamic_Reader.Controls
{
	public sealed partial class BookmarksControl : UserControl
	{
		public BookmarksControl()
		{
			this.InitializeComponent();
		}

		private void Item_OnTap(object sender, TappedRoutedEventArgs e)
		{
			var selectedItem = (StackPanel)sender;
			App.BookViewModel.GoToBookmarkCommand.Execute((Bookmark)selectedItem.DataContext);
		}

		private async void Item_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var menu = new PopupMenu();
			menu.Commands.Add(new UICommand("Delete", (command) =>
			{
				var sp = (StackPanel)sender;
				var item = (Bookmark)sp.DataContext;

				App.BookViewModel.BookReader.CurrentBook.Bookmarks.Remove(item);
			}));

			await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));

		}

		private static Rect GetElementRect(FrameworkElement element)
		{
			GeneralTransform buttonTransform = element.TransformToVisual(null);
			Point point = buttonTransform.TransformPoint(new Point());
			return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
		}
	}
}
