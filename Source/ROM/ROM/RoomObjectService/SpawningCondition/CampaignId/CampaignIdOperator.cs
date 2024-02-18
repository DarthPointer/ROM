using Newtonsoft.Json.Linq;
using ROM.UserInteraction.InroomManagement;
using ROM.UserInteraction.ObjectEditorElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService.SpawningCondition.CampaignId
{
    public class CampaignIdOperator : SpawningConditionOperator<CampaignIdSpawningCondition>
    {
        public override string ConditionTypeId => "CampaignId";

        public override CampaignIdSpawningCondition CreateNew()
        {
            return new CampaignIdSpawningCondition();
        }

        public override CampaignIdSpawningCondition Load(JToken conditionData)
        {
            throw new NotImplementedException();
        }

        public override JToken Save(CampaignIdSpawningCondition conditionData)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IObjectEditorElement> GetEditorElements(CampaignIdSpawningCondition condition)
        {
            yield return Elements.Checkbox(nameof(CampaignIdSpawningCondition.IsWhitelist),
                getter: () => condition.IsWhitelist,
                setter: val => condition.IsWhitelist = val);
        }
    }
}
