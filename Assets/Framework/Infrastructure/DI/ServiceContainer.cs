using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.DI.Interfaces;
using BH.Framework.Singleton;
using Debug = UnityEngine.Debug;

namespace BH.Framework.Infrastructure.DI
{
    /// <summary>
    /// 生产级轻量级依赖注入容器
    /// </summary>
    public class ServiceContainer : CSharpSingleton<ServiceContainer>
    {
        #region 字段与属性

        private readonly Dictionary<Type, object> _services = new();
        private readonly List<IService> _initializedServices = new();

        // 反射缓存：Type -> List<FieldInfo> (仅缓存带有 [Inject] 的字段)
        private readonly Dictionary<Type, List<FieldInfo>> _injectionCache = new();
        private bool _isLocked;
        private bool _isInitializing;

        #endregion

        #region 服务注册

        /// <summary>
        /// 注册服务实例
        /// </summary>
        public void Register<T>(T service) where T : IService
        {
            if (_isLocked)
                throw new InvalidOperationException("容器已锁定，禁止在运行时注册服务。");
            
            var type = service.GetType();
            if (_services.ContainsKey(type))
            {
                Debug.Log($"[DI] 服务重复注册：{type.Name}。旧实例将被移除。");
                _services.Remove(type);
            }

            _services[type] = service;
            Debug.Log($"[DI] 已注册：{type.Name}");
        }

        #endregion

        #region 初始化生命周期

        /// <summary>
        /// 执行全量初始化 (包含注入和异步Init)
        /// </summary>
        public async Task InitializeAllAsync()
        {
            if (_isInitializing) return;
            if (_initializedServices.Count > 0)
            {
                Debug.LogWarning("[DI] 服务已初始化，跳过。");
                return;
            }

            _isInitializing = true;
            _isLocked = true;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                Debug.Log("=== [DI] 开始服务初始化流程 ===");

                // 1. 准备阶段：按优先级排序
                var sortedServices = _services.Values
                    .OfType<IService>()
                    .OrderBy(s => s.Priority)
                    .ToList();

                Debug.Log(
                    $"[DI] 共发现 {sortedServices.Count} 个服务，最大优先级差：" +
                    $"{sortedServices.LastOrDefault()?.Priority - sortedServices.FirstOrDefault()?.Priority ?? 0}");

                // 2. 执行阶段：串行初始化
                for (var i = 0; i < sortedServices.Count; i++)
                {
                    var service = sortedServices[i];
                    await InitializeSingleServiceAsync(service, i + 1, sortedServices.Count);
                }

                stopwatch.Stop();
                Debug.Log($"=== [DI] 所有服务初始化成功 (耗时: {stopwatch.ElapsedMilliseconds}ms) ===");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DI] 初始化流程中断: {ex.Message}");
                // 关键：发生错误时，自动回滚已启动的服务
                await RollbackAsync();
                throw; // 向上抛出，通知启动器停止游戏
            }
            finally
            {
                _isInitializing = false;
            }
        }

        /// <summary>
        /// 初始化单个服务 (包含注入和异常处理)
        /// </summary>
        private async Task InitializeSingleServiceAsync(IService service, int index, int total)
        {
            var type = service.GetType();
            var name = type.Name;
            var sw = Stopwatch.StartNew();

            try
            {
                Debug.Log($"[DI] [{index}/{total}] 正在初始化：{name} (Priority: {service.Priority})...");

                // 依赖注入 (如果在注册后有新服务加入，这里会解析到最新状态)
                InjectDependencies(service);

                // 执行组合的异步初始化
                await service.InitializeAsync();
                
                _initializedServices.Add(service);
                sw.Stop();
                Debug.Log($"[DI] ✓ {name} 初始化完成 ({sw.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                // 记录详细错误，包含堆栈
                Debug.LogError(
                    $"[DI] ✗ {name} 初始化失败 (耗时: {sw.ElapsedMilliseconds}ms)\n错误: {ex}\n堆栈: {ex.StackTrace}");
                // 抛出包装异常，明确是哪个服务挂了
                throw new Exception($"服务初始化失败: {name}", ex);
            }
        }

        /// <summary>
        /// 错误回滚机制：逆序关闭所有已成功初始化的服务
        /// </summary>
        private async Task RollbackAsync()
        {
            if (_initializedServices.Count == 0) return;

            Debug.Log("[DI] 触发回滚机制，正在清理已启动的服务...");

            // 逆序关闭
            for (var i = _initializedServices.Count - 1; i >= 0; i--)
            {
                var service = _initializedServices[i];
                try
                {
                    Debug.Log($"[DI] 回滚清理：{service.GetType().Name}");

                    // 将同步的 Shutdown 操作包装在 Task 中，使其在后台线程执行
                    await Task.Run(() => service.Shutdown());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DI] 回滚清理失败：{service.GetType().Name} - {ex.Message}");
                }
            }

            _initializedServices.Clear();
            Debug.Log("[DI] 回滚完成。");
        }

        #endregion

        #region 依赖注入逻辑

        /// <summary>
        /// 自动注入依赖 (带缓存优化)<br/>
        /// MonoBehaviour需要手动调用
        /// </summary>
        public void InjectDependencies(object target)
        {
            var type = target.GetType();

            // 1. 检查缓存
            if (!_injectionCache.TryGetValue(type, out var fieldsToInject))
            {
                // 2. 缓存未命中，反射查找
                fieldsToInject = type.GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    )
                    .Where(f => f.GetCustomAttribute<InjectAttribute>() != null)
                    .ToList();
                _injectionCache[type] = fieldsToInject;
            }

            // 3. 执行注入
            foreach (var field in fieldsToInject)
            {
                var fieldType = field.FieldType;
                if (_services.TryGetValue(fieldType, out var serviceInstance))
                {
                    field.SetValue(target, serviceInstance);
                }
                else
                {
                    // 检查是否是可选依赖
                    if (field.GetCustomAttribute<OptionalAttribute>() == null)
                    {
                        throw new Exception($"[DI] 注入失败：{type.Name}.{field.Name} 找不到服务类型 {fieldType.Name}");
                    }

                    Debug.Log($"[DI] 忽略可选依赖缺失：{type.Name}.{field.Name}");
                }
            }
        }

        #endregion

        #region 服务关闭

        /// <summary>
        /// 关闭所有服务
        /// </summary>
        public void ShutdownAll()
        {
            if (_isInitializing)
            {
                Debug.Log("[DI] 不能在初始化过程中关闭！");
                return;
            }

            Debug.Log("=== [DI] 正在关闭所有服务 ===");
            for (var i = _initializedServices.Count - 1; i >= 0; i--)
            {
                var service = _initializedServices[i];
                try
                {
                    service.Shutdown();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DI] 服务关闭异常：{service.GetType().Name} - {ex.Message}");
                }
            }

            _initializedServices.Clear();
            _services.Clear();
            _injectionCache.Clear();
            _isLocked = false;
            Debug.Log("=== [DI] 所有服务已关闭 ===");
        }

        #endregion
    }
}