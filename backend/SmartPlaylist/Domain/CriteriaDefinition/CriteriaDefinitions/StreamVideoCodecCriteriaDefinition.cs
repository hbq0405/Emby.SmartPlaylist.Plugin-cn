using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;
using System.Linq;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class StreamVideoCodecCriteriaDefinition : CriteriaDefinition
    {
        private static readonly Value[] VideoCodecsValues = Plugin.Instance.LibraryManager.GetVideoCodecs(new MediaBrowser.Controller.Entities.InternalItemsQuery())
            .Items.OrderBy(x => x).Select(x => ListValue.Create(x)).ToArray();

        public override string Name => "Stream: Video Codec";

        public override CriteriaDefinitionType Type => new ListValueDefinitionType(VideoCodecsValues.Count() == 0 ? ListValue.Create("") : VideoCodecsValues.First() as ListValue);

        public override Value[] Values => VideoCodecsValues;

        public override Value GetValue(UserItem item)
        {
            var video = item.Item.GetMediaStreams().Where(x => x.Type == MediaBrowser.Model.Entities.MediaStreamType.Video);
            if (video.Any())
                return ArrayValue<ListValue>.Create(video.Select(x => x.Codec).Distinct().Select(x => ListValue.Create(x)).ToArray());

            return Value.None;
        }
    }
}