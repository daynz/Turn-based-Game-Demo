namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface ICharacterRuntimeData : IUnitRuntimeData
    {
        float CritRate { get; set; }
        float CritDamage { get; set; }
        float BreakEffect { get; set; }
        float HealingBoost { get; set; }
        float EnergyMax { get; set; }
        float Energy { get; set; }
        float EnergyRegenerationRate { get; set; }
        float EffectHitRate { get; set; }
        float EffectResistance { get; set; }

        // 伤害加成
        float PhysicalDamageBoost { get; set; }
        float FireDamageBoost { get; set; }
        float IceDamageBoost { get; set; }
        float LightningDamageBoost { get; set; }
        float WindDamageBoost { get; set; }
        float QuantumDamageBoost { get; set; }
        float ImaginaryDamageBoost { get; set; }

        // 伤害减免
        float PhysicalResistance { get; set; }
        float FireResistance { get; set; }
        float IceResistance { get; set; }
        float LightningResistance { get; set; }
        float WindResistance { get; set; }
        float QuantumResistance { get; set; }
        float ImaginaryResistance { get; set; }
    }
}