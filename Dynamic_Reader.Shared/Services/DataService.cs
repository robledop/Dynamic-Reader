using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Dynamic_Reader.Readers;
using Dynamic_Reader.Util;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;

namespace Dynamic_Reader.Services
{
	public class DataService : ObservableObject, IDataService
	{
		private ObservableCollection<Book> _books;
		private ISdCardService _sdCardService;
		private StorageFolder _booksFolder;
		private bool _isBusy;

		public DataService()
		{
			_sdCardService = ServiceLocator.Current.GetInstance<ISdCardService>();
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			private set
			{
				_isBusy = value;
				RaisePropertyChanged();
			}
		}

		private List<Book> BooksInStorage { get; set; }

		/// <summary>
		/// Main method to load all the books
		/// </summary>
		public async Task<IEnumerable<Book>> GetData()
		{
			IsBusy = true;
			BooksInStorage = new List<Book>();
			_booksFolder = await Globals.GetBooksFolder();

			_books = await SerializationHelper.DeserializeBooksAsync();

			await LoadBooksFromStorageAsync();
			await LoadBooksFromSdCardAsync();
			AddNewBooksToCollection();
			await DeleteFoldersWithoutRespectiveEpubFileAsync();

			await PopulateBooksMetadataAsync(true);

			IsBusy = false;
			return _books;
		}

		/// <summary>
		/// Load books from the "Books" folder on the internal app storage
		/// </summary>
		private async Task LoadBooksFromStorageAsync()
		{
			try
			{
				var files = await _booksFolder.GetFilesAsync();
				if (!files.Any()) return;
				foreach (var item in files)
				{
					var newBook = new Book
					{
						FileName = item.Name,
						FilePath = item.Path,
					};
					if (!BooksInStorage.Contains(newBook))
					{
						BooksInStorage.Add(newBook);
					}
				}
			}
			catch (Exception ex)
			{
				DebugReporter.ReportError(ex);
				Debugger.Break();
			}
		}

		private async Task LoadBooksFromSdCardAsync()
		{
			var sdCardBooks = await _sdCardService.GetData();
			if (sdCardBooks == null) return;

			foreach (Book book in sdCardBooks)
			{
				if (!BooksInStorage.Contains(book))
				{
					BooksInStorage.Add(book);
				}
				else //TODO: este 'else' serve para lidar com a migração da versão 2.6 para 3.0
				{
					BooksInStorage.Remove(book);
					BooksInStorage.Add(book);
				}
			}
		}

		/// <summary>
		/// Add books that are in storage but not on the Books collection
		/// </summary>
		private void AddNewBooksToCollection()
		{
			foreach (var book in BooksInStorage)
			{
				if (!_books.Contains(book))
				{
					_books.Add(book);
				}
				else //TODO: este 'else' serve para lidar com a migração da versão 2.6 para 3.0
				{
					var existingBook = _books.First(b => b.FileName == book.FileName);
					existingBook.FilePath = book.FilePath;
					_books.Remove(existingBook);
					_books.Add(existingBook);
				}
			}
		}

		/// <summary>
		/// Delete extracted folders which does not have a respective .epub file
		/// </summary>
		private async Task DeleteFoldersWithoutRespectiveEpubFileAsync()
		{
			try
			{
				var onlyBooksWithARespectiveEpubFile = _books.Join(BooksInStorage, d => d, s => s,
					(d, s) => d);
				_books = new ObservableCollection<Book>(onlyBooksWithARespectiveEpubFile);

				var extractFolder = await Globals.GetExtractFolder();

				var allFolders = await extractFolder.GetFoldersAsync();
				if (!allFolders.Any())
				{
					return;
				}
				var foldersWithRespectiveEpubFiles = allFolders.Join(_books, d => d.Name, b => b.FileName,
					(d, b) => d);
				var foldersToDelete = allFolders.Except(foldersWithRespectiveEpubFiles);

				var toDelete = foldersToDelete as StorageFolder[] ?? foldersToDelete.ToArray();
				if (toDelete.Any())
				{
					foreach (var folder in toDelete)
					{
						await folder.DeleteAsync();
					}
				}
			}
			catch (Exception ex)
			{
				DebugReporter.ReportError(ex);
			}
		}

		/// <summary>
		/// Populate all books metadata.
		/// </summary>
		/// <param name="repopulate">Force repopulation</param>
		/// <returns></returns>
		public async Task PopulateBooksMetadataAsync(bool repopulate)
		{
			foreach (Book book in _books)
			{
				if ((string.IsNullOrWhiteSpace(book.Author) || repopulate) && book.FileName.ToLower().EndsWith(".epub"))
				{
					using (var bookToBeParsed = new EpubReader(book))
					{
						await bookToBeParsed.GetEpubMetadataAsync();
					}
				}
			}
		}
	}
}