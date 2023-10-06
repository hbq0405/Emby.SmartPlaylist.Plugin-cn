using System.Linq;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class MediaTypeCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] ListNameValues = Const.SupportedItemTypes.OrderBy(x => x.MediaType.Name).Select(s => ListMapValue.Create(s.Description, s.MediaType.Name)).Cast<Value>().ToArray();

        public override string Name => "Media Type";
        public override CriteriaDefinitionType Type => new ListMapValueDefinitionType(ListNameValues.First() as ListMapValue);

        public override Value[] Values => ListNameValues;

        public override Value GetValue(UserItem item)
        {
            string name = item.Item.GetType().Name;
            return ListMapValue.Create(name, name);
        }
    }
}