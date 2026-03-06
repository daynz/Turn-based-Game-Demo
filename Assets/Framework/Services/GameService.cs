using System;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Interfaces;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace BH.Framework.Services
{
    public class GameService : IInitializable, IDisposable
    {
        [Inject] private IResourceService _resourceService;
        [Inject] private ILogService _logService;
        [Inject] private IAddressableService _addressableService;
        [Inject] private IEventService _eventService;
        
        public async void Initialize()
        {
            try
            {
                _logService.Info("开始启动游戏");
                // 加载 Addressable 配置（依赖基础日志）
                // TODO: 修改加载位置
                var logConfig = await _resourceService.LoadJsonConfigAsync<LogConfig>("DefaultLogConfig");
                (_logService as LogService)?.UpdateConfig(logConfig);

                // 启动其他游戏服务
                await StartOtherGameServicesAsync();

                _logService.Info("游戏所有服务启动完成，进入运行阶段");
            }
            catch (Exception e)
            {
                _logService.Info($"游戏启动失败：{e.Message}");
            }
        }

        // todo: 异步加载
        private Task StartOtherGameServicesAsync()
        {
            try
            {
                _logService.Info("开始启动其他游戏服务...");

                // 示例：启动网络服务（此时已用完整日志）
                //var networkService = _container.Resolve<INetworkService>();
                //await networkService.ConnectAsync();

                // 示例：启动存档服务
                //var saveService = _container.Resolve<ISaveService>();
                //saveService.LoadSaveData();

                _logService.Info("所有游戏服务启动完成");
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }

        public void Dispose()
        {
            _logService.Info("GameLifecycleManager 释放资源");
            _logService?.Dispose(); // 若 LogService 有释放逻辑
        }
    }
}