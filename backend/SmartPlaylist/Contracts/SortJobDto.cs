using System;

namespace SmartPlaylist.Contracts
{
    public class SortJobDto
    {
        public bool Enabled { get; set; }
        public string UpdateType { get; set; }
        public string OrderBy { get; set; }
        public int SyncCount { get; set; }
        public long LastSyncDuration { get; set; } = 0;
        public string Status { get; set; }
        public DateTimeOffset? NextUpdate { get; set; } = null;
        public DateTime? LastUpdated { get; set; } = null;
        public DateTime? LastRan { get; set; } = null;
    }
}