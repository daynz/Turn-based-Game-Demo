using System;
using System.Text;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data
{
    [Serializable]
    public class CharacterData : UnitData<CharacterSaveData, CharacterRuntimeData>, ICharacterData
    {
        private const int MinLevel = 1;
        private const int MaxLevel = 80;

        /// <summary>
        /// 等级（范围 1-80）
        /// </summary>
        public override int Level
        {
            get => level;
            set
            {
                if (value is >= MinLevel and <= MaxLevel)
                    level = value;
                else
                    LOGService.Warning($"等级必须在 {MinLevel}-{MaxLevel} 之间，当前值：{value}");
            }
        }

        ICharacterSaveData IUnitData<ICharacterSaveData, ICharacterRuntimeData>.SaveData => SaveData;
        ICharacterRuntimeData IUnitData<ICharacterSaveData, ICharacterRuntimeData>.RuntimeData => RuntimeData;

        /// <summary>
        /// 返回包含所有数据的字符串表示
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            // 基础信息
            sb.AppendLine($"Character Data: {Name} (ID: {UnitId})");
            sb.AppendLine($"Level: {Level}");

            // 存档数据
            if (SaveData != null)
            {
                sb.AppendLine("--- Save Data ---");
                sb.AppendLine($"Rarity: {SaveData.Rarity}");
                sb.AppendLine($"CombatPath: {SaveData.CombatPath}");
                sb.AppendLine($"CombatType: {SaveData.CombatType}");
                sb.AppendLine($"CritRate: {SaveData.CritRate}");
                sb.AppendLine($"CritDamage: {SaveData.CritDamage}");
                sb.AppendLine($"BreakEffect: {SaveData.BreakEffect}");
                sb.AppendLine($"HealingBoost: {SaveData.HealingBoost}");
                sb.AppendLine($"EnergyMax: {SaveData.EnergyMax}");
                sb.AppendLine($"EnergyRegenerationRate: {SaveData.EnergyRegenerationRate}");
                sb.AppendLine($"EffectHitRate: {SaveData.EffectHitRate}");
                sb.AppendLine($"EffectResistance: {SaveData.EffectResistance}");
                // 伤害加成
                sb.AppendLine($"PhysicalDamageBoost: {SaveData.PhysicalDamageBoost}");
                sb.AppendLine($"FireDamageBoost: {SaveData.FireDamageBoost}");
                sb.AppendLine($"IceDamageBoost: {SaveData.IceDamageBoost}");
                sb.AppendLine($"LightningDamageBoost: {SaveData.LightningDamageBoost}");
                sb.AppendLine($"WindDamageBoost: {SaveData.WindDamageBoost}");
                sb.AppendLine($"QuantumDamageBoost: {SaveData.QuantumDamageBoost}");
                sb.AppendLine($"ImaginaryDamageBoost: {SaveData.ImaginaryDamageBoost}");
                // 抗性
                sb.AppendLine($"PhysicalResistance: {SaveData.PhysicalResistance}");
                sb.AppendLine($"FireResistance: {SaveData.FireResistance}");
                sb.AppendLine($"IceResistance: {SaveData.IceResistance}");
                sb.AppendLine($"LightningResistance: {SaveData.LightningResistance}");
                sb.AppendLine($"WindResistance: {SaveData.WindResistance}");
                sb.AppendLine($"QuantumResistance: {SaveData.QuantumResistance}");
                sb.AppendLine($"ImaginaryResistance: {SaveData.ImaginaryResistance}");
            }
            else
            {
                sb.AppendLine("SaveData is null");
            }

            // 运行时数据
            if (RuntimeData != null)
            {
                sb.AppendLine("--- Runtime Data ---");
                sb.AppendLine($"Health: {RuntimeData.Health} / {RuntimeData.MaxHealth}");
                sb.AppendLine($"Attack: {RuntimeData.Attack}");
                sb.AppendLine($"Defense: {RuntimeData.Defense}");
                sb.AppendLine($"Speed: {RuntimeData.Speed}");
                sb.AppendLine($"CritRate: {RuntimeData.CritRate}");
                sb.AppendLine($"CritDamage: {RuntimeData.CritDamage}");
                sb.AppendLine($"BreakEffect: {RuntimeData.BreakEffect}");
                sb.AppendLine($"HealingBoost: {RuntimeData.HealingBoost}");
                sb.AppendLine($"Energy: {RuntimeData.Energy} / {RuntimeData.EnergyMax}");
                sb.AppendLine($"EnergyRegenerationRate: {RuntimeData.EnergyRegenerationRate}");
                sb.AppendLine($"EffectHitRate: {RuntimeData.EffectHitRate}");
                sb.AppendLine($"EffectResistance: {RuntimeData.EffectResistance}");
                // 伤害加成
                sb.AppendLine($"PhysicalDamageBoost: {RuntimeData.PhysicalDamageBoost}");
                sb.AppendLine($"FireDamageBoost: {RuntimeData.FireDamageBoost}");
                sb.AppendLine($"IceDamageBoost: {RuntimeData.IceDamageBoost}");
                sb.AppendLine($"LightningDamageBoost: {RuntimeData.LightningDamageBoost}");
                sb.AppendLine($"WindDamageBoost: {RuntimeData.WindDamageBoost}");
                sb.AppendLine($"QuantumDamageBoost: {RuntimeData.QuantumDamageBoost}");
                sb.AppendLine($"ImaginaryDamageBoost: {RuntimeData.ImaginaryDamageBoost}");
                // 抗性
                sb.AppendLine($"PhysicalResistance: {RuntimeData.PhysicalResistance}");
                sb.AppendLine($"FireResistance: {RuntimeData.FireResistance}");
                sb.AppendLine($"IceResistance: {RuntimeData.IceResistance}");
                sb.AppendLine($"LightningResistance: {RuntimeData.LightningResistance}");
                sb.AppendLine($"WindResistance: {RuntimeData.WindResistance}");
                sb.AppendLine($"QuantumResistance: {RuntimeData.QuantumResistance}");
                sb.AppendLine($"ImaginaryResistance: {RuntimeData.ImaginaryResistance}");
            }
            else
            {
                sb.AppendLine("RuntimeData is null");
            }

            return sb.ToString();
        }
    }
}