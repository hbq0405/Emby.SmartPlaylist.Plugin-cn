using System;
using System.Collections.Generic;
using System.Linq;
namespace SmartPlaylist.Extensions
{
    public static class ListExtensions
    {
        public static List<T> ConcateList<T>(params IEnumerable<T>[] e)
        {
            List<T> ret = new List<T>();
            foreach (IEnumerable<T> ee in e)
                ret.AddRange(ee);
            return ret;
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IList<T> source, int size)
        {
            for (int i = 0; i < Math.Ceiling(source.Count / (double)size); i++)
                yield return source.Skip(size * i).Take(size);
        }
    }
}