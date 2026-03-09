using System;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Interfaces;
using UnityEngine;
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
                // var logConfig = await _resourceService.LoadJsonConfigAsync<LogConfig>("DefaultLogConfig");
                // (_logService as LogService)?.UpdateConfig(logConfig);
                
                SubscribeEvents();

                _eventService.Publish(EventBuilder.Create<GameStartEvent, EmptyEventData>()
                    .WithSender(this)
                    .Build()
                );
            }
            catch (Exception e)
            {
                _logService.Info($"游戏启动失败：{e.Message}");
            }
        }

        private void SubscribeEvents()
        {
            _eventService.Subscribe<GameStartEvent>(GameStart);
        }

        private async void GameStart(GameStartEvent @event)
        {
            try
            {
                await StartOtherGameServicesAsync();
            }
            catch (Exception e)
            {
                _logService.Error("启动游戏失败");
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

        public void TurnBattleStart()
        {
            _eventService.Publish(EventBuilder.CreateForEmptyData<BattleStartEvent>().WithSender(this).Build());
        }

        public void Dispose()
        {
            _logService.Info("释放资源");
            _logService?.Dispose();
        }
    }
}