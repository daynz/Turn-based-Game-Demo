using BH.GameSystems.BattleSystem.Enums;

namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface ICharacterSaveData : IUnitSaveData
    {
        Rarity Rarity { get; set; }
        CombatPath CombatPath { get; set; }
        CombatType CombatType { get; set; }

        float CritRate { get; set; }
        float CritDamage { get; set; }
        float BreakEffect { get; set; }
        float HealingBoost { get; set; }
        float EnergyMax { get; set; }
        float EnergyRegenerationRate { get; set; }
        float EffectHitRate { get; set; }
        float EffectResistance { get; set; }
        float PhysicalDamageBoost { get; set; }
        float FireDamageBoost { get; set; }
        float IceDamageBoost { get; set; }
        float LightningDamageBoost { get; set; }
        float WindDamageBoost { get; set; }
        float QuantumDamageBoost { get; set; }
        float ImaginaryDamageBoost { get; set; }
        float PhysicalResistance { get; set; }
        float FireResistance { get; set; }
        float IceResistance { get; set; }
        float LightningResistance { get; set; }
        float WindResistance { get; set; }
        float QuantumResistance { get; set; }
        float ImaginaryResistance { get; set; }
    }
}