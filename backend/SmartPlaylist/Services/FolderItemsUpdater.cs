using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Playlists;
using SmartPlaylist.Domain;

namespace SmartPlaylist.Services
{
    public interface IFolderItemsUpdater
    {
        Task UpdateAsync(UserFolder playlist, BaseItem[] newItems);
    }

    public class FolderItemsUpdater : IFolderItemsUpdater
    {
        private readonly IPlaylistManager _playlistManager;
        public FolderItemsUpdater(IPlaylistManager playlistManager)
        {
            _playlistManager = playlistManager;
        }

        public async Task UpdateAsync(UserFolder folder, BaseItem[] newItems)
        {
            if (folder.SmartType == SmartType.Playlist)
                await UpdatePlaylist(folder, newItems);
            else if (folder.SmartType == SmartType.Collection)
                await UpdateCollection(folder, newItems);
            else
                throw new System.Exception($"Invalid SmartType: {folder.SmartType}");
        }

        private async Task UpdateCollection(UserFolder folder, BaseItem[] newItems)
        {

            foreach (var item in folder.GetItems())
                Debug.Write(item);

            if (folder is LibraryUserFolder<Folder> libraryUserCollection)
            {
                Folder f = libraryUserCollection.Item;
                System.Console.WriteLine(f.ToString());
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

        private async Task UpdatePlaylist(UserFolder folder, BaseItem[] newItems)
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