using ROM.RoomObjectService.SpawningCondition;
using ROM.RoomObjectService.SpawningCondition.CampaignId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM
{
    internal static class BuiltinTypeReistration
    {
        public static void RegisterObjectTypes()
        {
        }

        public static void RegisterSpawningConditionTypes()
        {
            SpawningConditionOperator.RegisterConditionType<CampaignIdOperator>();
        }
    }
}
