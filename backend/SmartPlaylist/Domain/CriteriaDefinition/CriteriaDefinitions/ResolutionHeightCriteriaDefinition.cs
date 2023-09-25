using System.Diagnostics;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using SmartPlaylist.Domain.Rule;
using SmartPlaylist.Domain.Values;
using System.Linq;

namespace SmartPlaylist.Domain.CriteriaDefinition.CriteriaDefinitions
{
    public class ResolutionHeightCriteriaDefinition : CriteriaDefinition
    {
        public override string Name => "Resolution Height";

        public override CriteriaDefinitionType Type => NumberDefinitionType.Instance;

        public override Value GetValue(UserItem item)
        {
            if (item.Item is Video video)
                return NumberValue.Create(video.Height);

            return Value.None;
        }
    }
}