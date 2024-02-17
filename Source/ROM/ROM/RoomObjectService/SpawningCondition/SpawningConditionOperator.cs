using Newtonsoft.Json.Linq;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService.SpawningCondition
{
    public interface ISpawningConditionOperator
    {
        string ConditionTypeId { get; }

        ISpawningCondition CreateNew();

        ISpawningCondition Load(JToken conditionData);

        JToken Save(ISpawningCondition condition);

        IEnumerable<IObjectEditorElement> GetEditorElements(ISpawningCondition condition);
    }

    public abstract class SpawningConditionOperator<TCondition> : ISpawningConditionOperator
        where TCondition : ISpawningCondition
    {
        public abstract string ConditionTypeId { get; }

        public abstract TCondition CreateNew();
        ISpawningCondition ISpawningConditionOperator.CreateNew() => CreateNew();

        public abstract TCondition Load(JToken conditionData);
        ISpawningCondition ISpawningConditionOperator.Load(JToken conditionData) => Load(conditionData);

        public abstract JToken Save(TCondition conditionData);
        JToken ISpawningConditionOperator.Save(ISpawningCondition condition) => Save(AssureConditionType(condition));

        public abstract IEnumerable<IObjectEditorElement> GetEditorElements(TCondition condition);
        IEnumerable<IObjectEditorElement> ISpawningConditionOperator.GetEditorElements(ISpawningCondition condition) => GetEditorElements(AssureConditionType(condition));

        private TCondition AssureConditionType(ISpawningCondition condition)
        {
            if (condition is TCondition tcondition)
            {
                return tcondition;
            }

            throw new ArgumentException($"The received condition was of wrong type {condition.GetType()} while an instance of {typeof(TCondition)} was expected", nameof(condition));
        }
    }

    public static class SpawningConditionOperator
    {
        internal static Dictionary<string, ISpawningConditionOperator> ConditionTypeOperators { get; } = [];

        public static void RegisterConditionType(ISpawningConditionOperator conditionTypeOperator)
        {
            if (ConditionTypeOperators.ContainsKey(conditionTypeOperator.ConditionTypeId))
            {
                ROMPlugin.Logger?.LogWarning($"Operator for condition type {conditionTypeOperator.ConditionTypeId} already is set, overwriting.");
            }

            ConditionTypeOperators[conditionTypeOperator.ConditionTypeId] = conditionTypeOperator;
        }

        public static void RegisterConditionType<TOperator>()
            where TOperator : ISpawningConditionOperator, new()
        {
            RegisterConditionType(new TOperator());
        }
    }
}
