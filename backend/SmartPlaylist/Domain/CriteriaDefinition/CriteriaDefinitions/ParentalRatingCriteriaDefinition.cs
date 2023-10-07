using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class ParentalRatingCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] OfficialRatings = Plugin.Instance.LibraryManager.GetOfficialRatings(new InternalItemsQuery())
            .Items.OrderBy(x => x).Select(x => ListValue.Create(x)).ToArray();

        public override string Name => "Parental Rating";

        public override CriteriaDefinitionType Type => new ListValueDefinitionType(OfficialRatings.Length == 0 ? ListValue.Create("") : OfficialRatings.First() as ListValue);

        public override Value[] Values { get; } = OfficialRatings;

        public override Value GetValue(UserItem item)
        {
            var rating = item.Item.OfficialRating;
            if (string.IsNullOrEmpty(rating)) return Value.None;

            return ListValue.Create(rating);
        }
    }
}