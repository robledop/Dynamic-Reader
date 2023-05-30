using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Dynamic_Reader.Model;
using Dynamic_Reader.Readers;
using Dynamic_Reader.ViewModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Dynamic_Reader.Controls
{
	public sealed partial class TableOfContentsControl : UserControl
	{
		public TableOfContentsControl()
		{
			this.InitializeComponent();
		}

		private async void ListBox_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var bookViewModel = (BookViewModel)DataContext;
			var chapter = ((ListBox)sender).SelectedItem as Chapter;

			if (chapter != null)
			{
				await bookViewModel.GoToChapter(chapter);
				//Hide();
			}
		}
	}
}
