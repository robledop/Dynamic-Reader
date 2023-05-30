using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;
using Dynamic_Reader.Util;
using GalaSoft.MvvmLight;

namespace Dynamic_Reader.Helpers
{
	[XmlRoot]
	public sealed class SettingsHelper : ObservableObject
	{
		public  SettingsHelper()
		{
			
		}
		private const string SETTINGS_FILE = "Settings.xml";
		private static SettingsHelper _settings;

		private readonly Dictionary<string, string> _fontTypeList = new Dictionary<string, string>
		{
			{"Courier New","Courier New"},
			{"Lucida Console", "Lucida Console"},
			{"Droid Sans Mono","/Dynamic_Reader.WP;Component/Fonts/DroidSansMono.ttf#Droid Sans Mono"},
			{"Anonymous", "/Dynamic_Reader.WP;Component/Fonts/Anonymous.ttf#Anonymous"}
		};

		private readonly List<string> _themeList = new List<string>
		{
			"Light",
			"Dark"
		};

		private double _fontSize;
		private int _wordsPerMinute;
		private bool _enableScreenLock;
		private int _theme;
		private int _fontType;
		private bool _pauseAfterPunctuation;
		private bool _enableOrp;
		private int _pauseAfterPunctuationDuration;
		private int _pauseAfterLongWordsDuration;
		private bool _pauseAfterLongWords;

		private bool _displayGuides;

		[XmlIgnore]
		public static bool IsLoaded { get; private set; }

		[XmlIgnore]
		public static SettingsHelper Settings
		{
			get { return _settings ?? (_settings = new SettingsHelper()); }
		}

		[XmlIgnore]
		public Dictionary<string, string> FontTypeList
		{
			get { return _fontTypeList; }
		}

		[XmlIgnore]
		public IEnumerable<string> ThemeList
		{
			get { return _themeList; }
		}

		private int _wordsPerFixation;
		public int WordsPerFixation
		{
			get
			{
				if (_wordsPerFixation == 0)
				{
					return 1;
				}
				return _wordsPerFixation;
			}
			set
			{
				_wordsPerFixation = value;
				RaisePropertyChanged();
			}

		}

		public int PauseAfterPunctuationDuration
		{
			get { return _pauseAfterPunctuationDuration; }
			set
			{
				_pauseAfterPunctuationDuration = value;
				RaisePropertyChanged();
			}
		}

		public bool DisplayGuides
		{
			get { return _displayGuides; }
			set
			{
				_displayGuides = value;
				RaisePropertyChanged();
			}
		}

		private bool _pauseAtTheEndOfPhrase;

		public bool PauseAtTheEndOfPhrase
		{
			get { return _pauseAtTheEndOfPhrase; }
			set
			{
				_pauseAtTheEndOfPhrase = value;
				RaisePropertyChanged();
			}
		}

		private bool _showFavoritesAtStartup;

		public bool ShowFavoritesAtStartup
		{
			get { return _showFavoritesAtStartup; }
			set
			{
				_showFavoritesAtStartup = value; 
				RaisePropertyChanged();
			}
		}

		private int _pauseAtTheEndOfFraseDuration;

		public int PauseAtTheEndOfFraseDuration
		{
			get { return _pauseAtTheEndOfFraseDuration; }
			set
			{
				_pauseAtTheEndOfFraseDuration = value;
				RaisePropertyChanged();
			}
		}

		public int PauseAfterLongWordsDuration
		{
			get { return _pauseAfterLongWordsDuration; }
			set
			{
				_pauseAfterLongWordsDuration = value;
				RaisePropertyChanged();
			}
		}

		public bool PauseAfterLongWords
		{
			get { return _pauseAfterLongWords; }
			set
			{
				_pauseAfterLongWords = value;
				RaisePropertyChanged();
			}
		}

		public bool HideSdCardWarningMessage { get; set; }

		public bool PauseAfterPunctuation
		{
			get { return _pauseAfterPunctuation; }
			set
			{
				_pauseAfterPunctuation = value;
				RaisePropertyChanged();
			}
		}

		public int Theme
		{
			get { return _theme; }
			set
			{
				_theme = value;
				RaisePropertyChanged();
			}
		}

		public int FontType
		{
			get { return _fontType; }
			set
			{
				if (value > 3) // para tratar código legado
				{
					_fontType = 2;
				}
				else
				{
					_fontType = value;
				}
				RaisePropertyChanged();
			}
		}

		public bool EnableScreenLock
		{
			get { return _enableScreenLock; }
			set
			{
				_enableScreenLock = value;
				RaisePropertyChanged();
			}
		}

		public bool EnableOrp
		{
			get { return _enableOrp; }
			set
			{
				_enableOrp = value;

				if (value)
				{
					DisplayMoreThanOneWordAtATime = false;
				}
				RaisePropertyChanged();
			}
		}


		public int WordsPerMinute
		{
			get { return _wordsPerMinute; }
			set
			{
				_wordsPerMinute = value;
				RaisePropertyChanged();
			}
		}

		public double FontSize
		{
			get { return _fontSize; }
			set
			{
				_fontSize = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => EnableOrp);
			}
		}

		private bool _displayMoreThanOneWordAtATime;
		public bool DisplayMoreThanOneWordAtATime
		{
			get { return _displayMoreThanOneWordAtATime; }
			set
			{
				_displayMoreThanOneWordAtATime = value;
				if (value)
				{
					EnableOrp = false;
					DisplayGuides = false;
					PauseAfterLongWords = false;
					PauseAfterPunctuation = false;
					PauseAtTheEndOfPhrase = false;
				}
				else
				{
					WordsPerFixation = 1;
				}
				RaisePropertyChanged();
			}
		}

		public static async Task SaveAsync()
		{
			if (!IsLoaded) return;

			var serializer = new XmlSerializer(typeof(SettingsHelper));
			var xmlString = new StringBuilder();
			TextWriter xmlWriter = new StringWriter(xmlString);

			Debug.WriteLine("Serializing Settings");

			try
			{
				serializer.Serialize(xmlWriter, Settings);

				//Debug.WriteLine("Settings: " + xmlString);

				var dataFolder = await Globals.GetDataFolder();
				var file = await dataFolder.CreateFileAsync(SETTINGS_FILE, CreationCollisionOption.OpenIfExists);
				await FileIO.WriteTextAsync(file, xmlString.ToString());
			}
			catch (Exception ex)
			{
				DebugReporter.ReportError(ex);
				Debugger.Break();
			}
			finally
			{
				xmlWriter.Dispose();
			}
		}

		public static async Task RestoreAsync()
		{
			try
			{
				if (IsLoaded) return;
				Debug.WriteLine("Loading Settings");
				var serializer = new XmlSerializer(typeof(SettingsHelper));
				var dataFolder = await Globals.GetDataFolder();

				var file = await dataFolder.CreateFileAsync(SETTINGS_FILE, CreationCollisionOption.OpenIfExists);
				var text = await FileIO.ReadTextAsync(file);
				TextReader textReader = new StringReader(text);
				var xmlReader = XmlReader.Create(textReader);

			    try
			    {
			        if (serializer.CanDeserialize(xmlReader))
			        {
			            _settings = serializer.Deserialize(xmlReader) as SettingsHelper;
			            RaiseLoaded();
			        }
			    }
			    catch(Exception ex)
			    {
			        throw ex;
			    }
			    finally
			    {
			        textReader.Dispose();
			        xmlReader.Dispose();
			    }
			}
			catch(Exception ex)
			{
				DebugReporter.ReportError(ex);
				LoadDefaults();
				RaiseLoaded();
			}
		}

		private static void RaiseLoaded()
		{
			IsLoaded = true;
			var handler = Loaded;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}
		}

		private static void LoadDefaults()
		{
			_settings = new SettingsHelper
			{
				FontSize = 35,
				WordsPerMinute = 250,
				EnableScreenLock = false,
				PauseAfterPunctuation = false,
				EnableOrp = true,
				HideSdCardWarningMessage = false,
				FontType = 2,
				Theme = 1,
				DisplayGuides = true,
				PauseAfterLongWords = false,
				PauseAfterLongWordsDuration = 2,
				PauseAfterPunctuationDuration = 2,
				PauseAtTheEndOfPhrase = false,
				PauseAtTheEndOfFraseDuration = 2,
				WordsPerFixation = 1
			};
			IsLoaded = true;
		}

		public static event EventHandler Loaded;
	}
}