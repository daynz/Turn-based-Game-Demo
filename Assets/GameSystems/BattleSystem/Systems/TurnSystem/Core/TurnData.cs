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
        /// 回合序号
        /// </summary>
        [SerializeField] private int turnNumber;
        
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