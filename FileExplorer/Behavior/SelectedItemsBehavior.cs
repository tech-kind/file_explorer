using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FileExplorer.Behavior
{
    // [TypeConstraint(typeof(Selector))]
    public class SelectedItemsBehavior : Behavior<DataGrid>
    {
        public List<object> SelectedItems
        {
            get { return (List<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems"
                , typeof(List<object>)
                , typeof(SelectedItemsBehavior)
                ,
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject != null)
                this.AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems = AssociatedObject.SelectedItems.Cast<object>().ToList();
        }
    }
}
