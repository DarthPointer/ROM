using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService.SpawningCondition.CampaignId
{
    public class CampaignIdsSpawningCondition : ISpawningCondition
    {
        public bool IsWhitelist { get; set; }

        public List<string> CampaignIds { get; set; } = [];

        public bool ShouldSpawn(Room room)
        {
            bool campaignInTheList = CampaignIds.Contains(room.game.StoryCharacter.value);

            return campaignInTheList == IsWhitelist;
        }
    }
}
