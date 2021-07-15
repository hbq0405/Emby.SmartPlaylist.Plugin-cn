using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Playlists;
using SmartPlaylist.Domain;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Library;
using System.Threading;
using System.Collections.Generic;
using SmartPlaylist.Extensions;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Model.Configuration;

namespace SmartPlaylist.Services
{
    public interface IFolderItemsUpdater
    {
        Task UpdateAsync(UserFolder folder, BaseItem[] newItems);
    }



    public class PlayListItemsUpdater : IFolderItemsUpdater
    {
        private readonly IPlaylistManager _playlistManager;

        public PlayListItemsUpdater(IPlaylistManager playlistManager)
        {
            _playlistManager = playlistManager;
        }

        public async Task UpdateAsync(UserFolder folder, BaseItem[] newItems)
        {
            var playlistItems = folder.GetItems();
            if (folder is LibraryUserFolder<Playlist> libraryUserPlaylist)
            {
                RemoveFromPlaylist(libraryUserPlaylist, playlistItems);
                AddToPlaylist(libraryUserPlaylist, newItems);
            }
            else if (newItems.Any())
            {
                await _playlistManager.CreatePlaylist(new PlaylistCreationRequest
                {
                    ItemIdList = newItems.Select(x => x.InternalId).ToArray(),
                    Name = folder.SmartPlaylist.Name,
                    UserId = folder.User.InternalId
                }).ConfigureAwait(false);
            }
        }

        private void RemoveFromPlaylist(LibraryUserFolder<Playlist> playlist, BaseItem[] itemsToRemove)
        {
            _playlistManager.RemoveFromPlaylist(playlist.InternalId,
                itemsToRemove.Select(x => x.ListItemEntryId).ToArray());
        }

        private void AddToPlaylist(LibraryUserFolder<Playlist> playlist, BaseItem[] itemsToAdd)
        {
            _playlistManager.AddToPlaylist(playlist.InternalId,
                itemsToAdd.Select(x => x.InternalId).ToArray(), playlist.User);
        }
    }
}