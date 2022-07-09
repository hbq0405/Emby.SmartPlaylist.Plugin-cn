using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using SmartPlaylist.Domain;
using SmartPlaylist.Handlers.Commands;
using SmartPlaylist.Infrastructure;
using SmartPlaylist.Infrastructure.MesssageBus;
using SmartPlaylist.Services;
using SmartPlaylist.Services.SmartPlaylist;

namespace SmartPlaylist.Handlers.CommandHandlers
{
    public class UpdateSmartPlaylistCommandHandler : IMessageHandlerAsync<UpdateSmartPlaylistCommand>
    {
        private readonly IFolderItemsUpdater _playlistItemsUpdater;
        private readonly IFolderItemsUpdater _collectionItemsUpdater;
        private readonly IFolderRepository _folderRepository;
        private readonly ISmartPlaylistProvider _smartPlaylistProvider;
        private readonly ISmartPlaylistStore _smartPlaylistStore;

        private readonly IUserItemsProvider _userItemsProvider;
        private readonly ILibraryManager _libraryManager;

        public UpdateSmartPlaylistCommandHandler(
            IUserItemsProvider userItemsProvider, ISmartPlaylistProvider smartPlaylistProvider,
            IFolderRepository folderRepository, IFolderItemsUpdater playlistItemsUpdater,
            ISmartPlaylistStore smartPlaylistStore, IFolderItemsUpdater collectionItemsUpdater,
            ILibraryManager libraryManager)
        {
            _userItemsProvider = userItemsProvider;
            _smartPlaylistProvider = smartPlaylistProvider;
            _folderRepository = folderRepository;
            _playlistItemsUpdater = playlistItemsUpdater;
            _smartPlaylistStore = smartPlaylistStore;
            _collectionItemsUpdater = collectionItemsUpdater;
            _libraryManager = libraryManager;
        }

        public async Task HandleAsync(UpdateSmartPlaylistCommand message)
        {
            //            Stopwatch sw = new Stopwatch();
            //            sw.Start();
            //var smartPlaylist = await _smartPlaylistProvider.GetSmartPlaylistAsync(message.SmartPlaylistId)
            //    .ConfigureAwait(false);

            SmartPlaylistUpdater updater = new SmartPlaylistUpdater(_folderRepository, _playlistItemsUpdater, _collectionItemsUpdater, _smartPlaylistStore, _userItemsProvider);
            await updater.Update(await _smartPlaylistProvider.GetSmartPlaylistAsync(message.SmartPlaylistId).ConfigureAwait(false));
            /*

                        try
                        {
                            smartPlaylist.LastSyncDuration = 0;
                            smartPlaylist.Status = smartPlaylist.Enabled ? "Complete" : "Disabled";
                            if (!smartPlaylist.Enabled)
                                return;

                            (UserFolder user, BaseItem[] baseItems) folder = _folderRepository.GetBaseItemsForSmartPlayList(smartPlaylist, _userItemsProvider);

                            BaseItem[] newItems;
                            using (PerfLogger.Create("FilterPlaylistItems",
                                () => new { playlistName = folder.user.SmartPlaylist.Name, itemsCount = folder.baseItems.Length }))
                            {
                                newItems = smartPlaylist.FilterPlaylistItems(folder.user, folder.baseItems).ToArray();
                            }

                            var update = await (smartPlaylist.SmartType == Domain.SmartType.Collection ? _collectionItemsUpdater : _playlistItemsUpdater)
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
                        }
                        */
        }


    }
}