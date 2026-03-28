using System;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data;
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
        [Inject] private ISceneService _sceneService;

        #region 生命周期

        public void Initialize()
        {
            try
            {
                if (_logService == null)
                {
                    Debug.LogWarning("[GameService] LogService 注入失败");
                }

                if (_eventService == null)
                {
                    _logService?.Warning("[GameService] EventService 注入失败");
                }

                if (_addressableService == null)
                {
                    _logService?.Warning("[GameService] AddressableService 注入失败");
                }

                if (_resourceService == null)
                {
                    _logService?.Warning("[GameService] ResourceService 注入失败");
                }

                Enable();

                _eventService?.Publish(EventBuilder.CreateForEmptyData<ResourcePreLoadEvent>()
                    .WithSender(this)
                    .Build()
                );
                _logService?.Info("[GameService] 启动游戏");
            }
            catch (Exception)
            {
                _logService?.Error("[GameService] 游戏启动失败");
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
            _logService.Info("[GameService] 释放资源");
            _logService?.Dispose();
        }
        
        #endregion

        #region 事件

        private void SubscribeEvents()
        {
            //TODO: 修改EventService以支持取消订阅
            _eventService.Subscribe<GamePreloadCompleteEvent>(OnResourcePreLoadComplete);
        }

        private void UnsubscribeEvents()
        {
            //_eventService.Unsubscribe();
        }
        
        private void OnResourcePreLoadComplete(GamePreloadCompleteEvent @event)
        {
            GameStart();
        }

        #endregion

        private void GameStart()
        {
            _logService.Info("[GameService] 游戏开始");
           _sceneService.LoadAndActivateSceneAsync(AssetKeys.AddressableNames.UIScene);
        }
    }
}