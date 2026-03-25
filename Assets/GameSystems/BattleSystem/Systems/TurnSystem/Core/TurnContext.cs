using System;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.Core
{
    [Serializable]
    public class TurnContext
    {
        /// <summary>
        /// 回合角色
        /// </summary>
        [SerializeField] private BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> unit;
        
        public BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> Unit
        {
            get => unit;
            set => unit = value;
        }
    }
}