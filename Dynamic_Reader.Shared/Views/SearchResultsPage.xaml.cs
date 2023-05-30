using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Dynamic_Reader.Common;
using Dynamic_Reader.Model;
using Dynamic_Reader.Readers;

namespace Dynamic_Reader.Views
{
    public sealed partial class SearchResultsPage
    {
        private readonly ObservableDictionary _defaultViewModel = new ObservableDictionary();
        private readonly NavigationHelper _navigationHelper;

        public SearchResultsPage()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += navigationHelper_LoadState;
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var queryText = e.NavigationParameter as String;


            IEnumerable<Book> result = from b in App.MainViewModel.Books
                where queryText != null && b.Title.ToLower().Contains(queryText.ToLower())
                select b;

            var filterList = new List<Filter>
            {
                new Filter("All", App.MainViewModel.Books.Count()),
                new Filter("Results", result.Count(), true)
            };

            // Communicate results through the view model
            //this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            DefaultViewModel["QueryText"] = queryText;
            DefaultViewModel["Filters"] = filterList;
            DefaultViewModel["ShowFilters"] = filterList.Count > 1;
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                object filter = frameworkElement.DataContext;

                if (FiltersViewSource.View != null)
                {
                    FiltersViewSource.View.MoveCurrentTo(filter);
                }

                var selectedFilter = filter as Filter;
                if (selectedFilter != null)
                {
                    selectedFilter.Active = true;

                    if (selectedFilter.Name == "Results")
                    {
                        var queryText = DefaultViewModel["QueryText"] as string;

                        IEnumerable<Book> result = from b in App.MainViewModel.Books
                            where queryText != null && b.Title.ToLower().Contains(queryText.ToLower())
                            select b;

                        var filteredResults = new ObservableCollection<Book>(result);

                        DefaultViewModel["Results"] = filteredResults;
                    }
                    else if (selectedFilter.Name == "All")
                    {
                        DefaultViewModel["Results"] = App.MainViewModel.Books;
                    }


                    // Ensure results are found
                    object results;
                    ICollection resultsCollection;
                    if (DefaultViewModel.TryGetValue("Results", out results) &&
                        (resultsCollection = results as ICollection) != null &&
                        resultsCollection.Count != 0)
                    {
                        VisualStateManager.GoToState(this, "ResultsFound", true);
                        return;
                    }
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        private void resultsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((GridView) sender).SelectedItem != null)
            {
                if (BottomAppBar != null) BottomAppBar.IsOpen = true;
            }
            else
            {
                if (BottomAppBar != null) BottomAppBar.IsOpen = false;
            }
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        /// <summary>
        ///     View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : INotifyPropertyChanged
        {
            private bool _active;
            private int _count;
            private String _name;

            public Filter( String name, int count, bool active = false)
            {
                if (name == null) throw new ArgumentNullException("name");
                Name = name;
                Count = count;
                Active = active;
            }

            public String Name
            {
                get { return _name; }
                set { if (SetProperty(ref _name, value)) OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (SetProperty(ref _count, value)) OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }

            /// <summary>
            ///     Multicast event for property change notifications.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            public override String ToString()
            {
                return Description;
            }

            /// <summary>
            ///     Checks if a property already matches a desired value.  Sets the property and
            ///     notifies listeners only when necessary.
            /// </summary>
            /// <typeparam name="T">Type of the property.</typeparam>
            /// <param name="storage">Reference to a property with both getter and setter.</param>
            /// <param name="value">Desired value for the property.</param>
            /// <param name="propertyName">
            ///     Name of the property used to notify listeners.  This
            ///     value is optional and can be provided automatically when invoked from compilers that
            ///     support CallerMemberName.
            /// </param>
            /// <returns>
            ///     True if the value was changed, false if the existing value matched the
            ///     desired value.
            /// </returns>
            private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
            {
                if (Equals(storage, value)) return false;

                storage = value;
                if (propertyName != null) OnPropertyChanged(propertyName);
                return true;
            }

            /// <summary>
            ///     Notifies listeners that a property value has changed.
            /// </summary>
            /// <param name="propertyName">
            ///     Name of the property used to notify listeners.  This
            ///     value is optional and can be provided automatically when invoked from compilers
            ///     that support <see cref="CallerMemberNameAttribute" />.
            /// </param>
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChangedEventHandler eventHandler = PropertyChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}