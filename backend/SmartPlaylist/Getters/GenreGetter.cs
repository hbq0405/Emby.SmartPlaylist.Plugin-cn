using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using System;

namespace SmartPlaylist.Domain
{
    public class GenreGetter
    {
        public static string[] Get()
        {
            return Plugin.Instance.LibraryManager.GetGenres(new InternalItemsQuery()).Items.Select(x => x.Item1.ToString())
                .Distinct().OrderBy(x => x).ToArray();
        }
    }
}