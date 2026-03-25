using System;
using BH.Framework.Events;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data.Events;
using BH.Framework.Infrastructure.Resource.Interfaces;
using BH.Framework.Interfaces;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BH.Framework.Services
{
    [UsedImplicitly]
    public class GameService : IGameService
    {
        [Inject] private ILogService _logService;
        [Inject] private IEventService _eventService;
        [Inject] private IResourceService _resourceService;
        [Inject] private IAddressableService _addressableService;
        //[Inject] private ISceneService _sceneService;

        #region 生命周期

        public void Initialize()
        {
            try
            {
                if (_logService == null)
                {
                    Debug.LogWarning("LogService 注入失败");
                }

                if (_eventService == null)
                {
                    _logService?.Warning("EventService 注入失败");
                }

                if (_addressableService == null)
                {
                    _logService?.Warning("AddressableService 注入失败");
                }

                if (_resourceService == null)
                {
                    _logService?.Warning("ResourceService 注入失败");
                }

                _eventService?.Publish(EventBuilder.CreateForEmptyData<ResourcePreLoadEvent>()
                    .WithSender(this)
                    .Build()
                );
                _logService?.Info("开始启动游戏");
            }
            catch (Exception e)
            {
                _logService?.Info($"游戏启动失败：{e.Message}");
            }
        }

        public void Enable()
        {
            SubscribeEvents();
        }

        public void Disable()
        {
            UnsubscribeEvents();
        }

        public void Dispose()
        {
            _logService.Info("释放资源");
            _logService?.Dispose();
        }

        private void SubscribeEvents()
        {
            //TODO: 修改EventService以支持取消订阅
        }

        private void UnsubscribeEvents()
        {
            //_eventService.Unsubscribe();
        }

        #endregion
    }
}