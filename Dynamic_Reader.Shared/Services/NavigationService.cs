using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Dynamic_Reader.ViewModel;
using Dynamic_Reader.Views;

namespace Dynamic_Reader.Services
{
    public class NavigationService : INavigationService
    {
        private Bookmarks _bookmarks;

        private static readonly IDictionary<Type, Type> ViewModelRouting = new Dictionary<Type, Type>
        {
            {
                typeof (MainViewModel), typeof (StartPage)
            },
            {
                typeof (BookViewModel), typeof (ReadingView)
            }
        };

        private static Frame RootFrame
        {
            get { return Window.Current.Content as Frame; }
        }

        public bool CanGoBack
        {
            get { return RootFrame.CanGoBack; }
        }

        public void GoBack()
        {
            RootFrame.GoBack();
        }

        public void Navigate<TDestinationViewModel>( Book book)
        {
            Type dest = ViewModelRouting[typeof (TDestinationViewModel)];

            RootFrame.Navigate(dest, book);
        }

        public void About()
        {
            var about = new About();
            about.ShowIndependent();
        }

        public void Settings()
        {
            var settings = new AppSettingsFlyout();
            settings.ShowIndependent();
        }

        public async void RemoveAds()
        {
            try
            {
#if DEBUG
                PurchaseResults result = await CurrentAppSimulator.RequestProductPurchaseAsync("RemoveAds");
#else
                var result = await CurrentApp.RequestProductPurchaseAsync("RemoveAds");
#endif
                if (result.Status == ProductPurchaseStatus.Succeeded ||
                    result.Status == ProductPurchaseStatus.AlreadyPurchased)
                {
                    App.MainViewModel.DisplayAdvertisements = false;
                }
            }
            catch (Exception ex)
            {
                DebugReporter.ReportError(ex);
            }
        }

        public void Rate()
        {
        }

        public void GoToTableOfContents()
        {
            var toc = new TableOfContentsFlyout();
            toc.ShowIndependent();
        }

        public void PinToStart()
        {
            throw new NotImplementedException();
        }

        public void GoToBookmarks()
        {
            if (_bookmarks == null)
            {
                _bookmarks = new Bookmarks();
            }
            _bookmarks.ShowIndependent();
        }

        public async void GoToBookmark(Bookmark bookmark)
        {
            App.BookViewModel.BookReader.CurrentBook.Chapter = bookmark.Chapter;
            App.BookViewModel.BookReader.CurrentBook.PositionInChapter = bookmark.PositionInChapter;

            var chapterFileName = App.BookViewModel.BookReader.ChapterFiles[bookmark.Chapter];
            await App.BookViewModel.BookReader.ReadChapter(chapterFileName);
            _bookmarks.Hide();
        }
    }
}