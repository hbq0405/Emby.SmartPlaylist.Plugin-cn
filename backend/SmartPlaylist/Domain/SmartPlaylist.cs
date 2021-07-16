using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using SmartPlaylist.Comparers;
using SmartPlaylist.Contracts;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Extensions;
using SmartPlaylist.Infrastructure;

namespace SmartPlaylist.Domain
{
    public class SmartPlaylist
    {
        private readonly SmartPlaylistDto _dto;
        private long _internalid = 0;
        public SmartPlaylist(Guid id, string name, Guid userId, RuleBase[] rules,
            SmartPlaylistLimit limit, DateTimeOffset? lastShuffleUpdate, UpdateType updateType, SmartType smartType, long internalId, bool forceCreate, SmartPlaylistDto dto)
        {
            _dto = dto;
            Id = id;
            Name = name;
            UserId = userId;
            Rules = rules;
            Limit = limit;
            LastShuffleUpdate = lastShuffleUpdate;
            UpdateType = updateType;
            SmartType = smartType;
            InternalId = internalId;
            MediaType = MediaTypeGetter.Get(rules);
            ForceCreate = forceCreate;
        }

        public Guid Id { get; }
        public string Name { get; }
        public Guid UserId { get; }
        public RuleBase[] Rules { get; }

        public SmartPlaylistLimit Limit { get; }

        public UpdateType UpdateType { get; }
        public SmartType SmartType { get; }
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

            if (IsShuffleUpdateType)
            {
                newItems = newItems.Shuffle();
            }
            else
            {
                newItems = OrderItems(newItems);
            }

            newItems = newItems.Take(Limit.MaxItems);

            return newItems;
        }

        private static IEnumerable<BaseItem> RemoveMissingEpisodes(IEnumerable<BaseItem> items)
        {
            return items.Where(x => !(x is Episode episode && episode.IsMissingEpisode));
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

        public void RemovePriorName(string priorName)
        {
            for (int x = 0; x < _dto.PriorNames.Length; x++)
            {
                if (string.Equals(priorName, _dto.PriorNames[x], StringComparison.OrdinalIgnoreCase))
                    _dto.PriorNames[x] = null;
            }

            if (_dto.PriorNames.Contains(null))
                _dto.PriorNames = _dto.PriorNames.Where(x => x != null).ToArray();

        }

        public SmartPlaylistDto ToDto()
        {
            return new SmartPlaylistDto
            {
                Id = _dto.Id,
                LastShuffleUpdate = LastShuffleUpdate,
                Limit = _dto.Limit,
                Name = _dto.Name,
                RulesTree = _dto.RulesTree,
                UpdateType = _dto.UpdateType,
                SmartType = _dto.SmartType,
                UserId = _dto.UserId,
                PriorNames = _dto.PriorNames,
                InternalId = _dto.InternalId
            };
        }
    }
}