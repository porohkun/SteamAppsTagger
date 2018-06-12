using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;

namespace SteamAppsTagger
{
    public partial class ColumnsOrder : Decorator
    {
        #region ColumnId

        public static readonly DependencyProperty ColumnIdProperty = DependencyProperty.RegisterAttached(
            "ColumnId",
            typeof(string),
            typeof(ColumnsOrder),
            new PropertyMetadata(""));

        public static void SetColumnId(GridViewColumn element, string value)
        {
            element.SetValue(ColumnIdProperty, value);
        }

        public static string GetColumnId(GridViewColumn element)
        {
            return (string)element.GetValue(ColumnIdProperty);
        }

        #endregion //ColumnId

        #region ItemsSource

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ColumnsOrder),
            new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ColumnsOrder;
            if (control != null)
                control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
            }

            var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
            }

            if (Grid != null)
            {
                _staticColumnsCount = Grid.Columns.Count;

                Grid.Columns.CollectionChanged += gridView_CollectionChanged;
                CreateColumnsFrom((IEnumerable<string>)newValue);
                ReorderColumns();
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CreateColumnsFrom((IEnumerable<string>)ItemsSource);
            ReorderColumns();
        }

        private void CreateColumnsFrom(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (!Grid.Columns.Any(c => c.Header.Equals(key)))
                {
                    var expression = string.Format(ColumnsFormat, key);
                    var binding = new Binding(expression);
                    var column = new GridViewColumn()
                    {
                        Header = key,
                        DisplayMemberBinding = binding
                    };
                    SetColumnId(column, key);
                    SortBehavior.SetSortExpression(column, expression);
                    Grid.Columns.Add(column);
                }
            }
        }

        #endregion //ItemsSource

        public string ColumnsFormat { get; set; }

        private ListView _list;
        public ListView List
        {
            get
            {
                if (_list != Child)
                {
                    _list = Child as ListView;
                }
                return _list;
            }
        }

        private GridView _grid;
        public GridView Grid
        {
            get
            {
                if (List != null && _grid != List.View)
                {
                    _grid = List.View as GridView;

                }
                return _grid;
            }
        }

        private bool _lockGridColumnsCollectionChanged;
        private int _staticColumnsCount = 0;

        Dictionary<string, GridViewColumn> storedColumns = new Dictionary<string, GridViewColumn>();

        private void ReorderColumns()
        {
            _lockGridColumnsCollectionChanged = true;

            for (int c = 0; c < Grid.Columns.Count; c++)
            {
                var column = Grid.Columns.IndexOf(col => SortBehavior.GetFixedOrder(col) == c);
                if (column == -1)
                    break;
                Grid.Columns.Move(column, c);
            }

            int i = 0;
            lock (ItemsSource)
            {
                foreach (string columnData in ItemsSource)
                {
                    for (int j = i; j < Grid.Columns.Count; j++)
                    {
                        var column = Grid.Columns[j];
                        if (GetColumnId(column) == columnData)
                        {
                            Grid.Columns.Move(j, i + _staticColumnsCount);
                            i++;
                            break;
                        }
                    }
                }
            }
            _lockGridColumnsCollectionChanged = false;
        }

        private void gridView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_lockGridColumnsCollectionChanged && e.Action == NotifyCollectionChangedAction.Move)
            {
                var oldIndex = e.OldStartingIndex - _staticColumnsCount;
                var newIndex = Math.Max(0, e.NewStartingIndex - _staticColumnsCount);
                if (oldIndex >= 0)
                {
                    var collection = (ObservableCollection<string>)ItemsSource;
                    collection.Move(oldIndex, newIndex);
                }
                else if (SortBehavior.GetFixedOrder(Grid.Columns[e.NewStartingIndex]) != -1)
                    ReorderColumns();
            }
        }

    }
}
