namespace SmartPlaylist.Domain
{
    public enum UpdateType
    {
        Live = 1,
        Manual = 2,
        ShuffleDaily = 3,
        ShuffleWeekly = 4,
        ShuffleMonthly = 5
    }

    public enum SmartType
    {
        Playlist = 1,
        Collection = 2
    }

    public enum CollectionMode
    {
        Item = 1,
        Season = 2,
        Series = 3
    }
}