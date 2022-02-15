using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;
using SmartPlaylist.Contracts;
using SmartPlaylist.Domain;
using SmartPlaylist.Domain.CriteriaDefinition;
using System.Linq;
using SmartPlaylist.Handlers.Commands;

namespace SmartPlaylist.Api
{
    [Route("/smartplaylist", "POST", Summary = "")]
    public class SaveSmartPlaylist : SmartPlaylistDto, IReturn<SmartPlaylistDto>
    {
    }

    [Route("/smartplaylist/{Id}/{Keep}", "DELETE", Summary = "")]
    public class DeleteSmartPlaylist : IReturnVoid
    {
        public string Id { get; set; }
        public bool Keep { get; set; }
    }

    public class GetAppDataResponse
    {
        public SmartPlaylistDto[] Playlists { get; set; }

        public CriteriaDefinition[] RulesCriteriaDefinitions => DefinedCriteriaDefinitions.All;

        public string[] LimitOrdersBy => DefinedLimitOrders.AllNames;

        public SourceDto[] Sources => SourceDto.All;
    }

    [Route("/smartplaylist/appData", "GET", Summary = "")]
    public class GetAppData : IReturn<GetAppDataResponse>
    {
    }

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
            await Plugin.Instance.SmartPlaylistCommandHandler.HandleAsync(new UpdateSmartPlaylistCommand(Guid.Parse(request.Id)));

            var lastPlaylist = await Plugin.Instance.SmartPlaylistStore.GetSmartPlaylistAsync(Guid.Parse(request.Id));
            if (lastPlaylist != null)
                return getInfo(lastPlaylist);

            return "{}";
        }

    }

    [Route("/smartplaylist/info/{Id}", "GET", Summary = "")]
    public class GetPlaylistItems : IReturn<SmartPlaylistInfoDto>
    {
        public string Id { get; set; }
    }

    [Route("/smartplaylist/info/{Id}", "POST", Summary = "")]
    public class ExecutePlaylist : IReturn<SmartPlaylistInfoDto>
    {
        public string Id { get; set; }
    }
}