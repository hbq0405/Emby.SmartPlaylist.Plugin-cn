using System;

namespace SmartPlaylist.Contracts
{
    [Serializable]
    public class SmartPlaylistNewItemOrderDto
    {
        public bool HasSort { get; set; }
        public string OrderBy { get; set; }
    }
}