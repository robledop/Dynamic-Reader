using System;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Model;
using Dynamic_Reader.ViewModel;

namespace Dynamic_Reader.Views
{
    public sealed partial class ReadingView
    {
        private DisplayRequest _dispRequest;
        public ReadingView()
        {
            InitializeComponent();
            Loaded += ReadingView_Loaded;
        }

        private void ReadingView_Loaded(object sender, RoutedEventArgs e)
        {
            if (BottomAppBar != null) BottomAppBar.IsOpen = true;
            if (TopAppBar != null) TopAppBar.IsOpen = true;
            PreviousText.Visibility = Visibility.Visible;
            NextText.Visibility = Visibility.Visible;
            BookPositionSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(BookPositionSlider_OnPointerReleased), true);
        }

        private void BookPositionSlider_OnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            App.BookViewModel.SeekCommand.Execute((int)Math.Floor(BookPositionSlider.Value));
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            await App.MainViewModel.SaveAllAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Refresh)
            {
                var bookViewModel = DataContext as BookViewModel;
                if (bookViewModel != null)
                {
                    var book = e.Parameter as Book;
                    if (e.Parameter != null) if (book != null) await bookViewModel.LoadDataAsync(book);
                }
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            Grid_Tapped(this, new TappedRoutedEventArgs());
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var bookViewModel = DataContext as BookViewModel;
            if (bookViewModel == null) return;

            if (!bookViewModel.BookReader.Stopped)
            {
                bookViewModel.StopCommand.Execute(null);

#if !WINDOWS_PHONE_APP
                if (BottomAppBar != null) BottomAppBar.IsOpen = true;
                if (TopAppBar != null) TopAppBar.IsOpen = true;
#endif
                PreviousText.Visibility = Visibility.Visible;
                NextText.Visibility = Visibility.Visible;

                if (_dispRequest != null && !SettingsHelper.Settings.EnableScreenLock)
                {
                    _dispRequest.RequestRelease();
                    _dispRequest = null;
                }
            }
            else
            {
                bookViewModel.StartCommand.Execute(null);
#if !WINDOWS_PHONE_APP
                if (BottomAppBar != null) BottomAppBar.IsOpen = false;
                if (TopAppBar != null) TopAppBar.IsOpen = false;
#endif
                PreviousText.Visibility = Visibility.Collapsed;
                NextText.Visibility = Visibility.Collapsed;

                if (_dispRequest == null && !SettingsHelper.Settings.EnableScreenLock)
                {

                    _dispRequest = new DisplayRequest();
                    _dispRequest.RequestActive();
                }
            }
        }

        private void CurrentTextBlock_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!App.MainViewModel.Settings.EnableOrp)
            {
                return;
            }
            //Debug.WriteLine("CurrentTextBlock new size: " + e.NewSize);
            if (e.NewSize.Height == 0)
            {
                return;
            }
            var leftMargin = (-(ActualWidth / ((ActualWidth / CurrentTextBlock.FontSize) / 6))) + ActualWidth / 3;

            CurrentTextBlock.Margin = new Thickness(leftMargin, 0, 0, 0);
        }
    }
}