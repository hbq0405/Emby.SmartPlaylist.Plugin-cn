using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;
using SmartPlaylist.Handlers.Commands;
using SmartPlaylist.Infrastructure.MesssageBus;
using SmartPlaylist.Services.SmartPlaylist;
using SmartPlaylist.Services;
using SmartPlaylist.Adapters;

namespace SmartPlaylist.Api
{
    public class SmartPlaylistService : IService, IRequiresRequest
    {
        private readonly MessageBus _messageBus;
        private readonly ISessionContext _sessionContext;
        private readonly ISmartPlaylistStore _smartPlaylistStore;
        private readonly SmartPlaylistValidator _smartPlaylistValidator;
        private readonly IFolderRepository _folderRepository;

        public SmartPlaylistService(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;

            _messageBus = Plugin.Instance.MessageBus;
            _smartPlaylistStore = Plugin.Instance.SmartPlaylistStore;
            _smartPlaylistValidator = Plugin.Instance.SmartPlaylistValidator;
            _folderRepository = Plugin.Instance.FolderRepository;
        }

        public IRequest Request { get; set; }

        public Contracts.SmartPlaylistResponseDto Post(SaveSortJobPlaylist request)
        {
            try
            {
                var playlist = request;
                _smartPlaylistValidator.Validate(playlist);

                Domain.SmartPlaylist smartPlaylist = new Domain.SmartPlaylist(playlist);

                smartPlaylist.SortJob.LastUpdated = DateTime.Now;

                if (smartPlaylist.SortJob.Enabled)
                {
                    if (smartPlaylist.SortJob.NextUpdate == null)
                        smartPlaylist.SortJob.UpdateNextUpdate();
                }

                Contracts.SmartPlaylistDto dto = smartPlaylist.ToDto();
                _smartPlaylistStore.Save(dto);

                return new Contracts.SmartPlaylistResponseDto()
                {
                    Success = true,
                    Playlist = dto
                };
            }
            catch (Exception ex)
            {
                Plugin.Instance.Logger.Error($"Error saving smart playlist: {ex.Message}", request);
                return new Contracts.SmartPlaylistResponseDto()
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
        public Contracts.SmartPlaylistResponseDto Post(SaveSmartPlaylist request)
        {
            try
            {
                var playlist = request;
                var user = GetUser();

                playlist.UserId = user.Id;
                playlist.LastShuffleUpdate = DateTimeOffset.UtcNow.Date;

                _smartPlaylistValidator.Validate(playlist);

                var persistedPlaylist = GetPlaylistFromStore(Guid.Parse(playlist.Id));
                if (persistedPlaylist != null)
                {
                    playlist.InternalId = persistedPlaylist.InternalId;
                    playlist.ForceCreate = !string.Equals(persistedPlaylist.SmartType, playlist.SmartType, StringComparison.OrdinalIgnoreCase);
                    playlist.OriginalSmartType = persistedPlaylist.SmartType;
                }

                playlist.LastUpdated = DateTime.Now;
                _smartPlaylistStore.Save(playlist);

                _messageBus.Publish(new UpdateSmartPlaylistCommand(Guid.Parse(playlist.Id), ExecutionModes.OnSave));

                return new Contracts.SmartPlaylistResponseDto()
                {
                    Success = true,
                    Playlist = playlist
                };
            }
            catch (Exception ex)
            {
                Plugin.Instance.Logger.Error($"Error saving smart playlist: {ex.Message}", request);
                return new Contracts.SmartPlaylistResponseDto()
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private Contracts.SmartPlaylistDto GetPlaylistFromStore(Guid playlistId)
        {
            var persistedPlaylist = _smartPlaylistStore.GetSmartPlaylistAsync(playlistId);
            return persistedPlaylist != null ? ((persistedPlaylist.IsFaulted) ? null : persistedPlaylist.Result) : null;
        }

        public void Delete(DeleteSmartPlaylist request)
        {
            try
            {
                var user = GetUser();
                var playlist = GetPlaylistFromStore(Guid.Parse(request.Id));
                if (playlist != null && !request.Keep)
                    _folderRepository.Remove(SmartPlaylistAdapter.Adapt(playlist));
                _smartPlaylistStore.Delete(user.Id, request.Id);
            }
            catch (Exception ex)
            {
                Plugin.Instance.Logger.Error($"Error deleting smart playlist: {ex.Message}", request);
            }
        }

        public async Task<object> Get(GetAppData request)
        {
            var user = GetUser();
            var playlists = await _smartPlaylistStore.LoadPlaylistsAsync(user.Id).ConfigureAwait(false);

            return new AppDataResponse
            {
                Playlists = playlists
            };
        }

        private User GetUser()
        {
            return _sessionContext.GetUser(Request);
        }
    }
}