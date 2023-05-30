using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;

namespace Dynamic_Reader.Readers
{
	public abstract class BookReader : ObservableObject, IDisposable
	{
		private string _timeLeft;
		private string _previousText;
		private string _nextText;
		private string _currentText;
		protected string[] TextToRead;
		private bool _bookLoading;
		protected bool Disposed;
		protected int TimerInterval;
		public string TimeLeft
		{
			get { return _timeLeft; }
			private set
			{
				_timeLeft = value;
				RaisePropertyChanged();
			}
		}


		private string GetTimeLeft()
		{
			if (TextToRead == null)
			{
				return "";
			}

			var wordsLetf = TextToRead.Length - CurrentBook.PositionInChapter;
			var milliseconsLeft = wordsLetf * (3600000 /
											   (SettingsHelper.Settings.WordsPerMinute *
												SettingsHelper.Settings.WordsPerFixation * 60));

			var timeLeft = TimeSpan.FromMilliseconds(milliseconsLeft);
			return "Time left until the end of this chapter: " +
				   timeLeft.Hours.ToString("00") + ":" +
				   timeLeft.Minutes.ToString("00") + ":" +
				   timeLeft.Seconds.ToString("00");
		}

		protected bool EndWithPunctuation(string text)
		{
			var result = (text.EndsWith(",")
					|| text.EndsWith(":")
					|| text.EndsWith(";"));
			return result;
		}

		protected bool IsEndOfFrase(string text)
		{
			return (text.EndsWith(".")
				|| text.EndsWith("!")
				|| text.EndsWith("?")
				|| text.EndsWith(".\""));
		}

		private string GetNextText()
		{
			if (CurrentBook == null || TextToRead == null)
			{
				return string.Empty;
			}

			var nextTextArray = new StringBuilder();
			for (var i = 0; i < 30; i++)
			{
				var nextPosition = CurrentBook.PositionInChapter + i;

				if (nextPosition < TextToRead.Length)
				{
					nextTextArray.Append(TextToRead[nextPosition]);
					nextTextArray.Append(" ");
				}
			}
			return nextTextArray.ToString();
		}

		private string GetPreviousText()
		{
			if (CurrentBook == null || TextToRead == null)
			{
				return string.Empty;
			}

			var previousTextArray = new StringBuilder();
			for (var i = 1; i < 30; i++)
			{
				var previousIndex = CurrentBook.PositionInChapter - i;
				if (previousIndex >= 0)
				{
					if (previousIndex < TextToRead.Length)
					{
						previousTextArray.Insert(0, TextToRead[previousIndex]);
						previousTextArray.Insert(0, " ");
					}
				}
				else
				{
					break;
				}
			}
			return previousTextArray.ToString();
		}

		public string CurrentText
		{
			get { return _currentText; }
			private set
			{
				_currentText = value;
				RaisePropertyChanged();
			}
		}
		public string PreviousText
		{
			get { return _previousText; }
			private set
			{
				_previousText = value;
				RaisePropertyChanged();
			}
		}
		public string NextText
		{
			get { return _nextText; }
			private set
			{
				_nextText = value;
				RaisePropertyChanged();
			}
		}

		protected async Task ShowUnsupportedFileMessage()
		{
			var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
			var navigatonService = ServiceLocator.Current.GetInstance<INavigationService>();
			await dialogService.ShowMessage("The file \"" + CurrentBook.FileName +
					"\" is not supported or is corrupted. Try to import the file again.",
					"Unsupported file", "Ok", navigatonService.GoBack);
		}

		public Book CurrentBook { get; protected set; }
		public bool BookLoading
		{
			get { return _bookLoading; }
			protected set
			{
				_bookLoading = value;
				RaisePropertyChanged();
			}
		}
		public List<string> ChapterFiles { get; set; }
		public bool Stopped { get; set; }
		public Timer Timer { get; protected set; }


		public abstract Task OpenAsync();
		public void StartReading()
		{
			if (CurrentBook == null || BookLoading) return;
			TimerInterval = (int)(TimeSpan.TicksPerMinute / (SettingsHelper.Settings.WordsPerMinute * 10000));
			Stopped = false;
			Timer.Change(TimerInterval, Timeout.Infinite);
		}
		public void StopReading()
		{
			if (CurrentBook == null || BookLoading) return;
			Stopped = true;
			Timer.Change(-1, -1);
		}
		public abstract Task GoNext();
		public abstract void Seek(int position);
		public abstract Task GoPrevious();
		public abstract Task ReadChapter(string chapterFilename, string positionInBook = null);
		public void FillScreen()
		{
			try
			{

				TimeLeft = GetTimeLeft();
				PreviousText = GetPreviousText();
				CurrentText = GetCurrentText();
				NextText = GetNextText();
				if (string.IsNullOrWhiteSpace(CurrentText))
				{
					if (CurrentBook.PositionInChapter < TextToRead.Length - 1)
					{
						CurrentBook.PositionInChapter++;
						FillScreen();
					}
				}
			}
			catch (Exception ex)
			{
                Debug.WriteLine(ex);
            }
		}



		public List<Chapter> Toc { get; protected set; }

		public abstract void Dispose();

		protected void PauseReading()
		{
			Timer.Change(-1, -1);
		}

		public void AddBookmark()
		{
			var newBookmark = new Bookmark
			{
				Chapter = CurrentBook.Chapter,
				PositionInChapter = CurrentBook.PositionInChapter,
				PercentageRead = CurrentBook.PercentageRead,
				Excerpt = CurrentText + " " + NextText
			};
			CurrentBook.Bookmarks.Add(newBookmark);
		}

		protected void ResumeReading(int delay)
		{
			if (delay < 0 || delay > 3) throw new ArgumentOutOfRangeException("delay", "delay must be between 0 and 3");
			if (Stopped) return;

			int timeDelay = 0;
			switch (delay)
			{
				case 0:
					timeDelay = (TimerInterval / 2) * 3;
					break;
				case 1:
					timeDelay = TimerInterval * 2;
					break;
				case 2:
					timeDelay = TimerInterval * 3;
					break;
				case 3:
					timeDelay = TimerInterval * 4;
					break;
			}

			Timer.Change(TimerInterval + timeDelay, Timeout.Infinite);
		}

		private string GetCurrentText()
		{
			if (CurrentBook == null || TextToRead == null)
			{
				return string.Empty;
			}

			var textArray = new StringBuilder();
			int startPosition = CurrentBook.PositionInChapter;
			do
			{
				if (CurrentBook.PositionInChapter < TextToRead.Length)
				{
					textArray.Append(TextToRead[CurrentBook.PositionInChapter]);
					if (SettingsHelper.Settings.WordsPerFixation > 1)
					{
						textArray.Append(" ");
					}
				}
			} while (++CurrentBook.PositionInChapter < (startPosition +
				SettingsHelper.Settings.WordsPerFixation));

			return textArray.ToString();
		}
	}
}
