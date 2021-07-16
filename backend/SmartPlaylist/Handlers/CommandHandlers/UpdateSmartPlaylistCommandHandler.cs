using System;
using System.Collections.Generic;
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

        public UpdateSmartPlaylistCommandHandler(
            IUserItemsProvider userItemsProvider, ISmartPlaylistProvider smartPlaylistProvider,
            IFolderRepository folderRepository, IFolderItemsUpdater playlistItemsUpdater,
            ISmartPlaylistStore smartPlaylistStore, IFolderItemsUpdater collectionItemsUpdater)
        {
            _userItemsProvider = userItemsProvider;
            _smartPlaylistProvider = smartPlaylistProvider;
            _folderRepository = folderRepository;
            _playlistItemsUpdater = playlistItemsUpdater;
            _smartPlaylistStore = smartPlaylistStore;
            _collectionItemsUpdater = collectionItemsUpdater;
        }

        public async Task HandleAsync(UpdateSmartPlaylistCommand message)
        {
            var smartPlaylist = await _smartPlaylistProvider.GetSmartPlaylistAsync(message.SmartPlaylistId)
                .ConfigureAwait(false);

            var playlist = _folderRepository.GetUserPlaylistOrCollectionFolder(smartPlaylist);

            var items = _userItemsProvider.GetItems(playlist.User, Const.SupportedItemTypeNames).ToArray();

            BaseItem[] newItems;
            using (PerfLogger.Create("FilterPlaylistItems",
                () => new { playlistName = playlist.SmartPlaylist.Name, itemsCount = items.Length }))
            {
                newItems = smartPlaylist.FilterPlaylistItems(playlist, items).ToArray();
            }

            long id = await (smartPlaylist.SmartType == Domain.SmartType.Collection ? _collectionItemsUpdater : _playlistItemsUpdater)
               .UpdateAsync(playlist, newItems).ConfigureAwait(false);

            bool idChange = smartPlaylist.InternalId != id;
            if (idChange)
            {
                if (smartPlaylist.InternalId != 0)
                    CleanUp(playlist.User, smartPlaylist);
                smartPlaylist.InternalId = id;
            }

            var smDto = smartPlaylist.ToDto();

            if ((!_smartPlaylistStore.Exists(smDto.UserId, smDto.Id)) || smartPlaylist.IsShuffleUpdateType || idChange)
            {
                if (smartPlaylist.IsShuffleUpdateType)
                    smartPlaylist.UpdateLastShuffleTime();
                _smartPlaylistStore.Save(smDto);
            }
        }

        private void CleanUp(User user, Domain.SmartPlaylist smartPlaylist)
        {
            string[] priors = smartPlaylist.ToDto().PriorNames;

            foreach (string priorName in priors)
            {
                CleanUpPriorName<Folder>(smartPlaylist, priorName, SmartType.Collection, _folderRepository.FindCollection(smartPlaylist, priorName), _collectionItemsUpdater);
                CleanUpPriorName<Playlist>(smartPlaylist, priorName, SmartType.Playlist, _folderRepository.FindPlaylist(smartPlaylist, priorName), _playlistItemsUpdater);
            }
        }

        private void CleanUpPriorName<T>(Domain.SmartPlaylist smartPlaylist, string priorName, SmartType smartType, UserFolder priorFolder, IFolderItemsUpdater folderItemsUpdater) where T : Folder
        {
            bool current = string.Equals(smartPlaylist.Name, priorName, StringComparison.OrdinalIgnoreCase);

            if (!current || (current && smartPlaylist.SmartType != smartType))
                if (priorFolder is LibraryUserFolder<T> libFolder)
                {
                    folderItemsUpdater.RemoveItems(libFolder, libFolder.GetItems());
                    smartPlaylist.RemovePriorName(priorName);

                }
        }
    }
}