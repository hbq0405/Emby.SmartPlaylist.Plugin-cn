using System;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using SmartPlaylist.Domain;

namespace SmartPlaylist.Services
{
    public abstract class IFolderRepository
    {

        internal readonly ILibraryManager _libraryManager;
        internal readonly IUserManager _userManager;
        internal readonly IFolderItemsUpdater _collectionItemUpdater;
        internal readonly IFolderItemsUpdater _playListItemsUpdater;

        public IFolderRepository(IUserManager userManager, ILibraryManager libraryManager,
        IFolderItemsUpdater collectionItemUpdater, IFolderItemsUpdater playListItemsUpdater)
        {
            _userManager = userManager;
            _libraryManager = libraryManager;
            _collectionItemUpdater = collectionItemUpdater;
            _playListItemsUpdater = playListItemsUpdater;
        }

        public IFolderRepository() { }

        internal User GetUser(Guid userId)
        {
            return _userManager.GetUserById(userId);
        }

        public abstract UserFolder GetUserPlaylistOrCollectionFolder(Domain.SmartPlaylist smartPlaylist);
        public abstract UserFolder FindCollection(Domain.SmartPlaylist smartPlaylist);
        public abstract UserFolder FindCollection(Domain.SmartPlaylist smartPlaylist, string collectionName);
        public abstract Folder FindCollectionFolder(Domain.SmartPlaylist smartPlaylist, string collectionName);
        public abstract UserFolder FindPlaylist(Domain.SmartPlaylist smartPlaylist);
        public abstract UserFolder FindPlaylist(Domain.SmartPlaylist smartPlaylist, string playlistName);
        public abstract Playlist FindPlaylistFolder(Domain.SmartPlaylist smartPlaylist, string playlistName);
        public abstract void Remove(Domain.SmartPlaylist smartPlaylist);
    }

    public class FolderRepository : IFolderRepository
    {

        public FolderRepository(IUserManager userManager, ILibraryManager libraryManager,
            IFolderItemsUpdater collectionItemUpdater, IFolderItemsUpdater playListItemsUpdater)
        : base(userManager, libraryManager, collectionItemUpdater, playListItemsUpdater) { }

        public override UserFolder GetUserPlaylistOrCollectionFolder(Domain.SmartPlaylist smartPlaylist)
        {
            return smartPlaylist.SmartType == SmartType.Playlist
                ? FindPlaylist(smartPlaylist)
                : FindCollection(smartPlaylist);
        }

        public override UserFolder FindCollection(Domain.SmartPlaylist smartPlaylist)
        {
            return FindCollection(smartPlaylist, smartPlaylist.Name);
        }
        public override UserFolder FindCollection(Domain.SmartPlaylist smartPlaylist, string collectionName)
        {
            var user = _userManager.GetUserById(smartPlaylist.UserId);

            if (smartPlaylist.ForceCreate)
                return new UserFolder(user, smartPlaylist);

            Folder folder = null;
            if (smartPlaylist.InternalId > 0)
                folder = (Folder)_libraryManager.GetItemById(smartPlaylist.InternalId);

            if (folder == null)
                folder = FindCollectionFolder(smartPlaylist, collectionName);

            return folder != null
                ? new LibraryUserFolder<Folder>(user, folder, smartPlaylist)
                : new UserFolder(user, smartPlaylist);
        }

        public override Folder FindCollectionFolder(Domain.SmartPlaylist smartPlaylist, string collectionName)
        {
            var user = _userManager.GetUserById(smartPlaylist.UserId);

            return _libraryManager.GetItemsResult(new InternalItemsQuery
            {
                Name = collectionName,
                IncludeItemTypes = new[] { "collections", "Boxset" },
                User = user ////try using the user with policy.IsAdministator for testing               
            }).Items.OfType<Folder>().FirstOrDefault();
        }

        public override UserFolder FindPlaylist(Domain.SmartPlaylist smartPlaylist)
        {
            return FindPlaylist(smartPlaylist, smartPlaylist.Name);
        }

        public override UserFolder FindPlaylist(Domain.SmartPlaylist smartPlaylist, string playlistName)
        {
            var user = _userManager.GetUserById(smartPlaylist.UserId);

            if (smartPlaylist.ForceCreate)
                return new UserFolder(user, smartPlaylist);

            Playlist folder = null;
            if (smartPlaylist.InternalId > 0)
                folder = (Playlist)_libraryManager.GetItemById(smartPlaylist.InternalId);

            if (folder == null)
                folder = FindPlaylistFolder(smartPlaylist, playlistName);

            return folder != null
                ? new LibraryUserFolder<Playlist>(user, folder, smartPlaylist)
                : new UserFolder(user, smartPlaylist);
        }

        public override Playlist FindPlaylistFolder(Domain.SmartPlaylist smartPlaylist, string playlistName)
        {
            var user = _userManager.GetUserById(smartPlaylist.UserId);

            return _libraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                IncludeItemTypes = new[] { typeof(Playlist).Name },
                Name = playlistName,
                Recursive = true
            }).Items.OfType<Playlist>().FirstOrDefault();
        }

        public override void Remove(Domain.SmartPlaylist smartPlaylist)
        {
            if (smartPlaylist.OriginalSmartType == SmartType.Collection)
                Remove<Folder>(smartPlaylist, _collectionItemUpdater, GetUser(smartPlaylist.UserId), () => { return FindCollectionFolder(smartPlaylist, smartPlaylist.Name); });
            else if (smartPlaylist.OriginalSmartType == SmartType.Playlist)
                Remove<Playlist>(smartPlaylist, _playListItemsUpdater, GetUser(smartPlaylist.UserId), () => { return FindPlaylistFolder(smartPlaylist, smartPlaylist.Name); });
        }

        private void Remove<T>(Domain.SmartPlaylist smartPlaylist, IFolderItemsUpdater folderItemsUpdater, User user, Func<Folder> action) where T : Folder
        {
            BaseItem item = smartPlaylist.InternalId > 0 ? _libraryManager.GetItemById(smartPlaylist.InternalId) : action();
            if (item != null)
            {
                if (item is T)
                {
                    LibraryUserFolder<T> folder = new LibraryUserFolder<T>(user, (T)item, smartPlaylist);
                    folderItemsUpdater.RemoveItems(folder, folder.GetItems());
                }

                try
                {
                    _libraryManager.DeleteItem(item, new DeleteOptions()
                    {
                        DeleteFileLocation = true
                    });
                }
                catch (Exception) { }//TODO: This needs to be fixed, as it work intermittently, but we don't want to crash out.
            }
        }
    }
}