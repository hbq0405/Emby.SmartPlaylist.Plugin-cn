using System;
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

        public override UserFolder GetUserPlaylistOrCollectionFolder(Guid userId, string playlistName, SmartType smartType)
        {
            using (PerfLogger.Create("GetUserPlaylist", () => new { smartPlaylistName = playlistName }))
            {
                return _decorated.GetUserPlaylistOrCollectionFolder(userId, playlistName, smartType);
            }
        }
    }
}