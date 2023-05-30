using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Dynamic_Reader.Readers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;

namespace Dynamic_Reader.ViewModel
{
	public class BookViewModel : ViewModelBase
	{
		private readonly INavigationService _navigationService;
		private readonly IDialogService _dialogService;
		private BookReader _bookReader;


		public static SettingsHelper Settings
		{
			get { return SettingsHelper.Settings; }
		}

		public RelayCommand<int> SeekCommand { get; set; }
		public RelayCommand StartCommand { get; private set; }
		public RelayCommand StopCommand { get; private set; }
		public RelayCommand GoPreviousCommand { get; private set; }
		public RelayCommand GoNextCommand { get; private set; }
		public RelayCommand GoBackCommand { get; private set; }
		public RelayCommand GoToTableOfContentsCommand { get; private set; }
		public RelayCommand PinToStartCommand { get; private set; }
		public RelayCommand BookmarkCommand { get; private set; }
		public RelayCommand GoToBookmarksCommand { get; private set; }
		public RelayCommand<Bookmark> GoToBookmarkCommand { get; set; }

		public BookReader BookReader
		{
			get { return _bookReader; }
			private set
			{
				_bookReader = value;
				RaisePropertyChanged();
			}
		}

		public BookViewModel()
		{
			_navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
			_dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
			SetCommands();
		}

		private void SetCommands()
		{
			SeekCommand = new RelayCommand<int>(Seek);
			StartCommand = new RelayCommand(Start, () => BookReader != null && !BookReader.BookLoading);
			StopCommand = new RelayCommand(Stop, () => BookReader != null && !BookReader.BookLoading);
			GoNextCommand = new RelayCommand(GoNext, () => BookReader != null && !BookReader.BookLoading);
			GoPreviousCommand = new RelayCommand(GoPrevious, () => BookReader != null && !BookReader.BookLoading);
			GoBackCommand = new RelayCommand(GoBack, () => _navigationService.CanGoBack);
			GoToTableOfContentsCommand = new RelayCommand(
				_navigationService.GoToTableOfContents, () => BookReader != null && !BookReader.BookLoading && BookReader.Toc != null);
			PinToStartCommand = new RelayCommand(_navigationService.PinToStart, () => BookReader != null && !_bookReader.BookLoading);
			BookmarkCommand = new RelayCommand(async () =>
			{
				BookReader.AddBookmark();
				await SerializationHelper.SerializeAsync();
				_dialogService.ShowPopUp("Bookmarked successfully", "");
			}, () => _bookReader != null && !_bookReader.BookLoading);

			GoToBookmarksCommand = new RelayCommand(_navigationService.GoToBookmarks, () => _bookReader != null && !_bookReader.BookLoading);

			GoToBookmarkCommand = new RelayCommand<Bookmark>(GoToBookmark);
		}

		private void Start()
		{
			BookReader.StartReading();
		}

		private void Stop()
		{
			BookReader.StopReading();
		}

		private async void GoNext()
		{
			await BookReader.GoNext();
			RaiseBookNavigationStarted();
		}

		private void RaiseBookNavigationStarted()
		{
			if (BookNavigationStarted != null)
			{
				BookNavigationStarted(this, EventArgs.Empty);
			}
		}

		private void Seek(int position)
		{
			BookReader.Seek(position);
		}

		private async void GoPrevious()
		{
			await BookReader.GoPrevious();
			RaiseBookNavigationStarted();
		}

		private void GoBack()
		{
			_navigationService.GoBack();
		}

		public async Task GoToChapter(Chapter chapter)
		{
			if (chapter == null) throw new ArgumentNullException("chapter");
			string positionInBook = null;

			if (chapter.File.Contains("#"))
			{
				positionInBook = chapter.File.Split('#').Last();
			}
			else
			{
				BookReader.CurrentBook.PositionInChapter = 0;
			}

			BookReader.CurrentBook.Chapter = BookReader.ChapterFiles
				.FindIndex(s => s == chapter.File.Split('#').FirstOrDefault());

			await BookReader.ReadChapter(chapter.File.Split('#').FirstOrDefault(), positionInBook);
		}

		private void GoToBookmark(Bookmark bookmark)
		{
			_navigationService.GoToBookmark(bookmark);

		}

		public async Task LoadDataAsync(Book currentBook)
		{
			var success = true;
			try
			{
				if (currentBook == null) throw new ArgumentNullException("currentBook");

				if (BookReader != null && !BookReader.BookLoading)
				{
					BookReader.Dispose();
				}
				BookReader = null;
				RaiseCanExecuteChanged();

				if (currentBook.FileName.ToLower().EndsWith(".epub"))
				{
					BookReader = new EpubReader(currentBook);
					await BookReader.OpenAsync();
				}
				else if (currentBook.FileName.ToLower().EndsWith(".txt") || currentBook.FileName.ToLower().EndsWith(".book"))
				{
					BookReader = new TxtReader(currentBook);
					await BookReader.OpenAsync();
				}
				//else if (currentBook.FileName.ToLower().EndsWith(".pdf"))
				//{
				//	BookReader = new MyPdfReader(currentBook);
				//	await BookReader.OpenAsync();
				//}
				else if (currentBook.FileName.ToLower().EndsWith(".fb2"))
				{
					BookReader = new Fb2Reader(currentBook);
					await BookReader.OpenAsync();
				}

				RaiseCanExecuteChanged();
			}
			catch (KeyNotFoundException ex)
			{
				DebugReporter.ReportError(ex);
				success = false;
			}

			if (!success)
			{
				await _dialogService.ShowMessage("Book not found, try to pin it again.", "Error");
			}
		}

		private void RaiseCanExecuteChanged()
		{
			StartCommand.RaiseCanExecuteChanged();
			StopCommand.RaiseCanExecuteChanged();
			GoNextCommand.RaiseCanExecuteChanged();
			GoPreviousCommand.RaiseCanExecuteChanged();
			GoToTableOfContentsCommand.RaiseCanExecuteChanged();
			PinToStartCommand.RaiseCanExecuteChanged();
			BookmarkCommand.RaiseCanExecuteChanged();
			GoToBookmarksCommand.RaiseCanExecuteChanged();
		}

		public event EventHandler BookNavigationStarted;
	}
}