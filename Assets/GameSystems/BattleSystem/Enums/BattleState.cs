namespace BH.GameSystems.BattleSystem.Enums
{
    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum BattleState
    {
        NotStarted = 0,     // 未开始
        Preparation = 1,    // 准备中
        InProgress = 2,     // 进行中
        Paused = 3,         // 暂停
        Ended = 4           // 结束
    }
}