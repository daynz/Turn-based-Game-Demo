namespace BH.Framework.Enums
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug = 0, // 调试信息（灰色）
        Info = 1, // 一般信息（白色）
        Event = 2, // 事件
        Warning = 3, // 警告（黄色）
        Error = 4, // 错误（红色）
        Critical = 5, // 严重错误（深红）
    }
}