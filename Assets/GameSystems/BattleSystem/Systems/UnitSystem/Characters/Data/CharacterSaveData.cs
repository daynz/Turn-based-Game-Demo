using System;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data
{
    /// <summary>
    /// 角色存档数据
    /// </summary>
    [Serializable]
    public class CharacterSaveData : UnitSaveData, ICharacterSaveData
    {
        #region 基础信息

        [Tooltip("角色稀有度")] [SerializeField] private Rarity rarity = Rarity.Rarity5;

        [Tooltip("角色命途")] [SerializeField] private CombatPath combatPath;

        [Tooltip("角色属性类型")] [SerializeField] private CombatType combatType;

        #endregion

        #region 进阶属性

        [Header("进阶属性")] [Tooltip("暴击率")] [SerializeField] [Min(0)]
        private float critRate;

        [Tooltip("暴击伤害")] [SerializeField] [Min(0)]
        private float critDamage;

        [Tooltip("击破特攻")] [SerializeField] [Min(0)]
        private float breakEffect;

        [Tooltip("治疗量加成")] [SerializeField] [Min(0)]
        private float healingBoost;

        [Tooltip("能量上限")] [SerializeField] [Min(0)]
        private float energyMax;

        [Tooltip("能量恢复效率")] [SerializeField] [Min(0)]
        private float energyRegenerationRate;

        [Tooltip("效果命中")] [SerializeField] [Min(0)]
        private float effectHitRate;

        [Tooltip("效果抵抗")] [SerializeField] [Min(0)]
        private float effectResistance;

        #endregion

        #region 伤害&抗性属性

        [Header("伤害&抗性属性")] [Tooltip("物理伤害提高")] [SerializeField] [Min(0)]
        private float physicalDamageBoost;

        [Tooltip("火伤害提高")] [SerializeField] [Min(0)]
        private float fireDamageBoost;

        [Tooltip("冰伤害提高")] [SerializeField] [Min(0)]
        private float iceDamageBoost;

        [Tooltip("雷伤害提高")] [SerializeField] [Min(0)]
        private float lightningDamageBoost;

        [Tooltip("风伤害提高")] [SerializeField] [Min(0)]
        private float windDamageBoost;

        [Tooltip("量子伤害提高")] [SerializeField] [Min(0)]
        private float quantumDamageBoost;

        [Tooltip("虚数伤害提高")] [SerializeField] [Min(0)]
        private float imaginaryDamageBoost;

        [Tooltip("物理抗性")] [SerializeField] [Min(0)]
        private float physicalResistance;

        [Tooltip("火抗性")] [SerializeField] [Min(0)]
        private float fireResistance;

        [Tooltip("冰抗性")] [SerializeField] [Min(0)]
        private float iceResistance;

        [Tooltip("雷抗性")] [SerializeField] [Min(0)]
        private float lightningResistance;

        [Tooltip("风抗性")] [SerializeField] [Min(0)]
        private float windResistance;

        [Tooltip("量子抗性")] [SerializeField] [Min(0)]
        private float quantumResistance;

        [Tooltip("虚数抗性")] [SerializeField] [Min(0)]
        private float imaginaryResistance;

        #endregion

        #region 属性

        public Rarity Rarity
        {
            get => rarity;
            set
            {
                if (value == Rarity.Rarity4 || value == Rarity.Rarity5)
                    rarity = value;
                else
                    LOGService.Warning($"角色稀有度只能是 {Rarity.Rarity4} 或 {Rarity.Rarity5}，当前值：{value}", Name);
            }
        }

        public CombatPath CombatPath
        {
            get => combatPath;
            set => combatPath = value;
        }

        public CombatType CombatType
        {
            get => combatType;
            set => combatType = value;
        }

        public float CritRate
        {
            get => critRate;
            set => critRate = value;
        }

        public float CritDamage
        {
            get => critDamage;
            set => critDamage = value;
        }

        public float BreakEffect
        {
            get => breakEffect;
            set => breakEffect = value;
        }

        public float HealingBoost
        {
            get => healingBoost;
            set => healingBoost = value;
        }

        public float EnergyMax
        {
            get => energyMax;
            set => energyMax = value;
        }

        public float EnergyRegenerationRate
        {
            get => energyRegenerationRate;
            set => energyRegenerationRate = value;
        }

        public float EffectHitRate
        {
            get => effectHitRate;
            set => effectHitRate = value;
        }

        public float EffectResistance
        {
            get => effectResistance;
            set => effectResistance = value;
        }

        // 伤害加成属性
        public float PhysicalDamageBoost
        {
            get => physicalDamageBoost;
            set => physicalDamageBoost = value;
        }

        public float FireDamageBoost
        {
            get => fireDamageBoost;
            set => fireDamageBoost = value;
        }

        public float IceDamageBoost
        {
            get => iceDamageBoost;
            set => iceDamageBoost = value;
        }

        public float LightningDamageBoost
        {
            get => lightningDamageBoost;
            set => lightningDamageBoost = value;
        }

        public float WindDamageBoost
        {
            get => windDamageBoost;
            set => windDamageBoost = value;
        }

        public float QuantumDamageBoost
        {
            get => quantumDamageBoost;
            set => quantumDamageBoost = value;
        }

        public float ImaginaryDamageBoost
        {
            get => imaginaryDamageBoost;
            set => imaginaryDamageBoost = value;
        }

        // 抗性属性
        public float PhysicalResistance
        {
            get => physicalResistance;
            set => physicalResistance = value;
        }

        public float FireResistance
        {
            get => fireResistance;
            set => fireResistance = value;
        }

        public float IceResistance
        {
            get => iceResistance;
            set => iceResistance = value;
        }

        public float LightningResistance
        {
            get => lightningResistance;
            set => lightningResistance = value;
        }

        public float WindResistance
        {
            get => windResistance;
            set => windResistance = value;
        }

        public float QuantumResistance
        {
            get => quantumResistance;
            set => quantumResistance = value;
        }

        public float ImaginaryResistance
        {
            get => imaginaryResistance;
            set => imaginaryResistance = value;
        }

        #endregion

        // #region 函数覆写
        //
        // public override string ToJson()
        // {
        //     return base.ToJson();
        // }
        //
        // public override void FromJson(string json)
        // {
        //     base.FromJson(json);
        // }
        //
        // #endregion
    }
}