using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Dynamic_Reader.Helpers
{
    public class GridViewHelper
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof (IList), typeof (GridViewHelper),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = (GridView) d;
            ReSetSelectedItems(listBox);
            listBox.SelectionChanged += delegate { ReSetSelectedItems(listBox); };
        }

        private static void ReSetSelectedItems(GridView listBox)
        {
            IList selectedItems = GetSelectedItems(listBox);
            selectedItems.Clear();
            if (listBox.SelectedItems != null)
            {
                foreach (object item in listBox.SelectedItems)
                    selectedItems.Add(item);
            }
        }

        public static IList GetSelectedItems( DependencyObject d)
        {
            if (d == null) throw new ArgumentNullException("d");
            return (IList) d.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems( DependencyObject d,  IList value)
        {
            if (d == null) throw new ArgumentNullException("d");
            if (value == null) throw new ArgumentNullException("value");
            d.SetValue(SelectedItemsProperty, value);
        }
    }
}