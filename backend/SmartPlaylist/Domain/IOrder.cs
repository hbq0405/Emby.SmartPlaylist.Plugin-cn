using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using SmartPlaylist.Comparers;
using SmartPlaylist.Contracts;
using SmartPlaylist.Extensions;

namespace SmartPlaylist.Domain
{
    public class SmartPlaylistLimit
    {
        public SmartPlaylistLimit() { }
        public SmartPlaylistLimit(SmartPlaylistLimitDto dto)
        {
            MaxItems = dto.HasLimit ? dto.MaxItems : 100000;
            OrderBy = dto.HasLimit ? IOrder.GetOrderFromString(dto.OrderBy) : new OrderNone();
        }

        public static readonly SmartPlaylistLimit None = new SmartPlaylistLimit
        {
            MaxItems = 100000,
            OrderBy = new OrderNone()
        };

        public int MaxItems { get; set; }

        public IOrder OrderBy { get; set; }

        public bool HasLimit => !(OrderBy is OrderNone);

        public SmartPlaylistLimitDto ToDto()
        {
            return new SmartPlaylistLimitDto()
            {
                HasLimit = HasLimit,
                MaxItems = HasLimit ? MaxItems : 0,
                OrderBy = OrderBy.Name
            };
        }
    }

    public class OrderNone : IOrder
    {
        public override string Name => "None";
    }

    public static class DefinedOrders
    {
        public static readonly IOrder[] All = typeof(IOrder).Assembly.FindAndCreateDerivedTypes<IOrder>()
            .Where(x => x.GetType() != SmartPlaylistLimit.None.OrderBy.GetType()).ToArray();

        public static readonly string[] AllNames = All.Select(x => x.Name).ToArray();
    }

    public abstract class IOrder
    {
        public abstract string Name { get; }

        public virtual ValueTuple<string, SortOrder>[] OrderBy => new (string, SortOrder)[0];

        public virtual IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items;
        }

        public static IOrder GetOrderFromString(string orderBy)
        {
            return DefinedOrders.All.FirstOrDefault(x =>
                                string.Equals(x.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)) ??
                          SmartPlaylistLimit.None.OrderBy;
        }
    }

    public class OrderRandom : IOrder
    {
        public override string Name => "Random";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.Random, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.Shuffle();
        }
    }

    public class OrderAlbum : IOrder
    {
        public override string Name => "Album";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.Album, SortOrder.Ascending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.Album);
        }
    }


    public class OrderArtist : IOrder
    {
        public override string Name => "Artist";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.Artist, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x, new ArtistsComparer(a => a.Artists));
        }
    }

    public class OrderAlbumArtist : IOrder
    {
        public override string Name => "Album artist";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.AlbumArtist, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x, new ArtistsComparer(a => a.AlbumArtists));
        }
    }

    public class OrderMostFavorite : IOrder
    {
        public override string Name => "Most favorite";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.IsFavorite, SortOrder.Descending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.IsFavorite);
        }
    }

    public class OrderLessFavorite : IOrder
    {
        public override string Name => "Less favorite";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.IsFavorite, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.IsFavorite);
        }
    }


    public class OrderAddedDateDesc : IOrder
    {
        public override string Name => "Added date desc";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.DateCreated, SortOrder.Descending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.DateCreated);
        }
    }

    public class OrderAddedDateAsc : IOrder
    {
        public override string Name => "Added date asc";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.DateCreated, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.DateCreated);
        }
    }

    public class OrderMostPlayed : IOrder
    {
        public override string Name => "Most played";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.PlayCount, SortOrder.Descending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.PlayCount);
        }
    }

    public class OrderLeastPlayed : IOrder
    {
        public override string Name => "Least played";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.PlayCount, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.PlayCount);
        }
    }

    public class OrderPlayedDateDesc : IOrder
    {
        public override string Name => "Played date desc";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.DatePlayed, SortOrder.Descending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.LastPlayedDate);
        }
    }

    public class OrderPlayedDateAsc : IOrder
    {
        public override string Name => "Played date asc";

        public override (string, SortOrder)[] OrderBy => new (string, SortOrder)[]
            {(ItemSortBy.DatePlayed, SortOrder.Ascending)};

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.LastPlayedDate);
        }
    }

    public class OrderName : IOrder
    {
        public override string Name => "Name";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.Name, SortOrder.Ascending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.Name);
        }
    }

    public class OrderEpisode : IOrder
    {
        public override string Name => "Episode";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.AiredEpisodeOrder, SortOrder.Ascending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x, new EpisodeComparer());
        }
    }

    public class OrderSortName : IOrder
    {
        public override string Name => "SortName asc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.SortName, SortOrder.Ascending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.SortName);
        }
    }

    public class OrderSortNameDesc : IOrder
    {
        public override string Name => "SortName desc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.SortName, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.SortName);
        }
    }

    public class OrderReleaseDate : IOrder
    {
        public override string Name => "Release date asc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.PremiereDate, SortOrder.Ascending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x, new ReleaseDateComparer());
        }
    }

    public class OrderReleaseDateDesc : IOrder
    {
        public override string Name => "Release date desc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.PremiereDate, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x, new ReleaseDateComparer());
        }
    }

    public class OrderRuntime : IOrder
    {
        public override string Name => "Runtime asc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.Runtime, SortOrder.Ascending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.RunTimeTicks);
        }
    }

    public class OrderRuntimeDesc : IOrder
    {
        public override string Name => "Runtime desc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.Runtime, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.RunTimeTicks);
        }
    }

    public class OrderCommunityRating : IOrder
    {
        public override string Name => "Community rating asc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.CommunityRating, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.CommunityRating);
        }
    }

    public class OrderCommunityRatingDesc : IOrder
    {
        public override string Name => "Community rating desc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.CommunityRating, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.CommunityRating);
        }
    }

    public class OrderParentalRating : IOrder
    {
        public override string Name => "Parental rating asc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.OfficialRating, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderBy(x => x.GetInheritedParentalRatingValue());
        }
    }

    public class OrderParentalRatingDesc : IOrder
    {
        public override string Name => "Parental rating desc";

        public override (string, SortOrder)[] OrderBy =>
            new (string, SortOrder)[] { (ItemSortBy.OfficialRating, SortOrder.Descending) };

        public override IEnumerable<BaseItem> Order(IEnumerable<BaseItem> items)
        {
            return items.OrderByDescending(x => x.GetInheritedParentalRatingValue());
        }
    }

}