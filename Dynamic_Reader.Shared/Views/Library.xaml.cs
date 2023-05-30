using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Dynamic_Reader.Model;

#if !WINDOWS_PHONE_APP
using Windows.ApplicationModel.Search;
#endif

namespace Dynamic_Reader.Views
{
	public sealed partial class Library
	{
		private string _searchText;

		public static Library Current;

		public Library()
		{
			InitializeComponent();
			Current = this;
		}

		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((GridView)sender).SelectedItem != null)
			{
				if (BottomAppBar != null) BottomAppBar.IsOpen = true;
			}
			else
			{
				if (BottomAppBar != null) BottomAppBar.IsOpen = false;
			}
		}
#if WINDOWS_PHONE_APP
		private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			var selectedBook = (Book)args.SelectedItem;
			App.MainViewModel.SelectedBooks = new ObservableCollection<object>(new[]{selectedBook});
			App.MainViewModel.OpenBookCommand.Execute(null);
		}
#endif

#if !WINDOWS_PHONE_APP
		private void MySearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
		{
			Frame.Navigate(typeof (SearchResultsPage), args.QueryText);
		}


		private void MySearchBox_SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
		{
			string queryText = args.QueryText.ToLower();


			if (!string.IsNullOrEmpty(queryText) && _searchText != queryText)
			{
				_searchText = queryText;
				IEnumerable<string> suggestions = (from b in App.MainViewModel.Books
					where b.Title.ToLower().Contains(queryText)
					select b.Title).Take(5);

				SearchSuggestionCollection suggestionCollection = args.Request.SearchSuggestionCollection;

				suggestionCollection.AppendQuerySuggestions(suggestions);
			}
		}
#endif


		
	}
}