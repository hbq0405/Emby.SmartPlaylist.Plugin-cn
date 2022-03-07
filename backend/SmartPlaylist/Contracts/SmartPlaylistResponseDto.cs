using System;

namespace SmartPlaylist.Contracts
{
    [Serializable]
    public class SmartPlaylistResponseDto
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public SmartPlaylistDto Playlist { get; set; }
    }
}