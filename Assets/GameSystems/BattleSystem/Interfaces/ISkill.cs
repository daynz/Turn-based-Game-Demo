namespace BH.GameSystems.BattleSystem.Interfaces
{
    /// <summary>
    /// 技能接口
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// 技能ID
        /// </summary>
        string SkillId { get; }

        // /// <summary>
        // /// 技能配置
        // /// </summary>
        // SkillConfig Config { get; }
        //
        // /// <summary>
        // /// 技能名称
        // /// </summary>
        // string SkillName { get; }
        //
        // /// <summary>
        // /// 技能描述
        // /// </summary>
        // string Description { get; }
        //
        // /// <summary>
        // /// 冷却时间
        // /// </summary>
        // float Cooldown { get; }
        //
        // /// <summary>
        // /// 当前冷却时间
        // /// </summary>
        // float CurrentCooldown { get; }
        //
        // /// <summary>
        // /// 技能是否可用
        // /// </summary>
        // bool IsReady { get; }
        //
        // /// <summary>
        // /// 技能消耗类型
        // /// </summary>
        // SkillCostType CostType { get; }
        //
        // /// <summary>
        // /// 技能消耗值
        // /// </summary>
        // int CostValue { get; }
        //
        // /// <summary>
        // /// 初始化技能
        // /// </summary>
        // void Initialize(SkillConfig config);
        //
        // /// <summary>
        // /// 检查施放条件
        // /// </summary>
        // bool CanCast(ICharacter caster, SkillContext context);
        //
        // /// <summary>
        // /// 施放技能
        // /// </summary>
        // SkillCastResult Cast(ICharacter caster, SkillContext context);
        //
        // /// <summary>
        // /// 更新技能状态
        // /// </summary>
        // void Update(float deltaTime);
        //
        // /// <summary>
        // /// 重置技能状态
        // /// </summary>
        // void Reset();
        //
        // /// <summary>
        // /// 获取技能范围
        // /// </summary>
        // SkillRange GetSkillRange();
        //
        // /// <summary>
        // /// 获取技能目标
        // /// </summary>
        // List<ICharacter> GetTargets(ICharacter caster, SkillTargetInfo targetInfo);
    }
}