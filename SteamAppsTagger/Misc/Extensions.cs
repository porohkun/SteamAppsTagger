using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger
{
    public static class Extensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, params T[] items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (predicate == null ? true : predicate(item))
                    return i;
                i++;
            }
            return -1;
        }
    }
}
