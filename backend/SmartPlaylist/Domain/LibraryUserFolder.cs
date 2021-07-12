using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Playlists;

namespace SmartPlaylist.Domain
{
    public class LibraryUserFolder : UserFolder
    {
        private readonly Folder _item;
        public Folder Item => _item;
        public LibraryUserFolder(User user, Folder folder, SmartType smartType) : base(user, folder.Name, smartType)
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