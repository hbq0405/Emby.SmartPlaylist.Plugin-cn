using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;
using System.Linq;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class StreamAudioCodecCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] AudioCodecsValues = Plugin.Instance.LibraryManager.GetAudioCodecs(new MediaBrowser.Controller.Entities.InternalItemsQuery())
            .Items.OrderBy(x => x).Select(x => ListValue.Create(x)).ToArray();

        public override string Name => "Stream: Audio Codec";

        public override CriteriaDefinitionType Type => new ListValueDefinitionType(AudioCodecsValues.Count() == 0 ? ListValue.Create("") : AudioCodecsValues.First() as ListValue);

        public override Value[] Values => AudioCodecsValues;

        public override Value GetValue(UserItem item)
        {
            var audio = item.Item.GetMediaStreams().Where(x => x.Type == MediaBrowser.Model.Entities.MediaStreamType.Audio);
            if (audio.Any())
                return ArrayValue<ListValue>.Create(audio.Select(x => x.Codec).Distinct().Select(x => ListValue.Create(x)).ToArray());

            return Value.None;
        }
    }
}