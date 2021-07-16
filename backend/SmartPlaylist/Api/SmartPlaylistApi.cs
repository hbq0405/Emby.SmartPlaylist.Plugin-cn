using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SmartPlaylist.Extensions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;
using SmartPlaylist.Handlers.Commands;
using SmartPlaylist.Infrastructure.MesssageBus;
using SmartPlaylist.Services.SmartPlaylist;

namespace SmartPlaylist.Api
{
    public class SmartPlaylistApi : IService, IRequiresRequest
    {
        private readonly MessageBus _messageBus;
        private readonly ISessionContext _sessionContext;
        private readonly ISmartPlaylistStore _smartPlaylistStore;
        private readonly SmartPlaylistValidator _smartPlaylistValidator;


        public SmartPlaylistApi(ISessionContext sessionContext
        )
        {
            _sessionContext = sessionContext;

            _messageBus = Plugin.Instance.MessageBus;
            _smartPlaylistStore = Plugin.Instance.SmartPlaylistStore;
            _smartPlaylistValidator = Plugin.Instance.SmartPlaylistValidator;
        }

        public IRequest Request { get; set; }

        public void Post(SaveSmartPlaylist request)
        {
            var playlist = request;
            var user = GetUser();

            playlist.UserId = user.Id;
            playlist.LastShuffleUpdate = DateTimeOffset.UtcNow.Date;
            playlist.PriorNames = GetPriorNames(playlist);

            _smartPlaylistValidator.Validate(playlist);

            if (playlist.InternalId != 0)
            {
                Task<Contracts.SmartPlaylistDto> lastPlayList = _smartPlaylistStore.GetSmartPlaylistAsync(Guid.Parse(playlist.Id));
                if (lastPlayList.Result != null)
                    playlist.ForceCreate = !string.Equals(lastPlayList.Result.SmartType, playlist.SmartType, StringComparison.OrdinalIgnoreCase);
            }
            else
                playlist.ForceCreate = true;

            _smartPlaylistStore.Save(playlist);

            _messageBus.Publish(new UpdateSmartPlaylistCommand(Guid.Parse(playlist.Id)));
        }

        public void Delete(DeleteSmartPlaylist request)
        {
            var user = GetUser();
            _smartPlaylistStore.Delete(user.Id, request.Id);
        }

        public async Task<object> Get(GetAppData request)
        {
            var user = GetUser();
            var playlists = await _smartPlaylistStore.LoadPlaylistsAsync(user.Id).ConfigureAwait(false);

            return new GetAppDataResponse
            {
                Playlists = playlists
            };
        }

        private User GetUser()
        {
            return _sessionContext.GetUser(Request);
        }

        public static string[] GetPriorNames(SaveSmartPlaylist playlist)
        {
            StringCollection holder = new StringCollection();
            if (playlist.PriorNames != null)
                holder.AddRange(playlist.PriorNames);

            if (!holder.Contains(playlist.Name))
                holder.Add(playlist.Name);

            string[] result = new string[holder.Count];
            holder.CopyTo(result, 0);

            return result;
        }
    }
}