using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;
using SmartPlaylist.Contracts;
using SmartPlaylist.Domain;
using SmartPlaylist.Domain.CriteriaDefinition;

namespace SmartPlaylist.Api
{
    [Route("/smartplaylist", "POST", Summary = "")]
    public class SaveSmartPlaylist : SmartPlaylistDto, IReturn<SmartPlaylistDto>
    {
    }

    [Route("/smartplaylist/{Id}", "DELETE", Summary = "")]
    public class DeleteSmartPlaylist : IReturnVoid
    {
        public string Id { get; set; }
    }

    public class GetAppDataResponse
    {
        public SmartPlaylistDto[] Playlists { get; set; }

        public CriteriaDefinition[] RulesCriteriaDefinitions => DefinedCriteriaDefinitions.All;

        public string[] LimitOrdersBy => DefinedLimitOrders.AllNames;
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
            {
                SmartPlaylistInfoDto smartPlaylistInfo = SmartPlaylistInfoDto.FromSmartPlaylist(lastPlaylist);
                return smartPlaylistInfo;
            }
            return "{}";
        }
    }

    [Route("/smartplaylist/info/{Id}", "GET", Summary = "")]
    public class GetPlaylistItems : IReturn<SmartPlaylistInfoDto>
    {
        public string Id { get; set; }
    }
}