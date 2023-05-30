using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Model;
using FB2Library;
using GalaSoft.MvvmLight.Threading;

namespace Dynamic_Reader.Readers
{
	public class Fb2Reader : BookReader
	{
		public Fb2Reader(Book book)
		{
			CurrentBook = book;
			Timer = new Timer(Timer_Tick, SynchronizationContext.Current, -1, Timeout.Infinite);
		}

		private void Timer_Tick(object state)
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
					ResumeReading(SettingsHelper.Settings.PauseAtTheEndOfFraseDuration);
				}
				else if (SettingsHelper.Settings.PauseAfterPunctuation && EndWithPunctuation(CurrentText))
				{
					ResumeReading(SettingsHelper.Settings.PauseAfterPunctuationDuration);
				}
				else if (SettingsHelper.Settings.PauseAfterLongWords && CurrentText.Length > 8)
				{
					ResumeReading(SettingsHelper.Settings.PauseAfterLongWordsDuration);
				}
				else
				{
					Timer.Change(TimerInterval, Timeout.Infinite);
				}
			});
		}

		public override async Task OpenAsync()
		{
			var file = await StorageFile.GetFileFromPathAsync(CurrentBook.FilePath);
			var stream = await file.OpenStreamForReadAsync();
			ReadFb2FileStream(stream);
			stream.Dispose();
		}

		public override Task GoNext()
		{
			throw new NotImplementedException();
		}

		public override void Seek(int position)
		{
			throw new NotImplementedException();
		}

		public override Task GoPrevious()
		{
			throw new NotImplementedException();
		}

		public override Task ReadChapter(string chapterFilename, string positionInBook = null)
		{
			throw new NotImplementedException();
		}

		private void ReadFb2FileStream(Stream s)
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			XDocument fb2Document;
			using (XmlReader reader = XmlReader.Create(s, settings))
			{
				fb2Document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
			}
			FB2File file = new FB2File();
			try
			{
				file.Load(fb2Document, false);
				StringBuilder sb = new StringBuilder();
				foreach (var sectionItem in file.MainBody.Sections)
				{
					foreach (var fb2TextItem in sectionItem.Content)
					{
						sb.Append(fb2TextItem);
					}
				}
				var text = sb.ToString();
				TextToRead = text.Split(' ');
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error loading file : {0}", ex.Message);
			}
		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
