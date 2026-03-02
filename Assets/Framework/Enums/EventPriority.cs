namespace BH.Framework.Enums
{
    /// <summary>
    /// 事件优先级枚举
    /// </summary>
    public enum EventPriority
    {
        Critical = 0, // 关键事件，立即处理
        High = 1, // 高优先级事件
        Normal = 2, // 普通优先级事件
        Low = 3, // 低优先级事件
        Background = 4 // 后台事件，不阻塞主线程
    }
}