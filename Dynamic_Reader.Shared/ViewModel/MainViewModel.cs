using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Dynamic_Reader.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;

namespace Dynamic_Reader.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		private readonly IImporter _bookImporter;
		private readonly DataService _dataService;
		private readonly IDialogService _dialogService;
		private readonly INavigationService _navigationService;
		private ObservableCollection<Book> _books;
		private ObservableCollection<BookToImport> _booksToImport;
		private bool _displayAdvertisements;
		private ObservableCollection<Book> _suggestions;

		public MainViewModel()
		{
			_navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
			_bookImporter = ServiceLocator.Current.GetInstance<IImporter>();
			_dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
			_dataService = new DataService();

			SelectedBooks = new ObservableCollection<object>();

			SetCommands();

			SelectedBooks.CollectionChanged += (s, e) =>
			{
				OpenBookCommand.RaiseCanExecuteChanged();
				DeleteBooksCommand.RaiseCanExecuteChanged();
			};

			StartLoading += MainViewModel_StartLoading;
			if (!IsDataLoaded)
			{
				OnStartLoading();
			}
		}

		private async void MainViewModel_StartLoading(object sender, EventArgs e)
		{
			await LoadData();
		}

		private event EventHandler StartLoading;

		public RelayCommand AboutCommand { get; private set; }

		public ObservableCollection<Book> Books
		{
			get { return _books; }
			set
			{
				_books = value;
				_books.CollectionChanged += (s, e) =>
				{
					RaisePropertyChanged(() => RecentBooks);
					RaisePropertyChanged(() => FavoriteBooks);
				};
				RaisePropertyChanged();
				RaisePropertyChanged(() => RecentBooks);
				RaisePropertyChanged(() => FavoriteBooks);
			}
		}

		public ObservableCollection<BookToImport> BooksToImport
		{
			get { return _booksToImport; }
			set
			{
				_booksToImport = value;
				RaisePropertyChanged();
			}
		}

		public IEnumerable<object> BooksByTitle
		{
			get
			{
				return from b in Books
					   orderby b.Title
					   group b by b.Title.Substring(0, 1)
						   into g
						   select g;
			}
		}

		public DataService DataService
		{
			get { return _dataService; }
		}


		public bool DisplayAdvertisements
		{
			get { return _displayAdvertisements; }
			set
			{
				_displayAdvertisements = value;
				RaisePropertyChanged();
				RemoveAdsCommand.RaiseCanExecuteChanged();
			}
		}

		public ObservableCollection<Book> FavoriteBooks
		{
			get
			{
				if (Books == null)
				{
					return new ObservableCollection<Book>();
				}

				var favoriteBooks = from b in Books
									where b.Favorited
									select b;
				return new ObservableCollection<Book>(favoriteBooks);
			}
		}

		public RelayCommand DeleteBooksCommand { get; private set; }
		public RelayCommand GoBackCommand { get; private set; }
		public RelayCommand ImportBooksCommand { get; private set; }
		public bool IsDataLoaded { get; private set; }
		public RelayCommand<Book> MarkBookAsFavoriteCommand { get; set; }
		public RelayCommand OpenBookCommand { get; private set; }
		public RelayCommand RateCommand { get; private set; }

		public ObservableCollection<Book> RecentBooks
		{
			get
			{
				if (Books != null)
				{
					var recentBooks = (from b in Books
									   orderby b.LastAccess descending
									   select b).Take(8);
					return new ObservableCollection<Book>(recentBooks);
				}
				return new ObservableCollection<Book>();
			}
		}

		public RelayCommand RefreshCommand { get; private set; }
		public RelayCommand RemoveAdsCommand { get; private set; }
		public ObservableCollection<object> SelectedBooks { get; set; }

		public SettingsHelper Settings
		{
			get { return SettingsHelper.Settings; }
		}
		public RelayCommand SettingsCommand { get; private set; }

		public RelayCommand<Book> UnmarkBookAsFavoriteCommand { get; private set; }
		public RelayCommand SearchChangedCommand { get; set; }

		public string SearchText { get; set; }

		public ObservableCollection<Book>Suggestions
		{
			get { return _suggestions; }
			set
			{
				_suggestions = value;
				RaisePropertyChanged();
			}
		}


		private void SearchChanged()
		{
			Suggestions = (SearchText.Length > 1) ?
					GetQuerySuggestions(SearchText) :
					new ObservableCollection<Book> { };
		}

		public async Task LoadBooks()
		{
			Books = new ObservableCollection<Book>(await _dataService.GetData());
			RefreshCommand.RaiseCanExecuteChanged();
			IsDataLoaded = true;
		}

		public async Task RePopulateBooksMetadataAsync()
		{
			await DataService.PopulateBooksMetadataAsync(true);
		}

		public async Task SaveAllAsync()
		{
			try
			{
				if (!IsDataLoaded) return;
				if (Books != null)
				{
					await SerializationHelper.SerializeAsync(Books);
					await SettingsHelper.SaveAsync();
				}
			}
			catch (Exception ex)
			{
				DebugReporter.ReportError(ex);
				Debugger.Break();
			}
		}

		private async void DeleteBooksAfterConfirmation(bool confirm)
		{
			if (!confirm) return;

			var removeFiles = new Queue<Book>();
			try
			{
				var containSdFile = false;
				foreach (Book item in SelectedBooks)
				{
					var bookFile = await StorageFile.GetFileFromPathAsync(item.FilePath);
					if (bookFile != null)
					{
						await bookFile.DeleteAsync();
					}
					removeFiles.Enqueue(item);
				}

				while (removeFiles.Count > 0)
				{
					var file = removeFiles.Dequeue();
					Books.Remove(file);
				}
				await SaveAllAsync();
				SelectedBooks.Clear();
			}
			catch (Exception ex)
			{
				DebugReporter.ReportError(ex);
			}
		}

		private async void DeleteBooksAsync()
		{
			await _dialogService.ShowMessage(
				"Are you sure you want to delete the selected Book(s)?", "Exclusion confirmation",
				"Yes", "No", DeleteBooksAfterConfirmation);
		}

		private async void ImportBooksAsync()
		{
			await _bookImporter.ImportBooksAsync();
			Books = new ObservableCollection<Book>(await _dataService.GetData());
		}

		private async void MarkBookAsFavorite(Book book)
		{
			Books.Remove(book);
			book.Favorited = true;
			Books.Add(book);
			await SaveAllAsync();

		}

		/// <summary>
		/// Open the selected book for viewing
		/// </summary>
		private void OpenBook()
		{
			var selectedBook = (Book)SelectedBooks.FirstOrDefault();
			var bookToOpen = (from b in Books
							  where b.FileName == selectedBook.FileName
							  select b).FirstOrDefault();
			_navigationService.Navigate<BookViewModel>(bookToOpen);
		}

		private async Task RefreshAsync()
		{
			await LoadBooks();
		}

		private void SetCommands()
		{
			MarkBookAsFavoriteCommand = new RelayCommand<Book>(MarkBookAsFavorite);
			UnmarkBookAsFavoriteCommand = new RelayCommand<Book>(UnmarkBookAsFavorite);
			DeleteBooksCommand = new RelayCommand(DeleteBooksAsync, SelectedBooks.Any);
			OpenBookCommand = new RelayCommand(OpenBook, () => SelectedBooks.Count == 1);
			RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !_dataService.IsBusy);
			GoBackCommand = new RelayCommand(_navigationService.GoBack, () => _navigationService.CanGoBack);
			SettingsCommand = new RelayCommand(_navigationService.Settings);
			AboutCommand = new RelayCommand(_navigationService.About);
			ImportBooksCommand = new RelayCommand(ImportBooksAsync);
			RateCommand = new RelayCommand(_navigationService.Rate);
			SearchChangedCommand = new RelayCommand(SearchChanged);
		}

		/// <summary>
		/// Load all books and settings
		/// </summary>
		private async Task LoadData()
		{
			await SettingsHelper.RestoreAsync();
			await LoadBooks();
		}

		private async void UnmarkBookAsFavorite(Book book)
		{
			Books.Remove(book);
			book.Favorited = false;
			Books.Add(book);
			await SaveAllAsync();
		}

		protected void OnStartLoading()
		{
			if (StartLoading != null) StartLoading.Invoke(this, EventArgs.Empty);
		}

		internal ObservableCollection<Book> GetQuerySuggestions(string text)
		{
			text = text.ToLower();
			var suggestions = (from b in Books
							  where b.Title.ToLower().Contains(text) || b.Author.ToLower().Contains(text)
							  select b).ToList();
			return new ObservableCollection<Book>(suggestions);
		}
	}
}