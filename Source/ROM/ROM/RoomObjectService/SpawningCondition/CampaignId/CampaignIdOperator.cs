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
    public class CampaignIdOperator : SpawningConditionOperator<CampaignIdsSpawningCondition>
    {
        public override string ConditionTypeId => "CampaignId";

        public override CampaignIdsSpawningCondition CreateNew()
        {
            return new CampaignIdsSpawningCondition();
        }

        public override CampaignIdsSpawningCondition Load(JToken conditionData)
        {
            throw new NotImplementedException();
        }

        public override JToken Save(CampaignIdsSpawningCondition conditionData)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IObjectEditorElement> GetEditorElements(CampaignIdsSpawningCondition condition)
        {
            yield return Elements.Checkbox(nameof(CampaignIdsSpawningCondition.IsWhitelist),
                getter: () => condition.IsWhitelist,
                setter: val => condition.IsWhitelist = val);
        }
    }
}
