using System.Linq;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class GenreListedCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] ListValues = GenreGetter.Get().Select(s => ListValue.Create(s)).ToArray();

        public override string Name => "Genre (Listed)";
        public override CriteriaDefinitionType Type => new ListValueDefinitionType(ListValues.First() as ListValue);

        public override Value[] Values => ListValues;
        public override Value GetValue(UserItem item)
        {
            return ArrayValue<ListValue>.Create(item.Item.Genres?.Select(s => ListValue.Create(s)).ToArray());
        }
    }
}