using System.Threading.Tasks;

namespace BH.Framework.Infrastructure.DI.Interfaces
{
    /// <summary>
    /// 所有可被容器管理的服务必须实现的接口
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 初始化优先级 (数值越小越先执行)
        /// </summary>
        int Priority { get; }
        
        bool IsInitialized { get; }

        /// <summary>
        /// 组合初始化方法
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 清理资源
        /// </summary>
        void Shutdown();
    }
}