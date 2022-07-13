using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;
using SmartPlaylist.Services.SmartPlaylist;

namespace SmartPlaylist.Api
{
    public class IOService : IService, IRequiresRequest
    {
        private readonly ISmartPlaylistStore _smartPlaylistStore;
        private readonly ISessionContext _sessionContext;
        public IRequest Request { get; set; }

        public IOService(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
            _smartPlaylistStore = Plugin.Instance.SmartPlaylistStore;
        }

        public async Task Get(ExportPlaylists request)
        {
            string tempFile = string.Empty;
            try
            {
                byte[] decodedBytes = Convert.FromBase64String(request.payload);
                string[] ids = Encoding.UTF8.GetString(decodedBytes).Split(',');
                tempFile = _smartPlaylistStore.Export(ids);

                Request.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(tempFile) + "\"");
                await Request.Response.TransmitFile(tempFile, 0, 0, MediaBrowser.Model.IO.FileShareMode.Read, System.Threading.CancellationToken.None);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFile))
                    _smartPlaylistStore.Delete(tempFile);
            }
        }

        private User GetUser()
        {
            return _sessionContext.GetUser(Request);
        }
    }
}