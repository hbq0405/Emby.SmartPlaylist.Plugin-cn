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
        private string _name = string.Empty;
        public SmartPlaylist(SmartPlaylistDto dto)
        {
            _dto = dto;
            Id = Guid.Parse(dto.Id);
            Name = dto.Name;
            UserId = dto.UserId;
            Rules = new RuleBase[] { RuleAdapter.Adapt(dto.RulesTree) };
            Limit = new SmartPlaylistLimit(dto.Limit);
            NewItemOrder = new NewItemOrder(dto.NewItemOrder);
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
            LastSyncDuration = dto.LastSyncDuration;
            Status = dto.Status;
            Enabled = dto.Enabled;
            SourceType = dto.SourceType;
            if (dto.Source != null)
                Source = new Source(dto.Source);
            SortJob = new SortJob(dto.SortJob);
        }

        public Guid Id { get; }
        public string Name { get { return _name; } set { _name = value; } }
        public Guid UserId { get; }
        public RuleBase[] Rules { get; }
        public SmartPlaylistLimit Limit { get; }
        public NewItemOrder NewItemOrder { get; }
        public UpdateType UpdateType { get; }
        public SmartType SmartType { get; }
        public SmartType OriginalSmartType { get; }
        public CollectionMode CollectionMode { get; }
        public bool Enabled { get; }
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
        public long LastSyncDuration { get; set; } = 0;
        public string Status { get; set; }
        public bool CanUpdatePlaylist => CheckIfCanUpdatePlaylist();

        public bool IsScheduledType => UpdateType == UpdateType.Daily ||
                                        UpdateType == UpdateType.Weekly ||
                                        UpdateType == UpdateType.Monthly;
        public bool IsShuffleUpdateType => UpdateType == UpdateType.ShuffleDaily ||
                                           UpdateType == UpdateType.ShuffleMonthly ||
                                           UpdateType == UpdateType.ShuffleWeekly;

        public bool CanUpdatePlaylistWithNewItems => (IsRandomSort || !Limit.HasLimit) && !IsShuffleUpdateType && !IsScheduledType;
        public bool IsRandomSort => Limit.OrderBy is OrderRandom;
        public string MediaType { get; }
        public String SourceType { get; }
        public Source Source { get; }

        public SortJob SortJob { get; }

        private bool CheckIfCanUpdatePlaylist()
        {
            if (UpdateType == UpdateType.Manual) return false;

            if (LastShuffleUpdate.HasValue && (IsShuffleUpdateType || IsScheduledType))
                return DateTimeOffset.UtcNow > LastShuffleUpdate.Value;

            return true;
        }

        public IEnumerable<BaseItem> FilterPlaylistItems(UserFolder userPlaylist, IEnumerable<BaseItem> items)
        {
            var playlistItems = SourceType.Equals("Media Items", StringComparison.OrdinalIgnoreCase) ? userPlaylist.GetItems() : new BaseItem[] { };
            var newItems = FilterItems(playlistItems, items, userPlaylist.User);
            newItems = RemoveMissingEpisodes(newItems);
            if (SmartType == SmartType.Collection && CollectionMode != CollectionMode.Item)
                newItems = RollUpTo(newItems);

            if (IsShuffleUpdateType)
                newItems = newItems.Shuffle();

            if (NewItemOrder.HasSort)
                newItems = OrderNewItems(newItems);
            else if (Limit.HasLimit)
                newItems = OrderLimitItems(newItems).Take(Limit.MaxItems);

            return newItems;
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

        private IEnumerable<BaseItem> OrderLimitItems(IEnumerable<BaseItem> playlistItems)
        {
            return Limit.OrderBy.Order(playlistItems);
        }

        private IEnumerable<BaseItem> OrderNewItems(IEnumerable<BaseItem> playlistItems)
        {
            return NewItemOrder.OrderBy.Order(playlistItems);
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
            var now = DateTime.UtcNow.Date;

            switch (UpdateType)
            {
                case UpdateType.ShuffleDaily:
                case UpdateType.Daily:
                    LastShuffleUpdate = now.AddDays(1);
                    break;
                case UpdateType.ShuffleWeekly:
                case UpdateType.Weekly:
                    LastShuffleUpdate = now.AddDays(7);
                    break;
                case UpdateType.ShuffleMonthly:
                case UpdateType.Monthly:
                    LastShuffleUpdate = now.AddMonths(1);
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
                NewItemOrder = _dto.NewItemOrder,
                Name = Name,
                RulesTree = _dto.RulesTree,
                UpdateType = _dto.UpdateType,
                SmartType = _dto.SmartType,
                UserId = UserId,
                OriginalSmartType = _dto.OriginalSmartType,
                InternalId = InternalId,
                CollectionMode = _dto.CollectionMode,
                LastUpdated = LastUpdated,
                LastSync = LastSync,
                SyncCount = SyncCount,
                LastSyncDuration = LastSyncDuration,
                Status = Status,
                Enabled = Enabled,
                SourceType = SourceType,
                Source = _dto.Source,
                SortJob = _dto.SortJob
            };
        }
    }
}