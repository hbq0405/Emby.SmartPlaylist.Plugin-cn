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
    public class SaveSmartPlaylist : SmartPlaylistDto, IReturn<SmartPlaylistResponseDto>
    { }

    [Route("/smartplaylist/sort", "POST", Summary = "")]
    public class SaveSortJobPlaylist : SmartPlaylistDto, IReturn<SmartPlaylistResponseDto>
    { }

    [Route("/smartplaylist/log/{Id}", "GET", Summary = "")]
    public class LogFileRequest : IReturn
    {
        public string Id { get; set; }

    }

    [Route("/smartplaylist/{Id}/{Keep}", "DELETE", Summary = "")]
    public class DeleteSmartPlaylist : IReturnVoid
    {
        public string Id { get; set; }
        public bool Keep { get; set; }
    }

    [Route("/smartplaylist/appData", "GET", Summary = "")]
    public class GetAppData : IReturn<AppDataResponse>
    { }

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