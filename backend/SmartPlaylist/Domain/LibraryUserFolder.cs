using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Playlists;

namespace SmartPlaylist.Domain
{
    public class LibraryUserFolder<T> : UserFolder where T : Folder
    {
        private readonly T _item;
        public T Item => _item;
        public LibraryUserFolder(User user, T folder, SmartPlaylist smartPlaylist) : base(user, smartPlaylist)
        {
            _item = folder;
        }

        public long InternalId => _item.InternalId;

        public override BaseItem[] GetItems()
        {
            return _item.GetChildren(User).ToArray();
        }
    }
}