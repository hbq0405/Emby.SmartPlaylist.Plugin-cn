using System;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Collections;
using SmartPlaylist.Domain;
using MediaBrowser.Model.Dto;
using System.Collections.Generic;

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

        public abstract UserFolder GetUserPlaylistOrCollectionFolder(Guid userId, string playlistName, SmartType smartType);
    }

    public class FolderRepository : IFolderRepository
    {

        public FolderRepository(IUserManager userManager, ILibraryManager libraryManager)
        : base(userManager, libraryManager) { }

        public override UserFolder GetUserPlaylistOrCollectionFolder(Guid userId, string playlistName, SmartType smartType)
        {
            var user = GetUser(userId);

            var folder = smartType == SmartType.Playlist ?
             FindPlaylist(user, playlistName) :
             FindCollection(user, playlistName);

            return folder != null ? new LibraryUserFolder(user, folder, smartType) : new UserFolder(user, playlistName, smartType);
        }

        public Folder FindCollection(User user, string playlistName)
        {
            return _libraryManager.GetItemsResult(new InternalItemsQuery
            {
                Name = playlistName,
                IncludeItemTypes = new[] { "collections", "Boxset" },
                User = user ////try using the user with policy.IsAdministator for testing               
            }).Items.OfType<Folder>().FirstOrDefault();
        }

        private Playlist FindPlaylist(User user, string playlistName)
        {
            return _libraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                IncludeItemTypes = new[] { typeof(Playlist).Name },
                Name = playlistName,
                Recursive = true
            }).Items.OfType<Playlist>().FirstOrDefault();
        }
    }
}