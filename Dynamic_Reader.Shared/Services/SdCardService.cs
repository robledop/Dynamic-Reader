using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;

namespace Dynamic_Reader.Services
{
	public class SdCardService : ISdCardService
	{
		private List<Book> _sdCardBooks;

		public async Task<IEnumerable<Book>> GetData()
		{
			_sdCardBooks = new List<Book>();
			await LoadSdBooksAsync();
			return _sdCardBooks;
		}

		private async Task LoadSdBooksAsync()
		{
			var externalDevices = KnownFolders.RemovableDevices;

			var sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();

			if (sdCard != null)
			{
				try
				{
					var booksFolder = await sdCard.GetFolderAsync("Books");
					if (booksFolder == null) return;
					await GetFilesInSdCard(booksFolder);
					var subFolders = await booksFolder.GetFoldersAsync();
					var externalStorageFolders = subFolders as IList<StorageFolder> ?? subFolders.ToList();
					if (!externalStorageFolders.Any()) return;

					foreach (var folder in externalStorageFolders)
					{
						await GetFilesInSdCard(folder);
						var subSubFolders = await folder.GetFoldersAsync();
						if (subSubFolders == null) return;
						foreach (var subFolder in subSubFolders)
						{
							await GetFilesInSdCard(subFolder);
						}
					}
				}
				catch (FileNotFoundException ex)
				{
					DebugReporter.ReportError(ex);

				}
				catch (Exception ex)
				{
					DebugReporter.ReportError(ex);

				}
			}
		}

		private async Task GetFilesInSdCard(StorageFolder folder)
		{
			var allFiles = await folder.GetFilesAsync();
			var externalStorageFiles = allFiles as IList<StorageFile> ?? allFiles.ToList();
			foreach (var esf in externalStorageFiles)
			{
				if (esf.Path.ToLower().EndsWith(".epub") || esf.Path.ToLower().EndsWith(".txt") || esf.Path.ToLower().EndsWith(".book"))
				{
					var book = new Book
					{
						FileName = esf.Name,
						FilePath = esf.Path
					};

					if (!_sdCardBooks.Contains(book))
					{
						_sdCardBooks.Add(book);
					}
				}
			}
		}
	}
}