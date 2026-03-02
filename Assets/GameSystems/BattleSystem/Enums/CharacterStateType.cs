namespace BH.GameSystems.BattleSystem.Enums
{
    /// <summary>
    /// 角色状态类型
    /// </summary>
    public enum CharacterStateType
    {
        None = 0,
        Idle = 1,      // 空闲
        Move = 2,      // 移动
        Attack = 3,    // 攻击
        Skill = 4,     // 技能
        Hurt = 5,      // 受击
        Dead = 6,      // 死亡
        Stun = 7,      // 眩晕
        Frozen = 8,    // 冻结
        Invincible = 9 // 无敌
    }
}