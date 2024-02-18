using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService.SpawningCondition.CampaignId
{
    public class CampaignIdSpawningCondition : ISpawningCondition
    {
        public bool IsWhitelist { get; set; }

        public List<string> CampaignIds { get; set; } = [];

        public bool ShouldSpawn(Room room)
        {
            if (room.game?.StoryCharacter?.value == null)
            {
                ROMPlugin.Logger?.LogWarning($"{nameof(CampaignIdSpawningCondition)} could not obtain campaign id for the provided context. The associated room object will be spawned.");
                return true;
            }

            bool campaignInTheList = CampaignIds.Contains(room.game.StoryCharacter.value);

            return campaignInTheList == IsWhitelist;
        }
    }
}
