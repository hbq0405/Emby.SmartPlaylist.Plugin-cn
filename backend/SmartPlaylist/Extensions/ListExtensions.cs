using System.Collections.Generic;

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
    }
}