using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xaml.Interactivity;

namespace Dynamic_Reader.Common
{
    public class OrientationStateControlBehavior : DependencyObject, IBehavior
    {
        /// <summary>
        ///     The <see cref="LandscapeFlippedStateName" /> dependency property's name.
        /// </summary>
        public const string LANDSCAPE_FLIPPED_STATE_NAME_PROPERTY_NAME = "LandscapeFlippedStateName";

        /// <summary>
        ///     The <see cref="LandscapeStateName" /> dependency property's name.
        /// </summary>
        public const string LANDSCAPE_STATE_NAME_PROPERTY_NAME = "LandscapeStateName";

        /// <summary>
        ///     The <see cref="PortraitFlippedStateName" /> dependency property's name.
        /// </summary>
        public const string PORTRAIT_FLIPPED_STATE_NAME_PROPERTY_NAME = "PortraitFlippedStateName";

        /// <summary>
        ///     The <see cref="PortraitStateName" /> dependency property's name.
        /// </summary>
        public const string PORTRAIT_STATE_NAME_PROPERTY_NAME = "PortraitStateName";

        /// <summary>
        ///     The <see cref="SnapStateName" /> dependency property's name.
        /// </summary>
        public const string SNAP_STATE_NAME_PROPERTY_NAME = "SnapStateName";

        /// <summary>
        ///     Identifies the <see cref="LandscapeFlippedStateName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty LandscapeFlippedStateNameProperty = DependencyProperty.Register(
            LANDSCAPE_FLIPPED_STATE_NAME_PROPERTY_NAME,
            typeof (string),
            typeof (OrientationStateBehavior),
            new PropertyMetadata("Landscape"));

        /// <summary>
        ///     Identifies the <see cref="LandscapeStateName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty LandscapeStateNameProperty = DependencyProperty.Register(
            LANDSCAPE_STATE_NAME_PROPERTY_NAME,
            typeof (string),
            typeof (OrientationStateBehavior),
            new PropertyMetadata("Landscape"));

        /// <summary>
        ///     Identifies the <see cref="PortraitFlippedStateName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty PortraitFlippedStateNameProperty = DependencyProperty.Register(
            PORTRAIT_FLIPPED_STATE_NAME_PROPERTY_NAME,
            typeof (string),
            typeof (OrientationStateBehavior),
            new PropertyMetadata("PortraitFlipped"));

        /// <summary>
        ///     Identifies the <see cref="PortraitStateName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty PortraitStateNameProperty = DependencyProperty.Register(
            PORTRAIT_STATE_NAME_PROPERTY_NAME,
            typeof (string),
            typeof (OrientationStateBehavior),
            new PropertyMetadata("Portrait"));

        /// <summary>
        ///     Identifies the <see cref="SnapStateName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SnapStateNameProperty = DependencyProperty.Register(
            SNAP_STATE_NAME_PROPERTY_NAME,
            typeof (string),
            typeof (OrientationStateBehavior),
            new PropertyMetadata("Snap"));

        private UserControl _associatedControl;

        public DependencyObject AssociatedObject { get; protected set; }

        /// <summary>
        ///     Gets or sets the value of the <see cref="LandscapeFlippedStateName" />
        ///     property. This is a dependency property.
        /// </summary>
        public string LandscapeFlippedStateName
        {
            get { return (string) GetValue(LandscapeFlippedStateNameProperty); }
            set { SetValue(LandscapeFlippedStateNameProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the value of the <see cref="LandscapeStateName" />
        ///     property. This is a dependency property.
        /// </summary>
        public string LandscapeStateName
        {
            get { return (string) GetValue(LandscapeStateNameProperty); }
            set { SetValue(LandscapeStateNameProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the value of the <see cref="PortraitFlippedStateName" />
        ///     property. This is a dependency property.
        /// </summary>
        public string PortraitFlippedStateName
        {
            get { return (string) GetValue(PortraitFlippedStateNameProperty); }
            set { SetValue(PortraitFlippedStateNameProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the value of the <see cref="PortraitStateName" />
        ///     property. This is a dependency property.
        /// </summary>
        public string PortraitStateName
        {
            get { return (string) GetValue(PortraitStateNameProperty); }
            set { SetValue(PortraitStateNameProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the value of the <see cref="SnapStateName" />
        ///     property. This is a dependency property.
        /// </summary>
        public string SnapStateName
        {
            get { return (string) GetValue(SnapStateNameProperty); }
            set { SetValue(SnapStateNameProperty, value); }
        }

        public virtual void Attach( DependencyObject associatedObject)
        {
            if (associatedObject == null) throw new ArgumentNullException("associatedObject");
            AssociatedObject = associatedObject;
            _associatedControl = AssociatedObject as UserControl;

            if (_associatedControl == null)
            {
                throw new InvalidOperationException(
                    "OrientationStateControlBehavior can only be attached to a UserControl.");
            }

            Messenger.Default.Register<OrientationStateMessage>(
                this,
                HandleOrientationMessage);
        }

        private void HandleOrientationMessage(OrientationStateMessage message)
        {
            if (_associatedControl == null)
            {
                return;
            }

            HandleOrientation(message.Orientation);
        }

        protected void HandleOrientation(PageOrientations currentOrientation)
        {
            var control = (Control) AssociatedObject;

            switch (currentOrientation)
            {
                case PageOrientations.Portrait:
                    VisualStateManager.GoToState(control, PortraitStateName, true);
                    SendMessage(PageOrientations.Portrait);
                    break;

                case PageOrientations.PortraitFlipped:
                    VisualStateManager.GoToState(control, PortraitFlippedStateName, true);
                    SendMessage(PageOrientations.PortraitFlipped);
                    break;

                case PageOrientations.Landscape:
                    VisualStateManager.GoToState(control, LandscapeStateName, true);
                    SendMessage(PageOrientations.Landscape);
                    break;

                case PageOrientations.LandscapeFlipped:
                    VisualStateManager.GoToState(control, LandscapeFlippedStateName, true);
                    SendMessage(PageOrientations.LandscapeFlipped);
                    break;

                case PageOrientations.Snap:
                    VisualStateManager.GoToState(control, SnapStateName, true);
                    SendMessage(PageOrientations.Snap);
                    break;

                default:
                    VisualStateManager.GoToState(control, LandscapeStateName, true);
                    SendMessage(PageOrientations.Landscape);
                    break;
            }
        }

        protected virtual void SendMessage(PageOrientations orientation)
        {
        }

        public virtual void Detach()
        {
            Messenger.Default.Unregister<OrientationStateMessage>(
                this);
        }
    }
}