using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;
using System.Linq;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class StreamAudioLanguageCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] LanguageValues = Plugin.Instance.LibraryManager.GetStreamLanguages(
            new MediaBrowser.Controller.Entities.InternalItemsQuery(), MediaBrowser.Model.Entities.MediaStreamType.Audio)
            .Items.OrderBy(x => x).Select(x => ListValue.Create(x)).ToArray();

        public override string Name => "Stream: Audio Language";

        public override CriteriaDefinitionType Type => new ListValueDefinitionType(LanguageValues.Count() == 0 ? ListValue.Create("") : LanguageValues.First() as ListValue);

        public override Value[] Values => LanguageValues;

        public override Value GetValue(UserItem item)
        {
            var audio = item.Item.GetMediaStreams().Where(x => x.Type == MediaBrowser.Model.Entities.MediaStreamType.Audio);
            if (audio.Any())
                return ArrayValue<ListValue>.Create(audio.Select(x => x.Language).Distinct().Select(x => ListValue.Create(x)).ToArray());

            return Value.None;
        }
    }
}