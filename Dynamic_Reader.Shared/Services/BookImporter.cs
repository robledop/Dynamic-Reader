using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Dynamic_Reader.Views;

namespace Dynamic_Reader.Services
{
	public class BookImporter : IImporter
	{
		public async Task ImportBooksAsync()
		{
			try
			{
				var openPicker = new FileOpenPicker
				{
					ViewMode = PickerViewMode.List,
					CommitButtonText = "Import",
					SuggestedStartLocation = PickerLocationId.DocumentsLibrary
				};
				openPicker.FileTypeFilter.Add(".epub");
				openPicker.FileTypeFilter.Add(".txt");
				openPicker.FileTypeFilter.Add(".pdf");
				openPicker.FileTypeFilter.Add(".fb2");
				openPicker.FileTypeFilter.Add(".book");
#if WINDOWS_PHONE_APP
				openPicker.PickMultipleFilesAndContinue();
#else
				IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
				await ContinueImportBooksAsync(files);
#endif
			}
			catch (Exception ex)
			{
				DebugReporter.ReportError(ex);
			}
		}

		public async Task ContinueImportBooksAsync(object filesToImport)
		{
			var files = (IReadOnlyList<StorageFile>)filesToImport;
			var portableFiles = new List<BookToImport>();
			foreach (StorageFile storageFile in files)
			{
				var bookToImport = new BookToImport
				{
					Name = storageFile.Name,
					Path = storageFile.Path
				};

				portableFiles.Add(bookToImport);
			}

			if (files.Any())
			{
				await ImportBooksAsync(files, portableFiles);
			}
		}

		private async Task ImportBooksAsync(IEnumerable<StorageFile> books, IEnumerable<BookToImport> booksToImport)
		{
#if !WINDOWS_PHONE_APP
			var import = new ImportBooksFlyout();
			import.ShowIndependent();
#endif
			StorageFolder booksFolder =
				await ApplicationData.Current
					.LocalFolder.CreateFolderAsync("Books", CreationCollisionOption.OpenIfExists);

			App.MainViewModel.BooksToImport = new ObservableCollection<BookToImport>(booksToImport);

			var importQueue = new Queue<StorageFile>(books);

			while (importQueue.Count > 0)
			{
				StorageFile book = importQueue.Dequeue();
				StorageFile copyedFile =
					await book.CopyAsync(booksFolder, book.Name, NameCollisionOption.ReplaceExisting);
				StorageFile portableFile = await StorageFile.GetFileFromPathAsync(copyedFile.Path);

				var bookToAdd = new Book
				{
					FileName = portableFile.Name,
					FilePath = portableFile.Path,
				};

				App.MainViewModel.Books.Add(bookToAdd);

				BookToImport bookToRemove = (from b in App.MainViewModel.BooksToImport
											 where b.Path == book.Path
											 select b).FirstOrDefault();
				App.MainViewModel.BooksToImport.Remove(bookToRemove);
			}
#if !WINDOWS_PHONE_APP
			import.Hide();
#endif
		}
	}
}