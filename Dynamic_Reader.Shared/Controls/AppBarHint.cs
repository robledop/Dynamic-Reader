using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Dynamic_Reader.Controls
{
    public class AppBarHint : Button
    {
        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                var page = frame.Content as Page;
                if (page != null)
                {
                    if (page.BottomAppBar != null)
                    {
                        page.BottomAppBar.IsOpen = true;
                    }
                }
            }
        }
    }
}