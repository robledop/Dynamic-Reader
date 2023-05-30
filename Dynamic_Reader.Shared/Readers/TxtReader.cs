using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Model;
using Dynamic_Reader.Util;
using GalaSoft.MvvmLight.Threading;

namespace Dynamic_Reader.Readers
{
	public sealed class TxtReader : BookReader
	{
		public override void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Timer != null) Timer.Dispose();
			}
			Disposed = true;
		}

		public TxtReader(Book currentBook)
		{
			CurrentBook = currentBook;

			Timer = new Timer(Timer_Tick, null, -1, -1);
		}

		private void Timer_Tick(object state)
		{
			try
			{
				DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					FillScreen();

					if (CurrentText == null)
					{
						return;
					}

					if (SettingsHelper.Settings.PauseAtTheEndOfPhrase && IsEndOfFrase(CurrentText))
					{
						PauseReading();
						ResumeReading(SettingsHelper.Settings.PauseAtTheEndOfFraseDuration);
					}
					else if (SettingsHelper.Settings.PauseAfterPunctuation && EndWithPunctuation(CurrentText))
					{
						PauseReading();
						ResumeReading(SettingsHelper.Settings.PauseAfterPunctuationDuration);
					}
					else if (SettingsHelper.Settings.PauseAfterLongWords && CurrentText.Length > 8)
					{
						PauseReading();
						ResumeReading(SettingsHelper.Settings.PauseAfterLongWordsDuration);
					}
					Timer.Change(TimerInterval, Timeout.Infinite);
				});
			}
			catch (Exception ex)
			{
                Debug.WriteLine(ex);
			}
		}

		public override async Task OpenAsync()
		{
			BookLoading = true;
			CurrentBook.LastAccess = DateTime.Now;

			await ReadBook(false);

			if (CurrentBook.TotalNumberOfWords == 0)
			{
				CurrentBook.TotalNumberOfWords = await GetTotalNumberOfWords();
			}
			BookLoading = false;
		}
		private async Task<int> GetTotalNumberOfWords()
		{
		    var booksFolder = await Globals.GetBooksFolder();

			var chapterFile = await booksFolder.GetFileAsync(CurrentBook.FileName);
			var bookContent = await FileIO.ReadTextAsync(chapterFile);

			var text = Regex.Replace(bookContent, @"\s+", " ").Trim();
			var splitedText = text.Split(' ');

			return splitedText.Length + 1;
		}

		private async Task ReadBook(bool continueReading)
		{
			await ReadChapter(CurrentBook.FileName);

			if (continueReading)
			{
				StartReading();
			}
		}

		public override async Task GoNext()
		{
			try
			{
				if (CurrentBook == null || BookLoading || TextToRead == null) return;
				StopReading();
				CurrentBook.PositionInChapter = CurrentBook.PositionInChapter + 30;
				if (CurrentBook.PositionInChapter > TextToRead.Length)
				{
					CurrentBook.PositionInChapter = TextToRead.Length;
				}
				FillScreen();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		public override void Seek(int position)
		{
			if (CurrentBook != null)
			{
				CurrentBook.PositionInChapter = position;
				FillScreen();
			}
		}

		public override async Task GoPrevious()
		{
			try
			{
				if (CurrentBook == null || BookLoading || TextToRead == null) return;
				StopReading();
				CurrentBook.PositionInChapter = CurrentBook.PositionInChapter - 30;
				if (CurrentBook.PositionInChapter < 0)
				{
					CurrentBook.PositionInChapter = 0;
				}
				FillScreen();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		public override async Task ReadChapter(string fileName, string positionInBook = null)
		{
			var booksFolder = await Globals.GetBooksFolder();

			string chapterContent;

				var chapterFile = await booksFolder.GetFileAsync(fileName);
				using (var stream = await chapterFile.OpenStreamForReadAsync())
				{
					using (var reader = new StreamReader(stream, true))
					{
						chapterContent = await reader.ReadToEndAsync();
					}
				}

			var text = Regex.Replace(chapterContent, @"\s+", " ").Trim();
			TextToRead = text.Split(' ');
			FillScreen();
		}

		~TxtReader()
		{
			Dispose(true);
		}
	}
}
