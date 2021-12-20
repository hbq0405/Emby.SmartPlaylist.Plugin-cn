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
        Task<(long internalId, string message)> UpdateAsync(UserFolder folder, BaseItem[] newItems);
        void RemoveItems(UserFolder folder, BaseItem[] itemsToRemove);
    }

    public class PlayListItemsUpdater : IFolderItemsUpdater
    {
        private readonly IPlaylistManager _playlistManager;

        public PlayListItemsUpdater(IPlaylistManager playlistManager)
        {
            _playlistManager = playlistManager;
        }

        public async Task<(long internalId, string message)> UpdateAsync(UserFolder folder, BaseItem[] newItems)
        {
            (long internalId, string message) ret = (0, string.Empty);
            var playlistItems = folder.GetItems();

            if (folder is LibraryUserFolder<Playlist> libraryUserPlaylist)
            {
                RemoveItems(libraryUserPlaylist, playlistItems);
                AddToPlaylist(libraryUserPlaylist, newItems);
                libraryUserPlaylist.Item.Name = folder.SmartPlaylist.Name;
                ret = (libraryUserPlaylist.InternalId, $"Completed - (Added {newItems.Count()} to existing playlist)");
            }
            else if (newItems.Any())
            {
                PlaylistCreationResult request = await _playlistManager.CreatePlaylist(new PlaylistCreationRequest
                {

                    ItemIdList = newItems.Select(x => x.InternalId).ToArray(),
                    Name = folder.SmartPlaylist.Name,
                    UserId = folder.User.InternalId
                }).ConfigureAwait(false);

                ret = (long.Parse(request.Id), $"Completed - (Added {newItems.Count()} to new playlist)");
            }
            else
                ret = (-1, "Completed - (No new items found)");

            return ret;
        }

        private void AddToPlaylist(LibraryUserFolder<Playlist> playlist, BaseItem[] itemsToAdd)
        {
            _playlistManager.AddToPlaylist(playlist.InternalId,
                itemsToAdd.Select(x => x.InternalId).ToArray(), playlist.User);

            playlist.Item.RefreshMetadata(new CancellationToken());
        }

        public void RemoveItems(UserFolder folder, BaseItem[] itemsToRemove)
        {

            if (folder is LibraryUserFolder<Playlist> playlist)
            {
                _playlistManager.RemoveFromPlaylist(playlist.InternalId,
                    itemsToRemove.Select(x => x.ListItemEntryId).ToArray());
                playlist.Item.RefreshMetadata(new CancellationToken());
            }
        }
    }
}