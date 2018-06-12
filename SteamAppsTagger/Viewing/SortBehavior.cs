using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SteamAppsTagger
{
    public static class SortBehavior
    {
        #region CanUserSortColumns

        public static readonly DependencyProperty CanUserSortColumnsProperty =
            DependencyProperty.RegisterAttached(
                "CanUserSortColumns",
                typeof(bool),
                typeof(SortBehavior),
                new FrameworkPropertyMetadata(OnCanUserSortColumnsChanged));

        private static void OnCanUserSortColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listView = (ListView)d;
            if ((bool)e.NewValue)
            {
                listView.AddHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)OnColumnHeaderClick);
                if (listView.IsLoaded)
                {
                    DoInitialSort(listView);
                }
                else
                {
                    listView.Loaded += OnLoaded;
                }
            }
            else
            {
                listView.RemoveHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)OnColumnHeaderClick);
            }
        }

        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static bool GetCanUserSortColumns(ListView element)
        {
            return (bool)element.GetValue(CanUserSortColumnsProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static void SetCanUserSortColumns(ListView element, bool value)
        {
            element.SetValue(CanUserSortColumnsProperty, value);
        }

        #endregion //CanUserSortColumns

        #region CanUserSort

        public static readonly DependencyProperty CanUserSortProperty =
            DependencyProperty.RegisterAttached(
                "CanUserSort",
                typeof(bool),
                typeof(SortBehavior),
                new FrameworkPropertyMetadata(true));

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static bool GetCanUserSort(GridViewColumn element)
        {
            return (bool)element.GetValue(CanUserSortProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetCanUserSort(GridViewColumn element, bool value)
        {
            element.SetValue(CanUserSortProperty, value);
        }

        #endregion //CanUserSort

        #region FixedOrder

        public static readonly DependencyProperty FixedOrderProperty =
            DependencyProperty.RegisterAttached(
                "FixedOrder",
                typeof(int),
                typeof(SortBehavior),
                new FrameworkPropertyMetadata(-1));

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static int GetFixedOrder(GridViewColumn element)
        {
            return (int)element.GetValue(FixedOrderProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetFixedOrder(GridViewColumn element, int value)
        {
            element.SetValue(FixedOrderProperty, value);
        }

        #endregion //FixedOrder

        #region SortDirection

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached(
                "SortDirection",
                typeof(ListSortDirection?),
                typeof(SortBehavior));

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static ListSortDirection? GetSortDirection(GridViewColumn element)
        {
            return (ListSortDirection?)element.GetValue(SortDirectionProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetSortDirection(GridViewColumn element, ListSortDirection? value)
        {
            element.SetValue(SortDirectionProperty, value);
        }

        #endregion //SortDirection

        #region SortExpression

        public static readonly DependencyProperty SortExpressionProperty =
            DependencyProperty.RegisterAttached(
                "SortExpression",
                typeof(string),
                typeof(SortBehavior));

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static string GetSortExpression(GridViewColumn element)
        {
            var expression = (string)element.GetValue(SortExpressionProperty);
            if (expression == null)
            {
                expression = ColumnsOrder.GetColumnId(element);
            }
            return expression;
        }

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetSortExpression(GridViewColumn element, string value)
        {
            element.SetValue(SortExpressionProperty, value);
        }

        #endregion //SortExpression

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListView)e.Source;
            listView.Loaded -= OnLoaded;
            DoInitialSort(listView);
        }

        private static void DoInitialSort(ListView listView)
        {
            var gridView = (GridView)listView.View;
            var column = gridView.Columns.FirstOrDefault(c => GetSortDirection(c) != null);
            if (column != null)
            {
                DoSort(listView, column);
            }
        }

        private static void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var columnHeader = e.OriginalSource as GridViewColumnHeader;
            if (columnHeader != null && GetCanUserSort(columnHeader.Column))
            {
                DoSort((ListView)e.Source, columnHeader.Column);
            }
        }

        private static void DoSort(ListView listView, GridViewColumn newColumn)
        {
            var sortDescriptions = listView.Items.SortDescriptions;
            var newDirection = ListSortDirection.Ascending;

            var propertyPath = ResolveSortExpression(newColumn);
            if (propertyPath != null)
            {
                if (sortDescriptions.Count > 0)
                {
                    if (sortDescriptions[0].PropertyName == propertyPath)
                    {
                        newDirection = GetSortDirection(newColumn) == ListSortDirection.Ascending ?
                            ListSortDirection.Descending :
                            ListSortDirection.Ascending;
                    }
                    else
                    {
                        var gridView = (GridView)listView.View;
                        foreach (var column in gridView.Columns.Where(c => GetSortDirection(c) != null))
                        {
                            SetSortDirection(column, null);
                        }
                    }

                    sortDescriptions.Clear();
                }

                sortDescriptions.Add(new SortDescription(propertyPath, newDirection));
                SetSortDirection(newColumn, newDirection);
            }
        }

        private static string ResolveSortExpression(GridViewColumn column)
        {
            var propertyPath = GetSortExpression(column);
            if (propertyPath == null)
            {
                var binding = column.DisplayMemberBinding as Binding;
                return binding != null ? binding.Path.Path : null;
            }

            return propertyPath;
        }
    }
}
