using System;
using MediaBrowser.Controller.Entities;
using SmartPlaylist.Domain;
using SmartPlaylist.Infrastructure;
using SmartPlaylist.Services;

namespace SmartPlaylist.PerfLoggerDecorators.Services
{
    public class PlaylistRepositoryPerfLoggerDecorator : IFolderRepository
    {
        private readonly IFolderRepository _decorated;

        public PlaylistRepositoryPerfLoggerDecorator(IFolderRepository decorated)
        {
            _decorated = decorated;
        }

        public override UserFolder FindCollection(Domain.SmartPlaylist smartPlaylist)
        {
            using (PerfLogger.Create("FindCollection", () => new { smartPlaylistName = smartPlaylist.Name }))
            {
                return _decorated.FindCollection(smartPlaylist);
            }
        }

        public override UserFolder FindCollection(Domain.SmartPlaylist smartPlaylist, string collectionName)
        {
            using (PerfLogger.Create("FindCollection", () => new { smartPlaylistName = smartPlaylist.Name }))
            {
                return _decorated.FindCollection(smartPlaylist);
            }
        }

        public override Folder FindCollectionFolder(Domain.SmartPlaylist smartPlaylist, string collectionName)
        {
            using (PerfLogger.Create("FindCollectionFolder", () => new { smartPlaylistName = smartPlaylist.Name }))
            {
                return _decorated.FindCollectionFolder(smartPlaylist, collectionName);
            }
        }

        public override UserFolder FindPlaylist(Domain.SmartPlaylist smartPlaylist)
        {
            using (PerfLogger.Create("FindUserPlaylist", () => new { smartPlaylistName = smartPlaylist.Name }))
            {
                return _decorated.FindPlaylist(smartPlaylist);
            }
        }

        public override UserFolder GetUserPlaylistOrCollectionFolder(Domain.SmartPlaylist smartPlaylist)
        {
            using (PerfLogger.Create("GetUserPlaylist", () => new { smartPlaylistName = smartPlaylist.Name }))
            {
                return _decorated.GetUserPlaylistOrCollectionFolder(smartPlaylist);
            }
        }
    }
}