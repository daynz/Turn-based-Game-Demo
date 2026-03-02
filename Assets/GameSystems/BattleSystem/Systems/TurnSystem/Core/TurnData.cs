using System;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.Core
{
    /// <summary>
    /// 回合数据
    /// </summary>
    [Serializable]
    public class TurnData
    {
        /// <summary>
        /// 回合角色
        /// </summary>
        [SerializeField] private BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> unit;

        /// <summary>
        /// 回合序号
        /// </summary>
        [SerializeField] private int turnNumber;

        [SerializeField] private bool isCompleted;

        public bool IsCompleted
        {
            get => isCompleted;
            set => isCompleted = value;
        }

        public BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> Unit
        {
            get => unit;
            set => unit = value;
        }

        public int TurnNumber
        {
            get => turnNumber;
            set => turnNumber = value;
        }

        /// <summary>
        /// 回合描述
        /// </summary>
        public string Description => $"第{TurnNumber}回合";
    }
}