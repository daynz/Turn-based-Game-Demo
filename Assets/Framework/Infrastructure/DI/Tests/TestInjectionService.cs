namespace BH.Framework.Infrastructure.DI.Tests
{
    /// <summary>
    /// 用于测试依赖注入功能的测试服务
    /// </summary>
    //[AutoRegisterService]
    public class TestInjectionService // : IService
    {
        // public int Priority => (int)PriorityOrder.Game;
        // public bool IsInitialized { get; }
        //
        // // 1. 测试注入具体实现类
        // [field: Inject] private LogService ConcreteLogService { get; set; } = null!;
        //
        // // 2. 测试注入接口
        // [field: Inject] private EventService InterfaceEventService { get; set; } = null!;
        //
        //
        // public Task InitializeAsync()
        // {
        //     try
        //     {
        //         Debug.Log("[TestInjectionService] 开始初始化，验证依赖注入结果...");
        //
        //         // --- 验证注入结果 ---
        //         ValidateDependency(ConcreteLogService, "ConcreteLogService");
        //         ValidateDependency(InterfaceEventService, "InterfaceEventService");
        //
        //         Debug.Log("[TestInjectionService] 依赖注入验证全部通过！");
        //
        //         // --- 使用注入的服务 ---
        //         ConcreteLogService?.Info("TestInjectionService 已成功初始化并记录一条日志。",Name);
        //
        //         Debug.Log("[TestInjectionService] 初始化完成。");
        //         return Task.CompletedTask;
        //     }
        //     catch (Exception exception)
        //     {
        //         return Task.FromException(exception);
        //     }
        // }
        //
        // Task IService.InitializeAsync()
        // {
        //     return InitializeAsync();
        // }
        //
        // public void Shutdown()
        // {
        //     Debug.Log("[TestInjectionService] 正在关闭。");
        // }
        //
        // private void ValidateDependency(object? dependency, string name)
        // {
        //     if (dependency == null)
        //     {
        //         Debug.LogError($"[TestInjectionService] 依赖注入失败：{name} 为 null！");
        //         throw new InvalidOperationException($"依赖注入失败：{name} 为 null！");
        //     }
        //     else
        //     {
        //         Debug.Log($"[TestInjectionService] ✓ 依赖注入成功：{name} ({dependency.GetType().Name})");
        //     }
        // }
    }
}