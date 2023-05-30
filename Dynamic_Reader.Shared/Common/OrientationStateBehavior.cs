using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace Dynamic_Reader.Common
{
    public class OrientationStateBehavior : OrientationStateControlBehavior
    {
        /// <summary>
        ///     The <see cref="SnapViewMaximumWidth" /> dependency property's name.
        /// </summary>
        public const string SNAP_VIEW_MAXIMUM_WIDTH_PROPERTY_NAME = "SnapViewMaximumWidth";

        /// <summary>
        ///     Identifies the <see cref="SnapViewMaximumWidth" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SnapViewMaximumWidthProperty = DependencyProperty.Register(
            SNAP_VIEW_MAXIMUM_WIDTH_PROPERTY_NAME,
            typeof (double),
            typeof (OrientationStateBehavior),
            new PropertyMetadata(500.0));

        private Page _associatedPage;

        /// <summary>
        ///     Gets or sets the value of the <see cref="SnapViewMaximumWidth" />
        ///     property. This is a dependency property.
        /// </summary>
        public double SnapViewMaximumWidth
        {
            get { return (double) GetValue(SnapViewMaximumWidthProperty); }
            set { SetValue(SnapViewMaximumWidthProperty, value); }
        }

        public override void Attach( DependencyObject associatedObject)
        {
            if (associatedObject == null) throw new ArgumentNullException("associatedObject");
            AssociatedObject = associatedObject;
            _associatedPage = AssociatedObject as Page;

            if (_associatedPage == null)
            {
                throw new InvalidOperationException(
                    "OrientationStateBehavior can only be attached to a Page.");
            }

            _associatedPage.Loaded += AssociatedPageLoaded;
        }

        private async void AssociatedPageLoaded(object sender, RoutedEventArgs e)
        {
            _associatedPage.Loaded -= AssociatedPageLoaded;
            _associatedPage.SizeChanged += PageBaseSizeChanged;
            DisplayInformation.GetForCurrentView().OrientationChanged += PageBaseOrientationChanged;
            await DispatcherHelper.RunAsync(CheckOrientationForPage);
        }

        private void PageBaseSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ViewModelBase.IsInDesignModeStatic)
            {
                CheckOrientationForPage();
            }
        }

        private void CheckOrientationForPage()
        {
            if (!ViewModelBase.IsInDesignModeStatic)
            {
                if (_associatedPage.ActualWidth < SnapViewMaximumWidth)
                {
                    if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Portrait
                        && DisplayInformation.GetForCurrentView().CurrentOrientation
                        != DisplayOrientations.PortraitFlipped)
                    {
                        HandleOrientation(PageOrientations.Snap);
                    }
                }
                else
                {
                    HandleOrientation(DisplayInformation.GetForCurrentView().CurrentOrientation.GetPageOrientation());
                }
            }
        }

        private void PageBaseOrientationChanged(DisplayInformation sender, object args)
        {
            CheckOrientationForPage();
        }

        public override void Detach()
        {
            if (_associatedPage != null)
            {
                _associatedPage.SizeChanged -= PageBaseSizeChanged;
                DisplayInformation.GetForCurrentView().OrientationChanged -= PageBaseOrientationChanged;
            }
        }

        protected override void SendMessage(PageOrientations orientation)
        {
            Messenger.Default.Send(new OrientationStateMessage(orientation));
        }
    }
}