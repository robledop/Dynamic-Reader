//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using Dynamic_Reader.Helpers;
//using Dynamic_Reader.Model;
//using GalaSoft.MvvmLight.Threading;
//using Windows.Storage;

//namespace Dynamic_Reader.Readers
//{
//	public class MyPdfReader : BookReader
//	{
//		const string PDF_TABLE_FORMAT = @"\(.*\)Tj";
//		Regex PdfTableRegex = new Regex(PDF_TABLE_FORMAT);
//		public MyPdfReader(Book book)
//		{
//			CurrentBook = book;
//			Timer = new Timer(Timer_Tick, SynchronizationContext.Current, -1, Timeout.Infinite);
//		}

//		private void Timer_Tick(object state)
//		{
//			DispatcherHelper.CheckBeginInvokeOnUI(() =>
//			{
//				FillScreen();

//				if (CurrentText == null)
//				{
//					return;
//				}

//				if (SettingsHelper.Settings.PauseAtTheEndOfPhrase && IsEndOfFrase(CurrentText))
//				{
//					ResumeReading(SettingsHelper.Settings.PauseAtTheEndOfFraseDuration);
//				}
//				else if (SettingsHelper.Settings.PauseAfterPunctuation && EndWithPunctuation(CurrentText))
//				{
//					ResumeReading(SettingsHelper.Settings.PauseAfterPunctuationDuration);
//				}
//				else if (SettingsHelper.Settings.PauseAfterLongWords && CurrentText.Length > 8)
//				{
//					ResumeReading(SettingsHelper.Settings.PauseAfterLongWordsDuration);
//				}
//				else
//				{
//					Timer.Change(TimerInterval, Timeout.Infinite);
//				}
//			});
//		}
//		public override async Task OpenAsync()
//		{
//			BookLoading = true;
//			CurrentBook.LastAccess = DateTime.Now;
//			var file = await StorageFile.GetFileFromPathAsync(CurrentBook.FilePath);
//			var stream = await file.OpenStreamForReadAsync();
//			var pdfReader = new PdfReader.Pdf.PdfReader(stream);

//			var stringBuilder = new StringBuilder();
//			for (int i = 1; i < pdfReader.NumberOfPages; i++)
//			{
//				var textFromPage = PdfTextExtractor.GetTextFromPage(pdfReader, i);
//				stringBuilder.Append(textFromPage);
//			}

//			var allText = stringBuilder.ToString();
//			allText = Regex.Replace(allText, @"\s+", " ").Trim();
//			TextToRead = allText.Split(' ');
//			FillScreen();
//			BookLoading = false;
//		}

//		List<string> ExtractPdfContent(string rawPdfContent)
//		{
//			var matches = PdfTableRegex.Matches(rawPdfContent);

//			var list = matches.Cast<Match>()
//				.Select(m => m.Value
//					.Substring(1) //remove leading (
//					.Remove(m.Value.Length - 4) //remove trailing )Tj
//					.Replace(@"\)", ")") //unencode parens
//					.Replace(@"\(", "(")
//					.Trim()
//				)
//				.ToList();
//			return list;
//		}

//		public override Task GoNext()
//		{
//			throw new NotImplementedException();
//		}

//		public override void Seek(int position)
//		{
//			throw new NotImplementedException();
//		}

//		public override Task GoPrevious()
//		{
//			throw new NotImplementedException();
//		}

//		public override Task ReadChapter(string chapterFilename, string positionInBook = null)
//		{
//			throw new NotImplementedException();
//		}

//		public override void Dispose()
//		{

//		}
//	}
//}
