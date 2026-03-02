namespace BH.Framework.Enums
{
    /// <summary>
    /// 事件处理结果
    /// </summary>
    public enum EventHandleResult
    {
        Success, // 处理成功
        Failed, // 处理失败
        Handled, // 已处理完成
        Skipped, // 跳过处理
        Pending // 等待处理
    }
}