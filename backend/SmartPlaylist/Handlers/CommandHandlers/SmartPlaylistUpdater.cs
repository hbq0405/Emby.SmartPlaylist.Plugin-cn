using System.Diagnostics;
using MediaBrowser.Controller.Entities;
using SmartPlaylist.Domain;
using SmartPlaylist.Services;
using System.Linq;
using SmartPlaylist.Infrastructure;
using System.Threading.Tasks;
using System;
using SmartPlaylist;
using SmartPlaylist.Services.SmartPlaylist;


namespace SmartPlaylist.Handlers.CommandHandlers
{
    public class SmartPlaylistUpdater
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFolderItemsUpdater _playlistItemsUpdater;
        private readonly IFolderItemsUpdater _collectionItemsUpdater;
        private readonly ISmartPlaylistStore _smartPlaylistStore;
        private readonly IUserItemsProvider _userItemsProvider;
        public SmartPlaylistUpdater(IFolderRepository folderRepository, IFolderItemsUpdater playlistItemsUpdater, IFolderItemsUpdater collectionItemsUpdater, ISmartPlaylistStore smartPlaylistStore) :
        this(folderRepository, playlistItemsUpdater, collectionItemsUpdater, smartPlaylistStore, null)
        { }

        public SmartPlaylistUpdater(IFolderRepository folderRepository, IFolderItemsUpdater playlistItemsUpdater, IFolderItemsUpdater collectionItemsUpdater, ISmartPlaylistStore smartPlaylistStore, IUserItemsProvider userItemsProvider)
        {
            _folderRepository = folderRepository;
            _playlistItemsUpdater = playlistItemsUpdater;
            _collectionItemsUpdater = collectionItemsUpdater;
            _smartPlaylistStore = smartPlaylistStore;
            _userItemsProvider = userItemsProvider;
        }

        public async Task Update(SmartPlaylist.Domain.SmartPlaylist smartPlaylist)
        {
            await Update(smartPlaylist, null);
        }
        public async Task Update(SmartPlaylist.Domain.SmartPlaylist smartPlaylist, BaseItem[] items)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            smartPlaylist.LastSyncDuration = 0;
            smartPlaylist.Status = smartPlaylist.Enabled ? "Complete" : "Disabled";
            if (!smartPlaylist.Enabled)
                return;
            try
            {
                (UserFolder user, BaseItem[] items) folder = _folderRepository.GetBaseItemsForSmartPlayList(smartPlaylist, _userItemsProvider);
                BaseItem[] processItems = items == null ? folder.items : folder.items.Union(items).ToArray();
                smartPlaylist.Log($"Dealing with {processItems.Length} media items from source");
                BaseItem[] newItems;
                using (PerfLogger.Create("FilterPlaylistItems",
                    () => new { playlistName = folder.user.SmartPlaylist.Name, itemsCount = processItems.Length }))
                {
                    newItems = smartPlaylist.FilterPlaylistItems(folder.user, processItems).ToArray();
                }

                var update = await (smartPlaylist.SmartType == SmartPlaylist.Domain.SmartType.Collection ? _collectionItemsUpdater : _playlistItemsUpdater)
                    .UpdateAsync(folder.user, newItems).ConfigureAwait(false);

                smartPlaylist.Status = update.message;

                if (smartPlaylist.InternalId != update.internalId)
                {
                    if (smartPlaylist.InternalId > 0)
                        _folderRepository.Remove(smartPlaylist);

                    smartPlaylist.InternalId = update.internalId;
                }
                smartPlaylist.LastSync = DateTime.Now;
                smartPlaylist.SyncCount++;

                if (smartPlaylist.IsShuffleUpdateType || smartPlaylist.IsScheduledType)
                    smartPlaylist.UpdateLastShuffleTime();
            }
            catch (Exception ex)
            {
                smartPlaylist.Status = $"Error {ex.Message}";
                Plugin.Instance.Logger.Error($"Error executing smart playlist: {ex.Message}", smartPlaylist);
                throw ex;
            }
            finally
            {
                sw.Stop();
                smartPlaylist.LastSyncDuration = sw.ElapsedMilliseconds;
                _smartPlaylistStore.Save(smartPlaylist.ToDto());
                await _smartPlaylistStore.WriteToLogAsync(smartPlaylist);
            }
        }
    }
}