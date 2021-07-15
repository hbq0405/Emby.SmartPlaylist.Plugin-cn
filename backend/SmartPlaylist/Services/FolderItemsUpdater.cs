using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Playlists;
using SmartPlaylist.Domain;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Library;
using System;
using System.Threading;
using System.Collections.Generic;

namespace SmartPlaylist.Services
{
    public interface IFolderItemsUpdater
    {
        Task UpdateAsync(UserFolder folder, BaseItem[] newItems);
    }

    public class CollectionItemUpdater : IFolderItemsUpdater
    {
        private readonly ILibraryManager _libraryManager;
        public CollectionItemUpdater(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public async Task UpdateAsync(UserFolder folder, BaseItem[] newItems)
        {
            if (folder is LibraryUserFolder<Folder> libraryUserCollection)
            {
                foreach (BaseItem item in folder.GetItems())
                    item.RemoveCollection(libraryUserCollection.InternalId);

                foreach (BaseItem b in newItems)
                {
                    b.AddCollectionInfo(new LinkedItemInfo()
                    {
                        Id = libraryUserCollection.InternalId,
                        Name = libraryUserCollection.Name
                    });
                }

                await Task.Run(() =>
                {
                    _libraryManager.UpdateItems(new List<BaseItem>(newItems),
                            libraryUserCollection.Item,
                            ItemUpdateType.MetadataEdit,
                            new CancellationToken(false));
                }).ConfigureAwait(false);
            }
        }
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
                    Name = folder.Name,
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