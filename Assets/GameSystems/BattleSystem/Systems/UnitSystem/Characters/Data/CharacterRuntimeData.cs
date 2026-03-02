using System;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data
{
    /// <summary>
    /// 角色运行时数据
    /// </summary>
    [Serializable]
    public class CharacterRuntimeData : UnitRuntimeData, ICharacterRuntimeData
    {
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

        [Tooltip("当前能量")] [SerializeField] [Min(0)]
        private float energy;

        [Tooltip("能量恢复效率")] [SerializeField] [Min(0)]
        private float energyRegenerationRate;

        [Tooltip("效果命中")] [SerializeField] [Min(0)]
        private float effectHitRate;

        [Tooltip("效果抵抗")] [SerializeField] [Min(0)]
        private float effectResistance;

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

        #region 属性

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

        public float Energy
        {
            get => energy;
            set => energy = Mathf.Clamp(value, 0, EnergyMax);
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
    }
}