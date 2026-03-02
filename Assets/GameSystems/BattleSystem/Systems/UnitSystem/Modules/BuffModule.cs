using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Modules
{
    /// <summary>
    /// Buff模块
    /// </summary>
    public class BuffModule : BaseModule
    {
        // public override string ModuleName => "BuffModule";
        //
        // /// <summary>
        // /// buff池
        // /// </summary>
        // public Dictionary<string, IBuff> BuffPool;
        //
        // /// <summary>
        // /// 激活的Buff列表
        // /// </summary>
        // private List<IBuff> _buffs;
        //
        // /// <summary>
        // /// Buff按类型分组
        // /// </summary>
        // private Dictionary<BuffType, List<IBuff>> _buffsByType;
        //
        // public override void Initialize(IBattleEntity owner)
        // {
        //     _buffs = new List<IBuff>();
        //     _buffsByType = new Dictionary<BuffType, List<IBuff>>
        //     {
        //         [BuffType.BeneficialEffect] = new List<IBuff>(),
        //         [BuffType.DetrimentalEffect] = new List<IBuff>(),
        //         [BuffType.OtherEffect] = new List<IBuff>()
        //     };
        //     PreloadingBuff(owner);
        // }
        //
        // /// <summary>
        // /// 添加Buff
        // /// </summary>
        // public bool AddBuff(IBuff buff, IBattleEntity caster = null)
        // {
        //     if (!IsEnabled) return false;
        //
        //     // 设置Buff信息
        //     buff.SetTarget(Owner.Data.Attribute.Name);
        //
        //     // 检查是否免疫
        //     if (Owner.IsImmuneToBuff(buff.Type))
        //     {
        //         LogManager.Instance.Info($"{Owner.Data.Attribute.Name} 免疫了 {buff.Name}");
        //         return false;
        //     }
        //
        //     // 检查是否存在相同Buff
        //     var existingBuffs = GetBuffsByType(buff.Type);
        //     bool applied = buff.OnApply(existingBuffs);
        //
        //     if (applied)
        //     {
        //         _buffs.Add(buff);
        //         _buffsByType[buff.Type].Add(buff);
        //         LogManager.Instance.Info($"添加Buff: {buff.Name} 到 {Owner.Data.Attribute.Name}");
        //     }
        //
        //     return applied;
        // }
        //
        // /// <summary>
        // /// 预加载buff
        // /// </summary>
        // /// <param name="battleEntity"></param>
        // private void PreloadingBuff(IBattleEntity battleEntity)
        // {
        //     throw new System.NotImplementedException();
        // }
        //
        // /// <summary>
        // /// 移除Buff
        // /// </summary>
        // public bool RemoveBuff(IBuff buff)
        // {
        //     if (_buffs.Remove(buff))
        //     {
        //         if (_buffsByType.TryGetValue(buff.Type, out var buffList))
        //         {
        //             buffList.Remove(buff);
        //         }
        //
        //         buff.OnRemove();
        //         LogManager.Instance.Info($"移除Buff: {buff.Name} 从 {Owner.Data.Attribute.Name}");
        //
        //         return true;
        //     }
        //
        //     return false;
        // }
        //
        // /// <summary>
        // /// 移除指定类型的所有Buff
        // /// </summary>
        // public void RemoveBuffsByType(BuffType type)
        // {
        //     if (_buffsByType.TryGetValue(type, out var buffList))
        //     {
        //         for (int i = buffList.Count - 1; i >= 0; i--)
        //         {
        //             RemoveBuff(buffList[i]);
        //         }
        //     }
        // }
        //
        // /// <summary>
        // /// 获取指定类型的Buff
        // /// </summary>
        // public List<IBuff> GetBuffsByType(BuffType type)
        // {
        //     if (_buffsByType.TryGetValue(type, out var buffList))
        //     {
        //         return buffList;
        //     }
        //
        //     return new List<IBuff>();
        // }
        //
        // /// <summary>
        // /// 检查是否有指定类型的Buff
        // /// </summary>
        // public bool HasType(BuffType type)
        // {
        //     return _buffsByType.ContainsKey(type) && _buffsByType[type].Count > 0;
        // }
        //
        // /// <summary>
        // /// 清理所有Buff
        // /// </summary>
        // public void ClearAllBuffs()
        // {
        //     for (int i = _buffs.Count - 1; i >= 0; i--)
        //     {
        //         RemoveBuff(_buffs[i]);
        //     }
        // }
        //
        // /// <summary>
        // /// 清理负面Buff
        // /// </summary>
        // public void ClearDebuffs()
        // {
        //     for (int i = _buffs.Count - 1; i >= 0; i--)
        //     {
        //         RemoveBuff(_buffs[i]);
        //     }
        // }
        //
        // public override void OnUpdate(float deltaTime)
        // {
        //     base.OnUpdate(deltaTime);
        // }
        //
        // public override void OnBattleEnd()
        // {
        //     base.OnBattleEnd();
        //
        //     ClearAllBuffs();
        // }
        //
        // public override void Cleanup()
        // {
        //     base.Cleanup();
        //     ClearAllBuffs();
        //     _buffs.Clear();
        //     _buffsByType.Clear();
        // }
        //
        // /// <summary>
        // /// 获取所有的Buff
        // /// </summary>
        // public List<IBuff> GetAllBuffs()
        // {
        //     return new List<IBuff>(_buffs);
        // }
    }
}