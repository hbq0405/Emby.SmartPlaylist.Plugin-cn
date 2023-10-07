using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;
using System.Linq;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class StreamSubtitleLanguageCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] LanguageValues = Plugin.Instance.LibraryManager.GetStreamLanguages(
            new MediaBrowser.Controller.Entities.InternalItemsQuery(), MediaBrowser.Model.Entities.MediaStreamType.Subtitle)
            .Items.OrderBy(x => x).Select(x => ListValue.Create(x)).ToArray();

        public override string Name => "Stream: Subtitle Language";

        public override CriteriaDefinitionType Type => new ListValueDefinitionType(LanguageValues.Count() == 0 ? ListValue.Create("") : LanguageValues.First() as ListValue);

        public override Value[] Values => LanguageValues;

        public override Value GetValue(UserItem item)
        {
            var subtitle = item.Item.GetMediaStreams().Where(x => x.Type == MediaBrowser.Model.Entities.MediaStreamType.Subtitle);
            if (subtitle.Any())
                return ArrayValue<ListValue>.Create(subtitle.Select(x => x.Language).Distinct().Select(x => ListValue.Create(x)).ToArray());

            return Value.None;
        }
    }
}