using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.DI;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.DI.Interfaces;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.Logging.Core;
using UnityEngine;

namespace BH.Framework.Managers
{
    [Obsolete]
    public class GameBootstrap : MonoBehaviour
    {
        private string Name => GetType().Name;
        [field: Inject] private EventService EventService { get; set; }
        [field: Inject] private LogService LogService { get; set; }

        #region Unity 生命周期

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private async void Start()
        {
            try
            {
                await InitializeGameAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"游戏启动异常\n{e}");
            }
        }

        #endregion

        #region 游戏初始化流程

        /// <summary>
        /// 初始化游戏核心系统
        /// </summary>
        private async Task InitializeGameAsync()
        {
            try
            {
                Debug.Log($"[{Name}] 开始启动游戏...");

                // 自动扫描并注册所有标记了 [AutoRegisterService] 的服务
                RegisterServicesAutomatically();

                // 启动所有已注册的服务
                await ServiceContainer.Instance.InitializeAllAsync();
                ServiceContainer.Instance.InjectDependencies(this);

                LogService.Info($"依赖注入完毕", Name);
                OnGameStarted();
            }
            catch (Exception ex)
            {
                Debug.Log($"[{Name}] 游戏启动失败: {ex}");
                HandleStartupError(ex);
            }
        }

        /// <summary>
        /// 自动扫描当前程序集中的所有服务并注册
        /// </summary>
        private static void RegisterServicesAutomatically()
        {
            Debug.Log("[GameBootstrap] 正在扫描并自动注册服务...");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // 查找所有标记了 [AutoRegisterService] 的类型
                    var serviceTypes = assembly.GetTypes()
                        .Where(t =>
                            t.IsClass
                            && !t.IsAbstract
                            && t.GetInterfaces().Contains(typeof(IService))
                            && t.GetCustomAttribute<AutoRegisterServiceAttribute>() != null
                        );

                    foreach (var serviceType in serviceTypes)
                    {
                        // 创建服务实例并注册
                        var serviceInstance = Activator.CreateInstance(serviceType);
                        ServiceContainer.Instance.Register((IService)serviceInstance);

                        Debug.Log($"[DI] 已自动注册服务: {serviceType.Name}");
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载类型的程序集
                }
            }

            Debug.Log($"[GameBootstrap] 服务自动注册完成。");
        }

        #endregion

        #region 启动后回调

        private void OnGameStarted()
        {
            LogService.Info($"[GameBootstrap] 进入游戏主流程...", Name);
            //EventService.Publish(EventBuilder.Create<GameStartEvent>().WithSender(this).Build());
        }

        private void HandleStartupError(Exception e)
        {
            LogService.Error($"[GameBootstrap] 启动错误处理。{e.StackTrace}", Name);
        }

        #endregion
    }
}