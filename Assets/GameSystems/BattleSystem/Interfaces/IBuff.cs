using System.Collections.Generic;
using BH.GameSystems.BattleSystem.Enums;

namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface IBuff
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BriefDescription { get; set; }
        public BuffType Type { get; set; }
        public string Target { get; set; }

        public void SetTarget(string target);
        public bool OnApply(List<IBuff> existingBuffs);
        public void OnRemove();
    }
}