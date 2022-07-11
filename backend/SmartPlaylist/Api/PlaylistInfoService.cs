using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;
using SmartPlaylist.Contracts;
using SmartPlaylist.Handlers.Commands;
using System.Linq;
using System.Threading;
namespace SmartPlaylist.Api
{
    public class PlaylistInfoService : IService
    {
        public async Task<object> Get(GetPlaylistItems request)
        {
            var persistedPlaylist = await Plugin.Instance.SmartPlaylistStore.GetSmartPlaylistAsync(Guid.Parse(request.Id));
            if (persistedPlaylist != null)
                return getInfo(persistedPlaylist);

            return "{}";
        }

        private SmartPlaylistInfoDto getInfo(SmartPlaylistDto dto)
        {
            SmartPlaylistInfoDto smartPlaylistInfo = SmartPlaylistInfoDto.FromSmartPlaylist(dto);
            var folder = Plugin.Instance.FolderRepository.GetUserPlaylistOrCollectionFolder(new Domain.SmartPlaylist(dto));
            smartPlaylistInfo.Items = folder.GetItems().Select(x => x.Name).ToArray();
            return smartPlaylistInfo;
        }

        public async Task<object> Post(ExecutePlaylist request)
        {
            new Thread(new ThreadStart(async () =>
            {
                await Plugin.Instance.SmartPlaylistCommandHandler.HandleAsync(new UpdateSmartPlaylistCommand(Guid.Parse(request.Id), ExecutionModes.Manual));
            })).Start();

            var smartPlaylist = await Plugin.Instance.SmartPlaylistStore.GetSmartPlaylistAsync(Guid.Parse(request.Id));
            if (smartPlaylist != null)
            {

                return getInfo(smartPlaylist);
            }

            return "{}";
        }

    }

}