using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.Core
{
    public class TurnStats
    {
        public BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> Unit;
        public int TotalTurns;
        public float AverageActionCount;
        public int TotalDamageDealt;
        public int TotalHealingDone;

        /// <summary>
        /// 每回合平均伤害
        /// </summary>
        public float AverageDamagePerTurn
        {
            get
            {
                if (TotalTurns == 0) return 0f;
                return (float)TotalDamageDealt / TotalTurns;
            }
        }

        /// <summary>
        /// 每回合平均治疗
        /// </summary>
        public float AverageHealingPerTurn
        {
            get
            {
                if (TotalTurns == 0) return 0f;
                return (float)TotalHealingDone / TotalTurns;
            }
        }
    }
}