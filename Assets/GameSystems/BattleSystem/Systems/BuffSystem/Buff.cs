using System.Collections.Generic;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Interfaces;

namespace BH.GameSystems.BattleSystem.Systems.BuffSystem
{
    public class Buff : IBuff
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BriefDescription { get; set; }
        public BuffType Type { get; set; }
        public string Target { get; set; }
        
        public Buff(string name, string description, string briefDescription, BuffType type, string target)
        {
            Name = name;
            Description = description;
            BriefDescription = briefDescription;
            Type = type;
            Target = target;
        }

        public void SetTarget(string target)
        {
            Target = target;
        }

        // TODO: 修改逻辑
        public bool OnApply(List<IBuff> existingBuffs)
        {
            // 触发buff
            //EventManager.Instance.Publish(new TriggerBuffEvent(this, this), EventType.Battle);
            
            foreach (var buff1 in existingBuffs)
            {
                var buff = (Buff)buff1;
                if (this.Name == buff.Name)
                {
                    return false;
                }
            }
            return true;
        }

        public void OnRemove()
        {
            //EventManager.Instance.Publish(new RemoveBuffEvent(this, this), EventType.Battle);
        }
    }
}