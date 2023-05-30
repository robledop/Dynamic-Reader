using Dynamic_Reader.Model;

namespace Dynamic_Reader.Interfaces
{
    public interface INavigationService
    {
        bool CanGoBack { get; }
        void GoBack();
        void Navigate<TDestinationViewModel>(Book book = null);
        void About();
        void Settings();
        void RemoveAds();
        void Rate();
        void GoToTableOfContents();
        void PinToStart();
        void GoToBookmarks();
        void GoToBookmark(Bookmark bookmark);
    }
}