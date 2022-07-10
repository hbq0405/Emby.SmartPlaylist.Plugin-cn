using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;
using SmartPlaylist.Contracts;
using SmartPlaylist.Handlers.Commands;
using System.Linq;

namespace SmartPlaylist.Api
{
    public class PlaylistInfoService : IService
    {
        public async Task<object> Get(GetPlaylistItems request)
        {
            var lastPlaylist = await Plugin.Instance.SmartPlaylistStore.GetSmartPlaylistAsync(Guid.Parse(request.Id));
            if (lastPlaylist != null)
                return getInfo(lastPlaylist);

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
            await Plugin.Instance.SmartPlaylistCommandHandler.HandleAsync(new UpdateSmartPlaylistCommand(Guid.Parse(request.Id), ExecutionModes.Manual));

            var smartPlaylist = await Plugin.Instance.SmartPlaylistStore.GetSmartPlaylistAsync(Guid.Parse(request.Id));
            if (smartPlaylist != null)
            {

                return getInfo(smartPlaylist);
            }

            return "{}";
        }

    }

}