using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;
using SmartPlaylist.Extensions;
using System.Linq;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class FavoriteParentCriteriaDefinition : UserCriteriaDefinition
    {
        private static readonly ListValue FavoriteListValue = ListValue.Create("Favorite");
        private static readonly ListValue LikedListValue = ListValue.Create("Liked");
        private static readonly ListValue NoneListValue = ListValue.Create("None");
        public override string Name => "Favorite (Tree Crawl)";
        public override CriteriaDefinitionType Type => new ListValueDefinitionType(FavoriteListValue);

        public override Value[] Values
        {
            get { return new Value[] { FavoriteListValue, LikedListValue, NoneListValue }; }
        }

        public override Value GetValue(UserItem item)
        {
            BaseItem find = RecurseFavorite(item.Item, new HashSet<BaseItem>()).FirstOrDefault(x => x.IsFavoriteOrLiked(item.User));
            if (find != null && UserItem.TryGetUserItemData(out var userData, item.User, find))
            {
                return userData.IsFavorite ? FavoriteListValue : LikedListValue;
            }

            return NoneListValue;

        }

        private HashSet<BaseItem> RecurseFavorite(BaseItem item, HashSet<BaseItem> current)
        {
            if (!current.Contains(item))
                current.Add(item);

            return (item.Parent != null) ? RecurseFavorite(item.Parent, current) : current;
        }
    }
}