using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Playlists;
using SmartPlaylist.Domain;
using System.Collections.Generic;
using SmartPlaylist.Extensions;
using System;

namespace SmartPlaylist.Services
{
    public interface IFolderItemsUpdater
    {
        Task<(long internalId, string message)> UpdateAsync(UserFolder folder, BaseItem[] newItems);
        Task<int> RemoveItems(UserFolder folder, BaseItem[] currentItems, BaseItem[] newItems);

        Task<int> ClearPlaylist(UserFolder folder);

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
            if (folder.SmartPlaylist.IsShuffleUpdateType || folder.SmartPlaylist.Limit.HasLimit)
                await ClearPlaylist(folder);

            var currentItems = folder.GetItems();

            if (folder is LibraryUserFolder<Playlist> libraryUserPlaylist)
            {
                int removed = await RemoveItems(libraryUserPlaylist, currentItems, newItems);
                int added = AddToPlaylist(libraryUserPlaylist, currentItems, newItems);
                libraryUserPlaylist.DynamicUpdate();
                ret = (libraryUserPlaylist.InternalId, $"Completed - (Removed: {removed} Added: {added} items to the existing playlist)");
            }
            else if (newItems.Any())
            {
                PlaylistCreationResult request = await _playlistManager.CreatePlaylist(new PlaylistCreationRequest
                {

                    ItemIdList = newItems.Select(x => x.InternalId).ToArray(),
                    Name = folder.SmartPlaylist.Name,
                    User = folder.User,

                }).ConfigureAwait(false);

                ret = (long.Parse(request.Id), $"Completed - (Added {newItems.Count()} to new playlist)");
            }
            else
                ret = (-1, "Completed - (Playlist not created, no items found to add)");

            return ret;
        }

        private int AddToPlaylist(LibraryUserFolder<Playlist> playlist, BaseItem[] currentItems, BaseItem[] newItems)
        {
            List<BaseItem> toAdd = new List<BaseItem>(newItems.Except(currentItems, (n, c) => n.InternalId == c.InternalId));
            if (toAdd.Any())
                foreach (var chunk in toAdd.Partition(100))
                {
                    _playlistManager.AddToPlaylist(playlist.InternalId,
                        chunk.Select(x => x.InternalId).ToArray(), playlist.User);
                }
            return toAdd.Count;
        }

        public async Task<int> RemoveItems(UserFolder folder, BaseItem[] currentItems, BaseItem[] newItems)
        {
            List<BaseItem> toRemove = new List<BaseItem>(currentItems.Except(newItems, (c, n) => c.InternalId == n.InternalId));
            if (toRemove.Any() && folder is LibraryUserFolder<Playlist> playlist)
            {

                await _playlistManager.RemoveFromPlaylist(playlist.InternalId,
                  toRemove.Select(x => x.ListItemEntryId).ToArray()).ConfigureAwait(false);

            }

            return toRemove.Count;
        }

        public void DynamicUpdate()
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> ClearPlaylist(UserFolder folder)
        {
            if (folder is LibraryUserFolder<Playlist> playlist)
            {
                BaseItem[] items = playlist.Item.GetChildren(folder.User);
                await _playlistManager.RemoveFromPlaylist(playlist.InternalId, items.Select(c => c.ListItemEntryId).ToArray());
                return items.Length;
            }
            return 0;
        }
    }
}