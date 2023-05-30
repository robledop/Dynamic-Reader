
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Store;
using Windows.Globalization;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Dynamic_Reader.ViewModel;
using Dynamic_Reader.Views;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
#if WINDOWS_PHONE_APP
using Dynamic_Reader.WindowsPhone.Common;
using Windows.UI.Xaml.Media.Animation;
using Dynamic_Reader.Common;
using Windows.Phone.UI.Input;
#else
using Windows.UI.ApplicationSettings;
#endif

namespace Dynamic_Reader
{
	sealed partial class App
	{
		private static LicenseInformation _licenseInformation;

		public static LicenseInformation LicenseInformation
		{
			get { return _licenseInformation; }
		}

		public static BookViewModel BookViewModel
		{
			get { return ServiceLocator.Current.GetInstance<BookViewModel>(); }
		}

		public static MainViewModel MainViewModel
		{
			get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
		}

		public App()
		{
			InitializeComponent();
			Suspending += OnSuspending;
#if WINDOWS_PHONE_APP
			HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
		}

#if WINDOWS_PHONE_APP
		void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
			Frame rootFrame = Window.Current.Content as Frame;

			if (rootFrame != null && rootFrame.CanGoBack)
			{
				e.Handled = true;
				rootFrame.GoBack();
			}
		}
#endif

		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
			await MainViewModel.SaveAllAsync();
			deferral.Complete();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
#if DEBUG
			_licenseInformation = CurrentAppSimulator.LicenseInformation;

			if (Debugger.IsAttached)
			{
				DebugSettings.EnableFrameRateCounter = true;
			}
#else
			_licenseInformation = CurrentApp.LicenseInformation;
#endif
			var rootFrame = Window.Current.Content as Frame;

			if (rootFrame == null)
			{
				rootFrame = new Frame { Language = ApplicationLanguages.Languages[0] };

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null)
			{
				rootFrame.Navigate(typeof(StartPage), e.Arguments);
			}
			Window.Current.Activate();
			DispatcherHelper.Initialize();
		}

		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}


		protected override async void OnFileActivated(FileActivatedEventArgs args)
		{
			var filesToImport = new List<StorageFile>();
			foreach (var storageItem in args.Files)
			{
				var item = (StorageFile)storageItem;
				filesToImport.Add(item);
			}
			await ImportBooksAsync(filesToImport);

			MainViewModel.RefreshCommand.Execute(null);
#if DEBUG
			_licenseInformation = CurrentAppSimulator.LicenseInformation;

			if (Debugger.IsAttached)
			{
				DebugSettings.EnableFrameRateCounter = true;
			}
#else
			_licenseInformation = CurrentApp.LicenseInformation;

#endif

			Frame rootFrame = Window.Current.Content as Frame;

			if (rootFrame == null)
			{
				rootFrame = new Frame();
				rootFrame.Language = ApplicationLanguages.Languages[0];

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null)
			{
				rootFrame.Navigate(typeof(StartPage));
			}
			Window.Current.Activate();
		}

		private async Task ImportBooksAsync(IReadOnlyList<StorageFile> books)
		{
			var booksFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Books", CreationCollisionOption.OpenIfExists);

			var importQueue = new Queue<StorageFile>(books);

			while (importQueue.Count > 0)
			{
				var book = importQueue.Dequeue();
				await book.CopyAsync(booksFolder, book.Name, NameCollisionOption.ReplaceExisting);
			}
		}

#if WINDOWS_PHONE_APP
		protected async override void OnActivated(IActivatedEventArgs e)
		{
			base.OnActivated(e);

			var continuationManager = new ContinuationManager();

			Frame rootFrame = CreateRootFrame();
			await RestoreStatusAsync(e.PreviousExecutionState);

			if (rootFrame.Content == null)
			{
				rootFrame.Navigate(typeof(Library));
			}

			var continuationEventArgs = e as IContinuationActivatedEventArgs;
			if (continuationEventArgs != null)
			{
				Frame frame = StartPage.Current.Frame;
				if (frame != null)
				{
					// Call ContinuationManager to handle continuation activation
					continuationManager.Continue(continuationEventArgs, frame);
				}
			}

			Window.Current.Activate();
		}

		private Frame CreateRootFrame()
		{
			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				// Set the default language
				rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
				rootFrame.NavigationFailed += OnNavigationFailed;

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			return rootFrame;
		}
		private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
		{
			if (previousExecutionState == ApplicationExecutionState.Terminated)
			{
				try
				{
					await SuspensionManager.RestoreAsync();
				}
				catch (SuspensionManagerException)
				{
					//Something went wrong restoring state.
					//Assume there is no state and continue
				}
			}
		}

		public ContinuationManager ContinuationManager { get; private set; }
		private TransitionCollection transitions;

		private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
		{
			var rootFrame = sender as Frame;
			rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
			rootFrame.Navigated -= this.RootFrame_FirstNavigated;
		}
#else
				protected override void OnWindowCreated(WindowCreatedEventArgs args)
		{
			SettingsPane.GetForCurrentView().CommandsRequested += (s, e) =>
			{
				var settingsCommand = new SettingsCommand("Settings", "Settings", command =>
				{
					var flyout = new AppSettingsFlyout();
					flyout.Show();
				});

				var aboutCommand = new SettingsCommand("About", "About", command =>
				{
					var flyout = new About();
					flyout.Show();
				});

				var privacyPolicyCommand = new SettingsCommand("privacyPolicy", "Privacy Policy",
					uiCommand => { LaunchPrivacyPolicyUrl(); });
				e.Request.ApplicationCommands.Add(privacyPolicyCommand);

				e.Request.ApplicationCommands.Add(settingsCommand);
				e.Request.ApplicationCommands.Add(aboutCommand);
			};
		}

		private async void LaunchPrivacyPolicyUrl()
		{
			var privacyPolicyUrl = new Uri("http://myapppolicy.com/app/dynamicreader");
			await Launcher.LaunchUriAsync(privacyPolicyUrl);
		}
#endif
	}
}