namespace BH.Framework.Infrastructure.Logging.Interfaces
{
    /// <summary>
    /// 日志输出器接口
    /// </summary>
    public interface ILogOutput
    {
        void Output(ILogEntry entry);
    }
}