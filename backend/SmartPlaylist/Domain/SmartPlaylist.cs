using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using SmartPlaylist.Adapters;
using SmartPlaylist.Comparers;
using SmartPlaylist.Contracts;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Extensions;
namespace SmartPlaylist.Domain
{
    public class SmartPlaylist
    {
        private readonly SmartPlaylistDto _dto;
        private long _internalid = 0;

        public SmartPlaylist(SmartPlaylistDto dto)
        {
            _dto = dto;
            Id = Guid.Parse(dto.Id);
            Name = dto.Name;
            UserId = dto.UserId;
            Rules = new RuleBase[] { RuleAdapter.Adapt(dto.RulesTree) };
            Limit = new SmartPlaylistLimit(dto.Limit);
            LastShuffleUpdate = dto.LastShuffleUpdate;
            UpdateType = UpdateType.ConvertFromString<UpdateType>(dto.UpdateType, UpdateType.Live);
            SmartType = SmartType.ConvertFromString<SmartType>(dto.SmartType, SmartType.Playlist);
            InternalId = dto.InternalId;
            MediaType = MediaTypeGetter.Get(Rules);
            ForceCreate = dto.ForceCreate;
            OriginalSmartType = SmartType.ConvertFromString<SmartType>(dto.OriginalSmartType, SmartType.Playlist);
            CollectionMode = CollectionMode.ConvertFromString<CollectionMode>(dto.CollectionMode, CollectionMode.Item);
            LastUpdated = dto.LastUpdated;
            LastSync = dto.LastSync;
            SyncCount = dto.SyncCount;
        }

        public Guid Id { get; }
        public string Name { get; }
        public Guid UserId { get; }
        public RuleBase[] Rules { get; }
        public SmartPlaylistLimit Limit { get; }
        public UpdateType UpdateType { get; }
        public SmartType SmartType { get; }
        public SmartType OriginalSmartType { get; }
        public CollectionMode CollectionMode { get; }
        public long InternalId
        {
            get { return _internalid; }
            set
            {
                _internalid = value;
                _dto.InternalId = _internalid;
            }
        }
        public bool ForceCreate { get; }
        public DateTimeOffset? LastShuffleUpdate { get; private set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? LastSync { get; set; }
        public int SyncCount { get; set; } = 0;

        public bool CanUpdatePlaylist => CheckIfCanUpdatePlaylist();

        public bool IsShuffleUpdateType => UpdateType == UpdateType.ShuffleDaily ||
                                           UpdateType == UpdateType.ShuffleMonthly ||
                                           UpdateType == UpdateType.ShuffleWeekly;

        public bool CanUpdatePlaylistWithNewItems => (IsRandomSort || !Limit.HasLimit) && !IsShuffleUpdateType;
        public bool IsRandomSort => Limit.OrderBy is RandomLimitOrder;
        public string MediaType { get; }

        private bool CheckIfCanUpdatePlaylist()
        {
            if (UpdateType == UpdateType.Manual) return false;

            if (LastShuffleUpdate.HasValue && IsShuffleUpdateType)
            {
                var now = DateTimeOffset.UtcNow;
                switch (UpdateType)
                {
                    case UpdateType.ShuffleDaily:
                        return now >= LastShuffleUpdate.Value.AddDays(1);
                    case UpdateType.ShuffleWeekly:
                        return now >= LastShuffleUpdate.Value.AddDays(7);
                    case UpdateType.ShuffleMonthly:
                        return now >= LastShuffleUpdate.Value.AddMonths(1);
                }
            }

            return true;
        }

        public IEnumerable<BaseItem> FilterPlaylistItems(UserFolder userPlaylist, IEnumerable<BaseItem> items)
        {
            var playlistItems = userPlaylist.GetItems();
            var newItems = FilterItems(playlistItems, items, userPlaylist.User);
            newItems = RemoveMissingEpisodes(newItems);
            if (SmartType == SmartType.Collection && CollectionMode != CollectionMode.Item)
                newItems = RollUpTo(newItems);

            return (IsShuffleUpdateType ? newItems.Shuffle() : OrderItems(newItems)).Take(Limit.MaxItems);
        }

        private static IEnumerable<BaseItem> RemoveMissingEpisodes(IEnumerable<BaseItem> items)
        {
            return items.Where(x => !(x is Episode episode && episode.IsMissingEpisode));
        }

        private IEnumerable<BaseItem> RollUpTo(IEnumerable<BaseItem> items)
        {
            EpimodeAttribute rollTo = CollectionMode.GetAttributeOfType<EpimodeAttribute>();
            return items.Select(x => (x is Episode) ? RollUpToItemType(x, rollTo.MediaType) : x).Distinct();
        }

        private BaseItem RollUpToItemType(BaseItem item, Type rollType)
        {
            return item.GetType().IsAssignableFrom(rollType) ? item : RollUpToItemType(item.Parent, rollType);
        }

        private IEnumerable<BaseItem> OrderItems(IEnumerable<BaseItem> playlistItems)
        {
            return Limit.OrderBy.Order(playlistItems);
        }

        private IEnumerable<BaseItem> FilterItems(IEnumerable<BaseItem> playlistItems, IEnumerable<BaseItem> newItems,
            User user)
        {
            return playlistItems.Union(newItems, new BaseItemEqualByInternalId())
                .Where(x => IsMatchRules(x, user));
        }

        private bool IsMatchRules(BaseItem item, User user)
        {
            return Rules.All(x => x.IsMatch(new UserItem(user, item)));
        }

        public void UpdateLastShuffleTime()
        {
            var lastShuffleUpdate = LastShuffleUpdate.GetValueOrDefault(DateTimeOffset.UtcNow.Date);

            switch (UpdateType)
            {
                case UpdateType.ShuffleDaily:
                    LastShuffleUpdate = lastShuffleUpdate.AddDays(1);
                    break;
                case UpdateType.ShuffleWeekly:
                    LastShuffleUpdate = lastShuffleUpdate.AddDays(7);
                    break;
                case UpdateType.ShuffleMonthly:
                    LastShuffleUpdate = lastShuffleUpdate.AddMonths(1);
                    break;
            }
        }
        public SmartPlaylistDto ToDto()
        {
            return new SmartPlaylistDto
            {
                Id = Id.ToString(),
                LastShuffleUpdate = LastShuffleUpdate,
                Limit = _dto.Limit,
                Name = _dto.Name,
                RulesTree = _dto.RulesTree,
                UpdateType = _dto.UpdateType,
                SmartType = _dto.SmartType,
                UserId = _dto.UserId,
                OriginalSmartType = _dto.OriginalSmartType,
                InternalId = _dto.InternalId,
                CollectionMode = _dto.CollectionMode,
                LastUpdated = LastUpdated,
                LastSync = LastSync,
                SyncCount = SyncCount
            };
        }
    }
}