using System;
using BH.GameSystems.BattleSystem.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Base
{
    [Serializable]
    public class UnitRuntimeData : IUnitRuntimeData
    {
        [SerializeField] [Tooltip("单位Id")] [JsonProperty]
        protected string unitId = "";

        [Min(0)] [Tooltip("生命值")] [SerializeField] [JsonProperty]
        protected float health;

        [Min(0)] [Tooltip("最大生命值")] [SerializeField] [JsonProperty]
        protected float maxHealth = 100f;

        [Min(0)] [Tooltip("攻击力")] [SerializeField] [JsonProperty]
        protected float attack;

        [Min(0)] [Tooltip("防御力")] [SerializeField] [JsonProperty]
        protected float defense;

        [Min(0)] [Tooltip("速度")] [SerializeField] [JsonProperty]
        protected float speed;

        #region 属性

        public string UnitId => unitId;

        public float Health
        {
            get => health;
            set => health = Mathf.Max(0, value);
        }

        public float MaxHealth
        {
            get => maxHealth;
            set => maxHealth = Mathf.Max(0, value);
        }

        public float Attack
        {
            get => attack;
            set => attack = Mathf.Max(0, value);
        }

        public float Defense
        {
            get => defense;
            set => defense = Mathf.Max(0, value);
        }

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0, value);
        }

        #endregion
    }
}