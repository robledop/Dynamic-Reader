using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Util;
using GalaSoft.MvvmLight;

namespace Dynamic_Reader.Model
{
	[DebuggerDisplay("Title: {Title}")]
	[XmlRoot]
	public sealed class Book : ObservableObject
	{
		private string _author;
		private int _chapter;
		private string _cover;
		private string _description;
		private string _fileName;
		private DateTime _lastAccess;
		private int _positionInChapter;
		private string _publisher;
		private string _title;
		private double _wordsRead;

		public double PercentageRead
		{
			get
			{
				if (TotalNumberOfWords == 0)
				{
					return 0;
				}

				return Math.Round((WordsRead / TotalNumberOfWords) * 100, 2);
			}
		}

		public double WordsRead
		{
			get
			{
				if (FileName.ToLower().EndsWith(".txt"))
				{
					return PositionInChapter;
				}

				if (ChapterFileList == null || !ChapterFileList.Any())
				{
					return 0;
				}

				double total = 0;
				if (Chapter >= ChapterFileList.Count)
				{
					Chapter = ChapterFileList.Count - 1;
				}

				for (int i = 0; i < Chapter; i++)
				{
					total += ChapterFileList[i].NumberOfWords;
				}
				total += PositionInChapter;
				return total;
			}
			set { _wordsRead = value; }
		}

		[XmlElement]
		public bool Extracted { get; set; }

		
		private bool _favorited;

		[XmlElement]
		public bool Favorited
		{
			get { return _favorited; }
			set
			{
				_favorited = value;
				RaisePropertyChanged();
			}
		}


		public List<ChapterFile> ChapterFileList { get; set; }

		private ObservableCollection<Bookmark> _bookmarks;
		public ObservableCollection<Bookmark> Bookmarks
		{
			get
			{
				if (_bookmarks == null)
				{
					_bookmarks = new ObservableCollection<Bookmark>();
				}
				return _bookmarks;
			}
			set
			{
				_bookmarks = value;

			}
		}

		private int _totalNumberOfWords;

		[XmlElement]
		public int TotalNumberOfWords
		{
			get { return _totalNumberOfWords; }
			set
			{
				_totalNumberOfWords = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public string Author
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_author))
				{
					return "Unknown";
				}
				return _author;
			}
			set
			{
				_author = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public DateTime LastAccess
		{
			get { return _lastAccess; }
			set
			{
				_lastAccess = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public string Title
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_title))
				{
					return FileName;
				}
				return _title;
			}
			set
			{
				_title = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public string Cover
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_cover) || !Helper.IsImage(_cover))
				{
					_cover = "/Assets/book.jpg";
				}
				else if (_cover != "/Assets/book.jpg")
				{
					var exists = Task.Run(async () => await FileHelper.FileExists(_cover)).Result;
					if (!exists)
					{
						Cover = "/Assets/book.jpg";
						return _cover;
					}
				}

				return _cover;
			}
			set
			{
				_cover = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public string Publisher
		{
			get { return _publisher; }
			set
			{
				_publisher = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				RaisePropertyChanged();
			}
		}

		[XmlElement]
		public int PositionInChapter
		{
			get { return _positionInChapter; }
			set
			{
				_positionInChapter = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => PercentageRead);
				RaisePropertyChanged(() => WordsRead);
			}
		}

		[XmlElement]
		public int Chapter
		{
			get { return _chapter; }
			set
			{
				_chapter = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => PercentageRead);
				RaisePropertyChanged(() => WordsRead);
			}
		}

		[XmlElement]
		public string FilePath { get; set; }

		[XmlElement]
		public string FileName
		{
			get { return _fileName; }
			set
			{
				_fileName = value;
				RaisePropertyChanged();
			}
		}

		public Book()
		{
		}

		public Book(StorageFile bookFile)
		{
			if (bookFile == null) throw new ArgumentNullException("bookFile");
			FilePath = bookFile.Path;
			FileName = bookFile.Name;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			return FileName;
		}

		public override bool Equals(object obj)
		{
			var book = obj as Book;

			if (book != null)
			{
				return (book.FileName == FileName);
			}

			return false;
		}

		public static bool operator ==(Book book1, Book book2)
		{
			if (book1 != null && book2 != null)
			{
				return book1.Equals(book2);
			}

			return Equals(book1, book2);
		}

		public static bool operator !=(Book book1, Book book2)
		{
			if (Equals(book1, null))
			{
				return false;
			}

			return !book1.Equals(book2);
		}
	}
}