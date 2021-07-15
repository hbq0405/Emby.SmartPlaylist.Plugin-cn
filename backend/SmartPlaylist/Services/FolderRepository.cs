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

        public IFolderRepository(IUserManager userManager, ILibraryManager libraryManager)
        {
            _userManager = userManager;
            _libraryManager = libraryManager;
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
    }

    public class FolderRepository : IFolderRepository
    {

        public FolderRepository(IUserManager userManager, ILibraryManager libraryManager)
        : base(userManager, libraryManager) { }

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
            var folder = FindCollectionFolder(smartPlaylist, collectionName);

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
            var user = _userManager.GetUserById(smartPlaylist.UserId);
            var folder = _libraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                IncludeItemTypes = new[] { typeof(Playlist).Name },
                Name = smartPlaylist.Name,
                Recursive = true
            }).Items.OfType<Playlist>().FirstOrDefault();

            return folder != null
                ? new LibraryUserFolder<Playlist>(user, folder, smartPlaylist)
                : new UserFolder(user, smartPlaylist);
        }
    }
}