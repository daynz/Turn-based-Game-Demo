using System;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.FSM.StateMachine;
using UnityEngine;

namespace BH.Framework.Services
{
    [Serializable]
    [AutoRegisterService]
    public sealed class GameService : ServiceBase
    {
        public string Name => GetType().Name;
        private GameStateMachine _gameStateMachine;
        [SerializeField] private int priority = (int)PriorityOrder.Game;
        private GameEventHandler _eventHandler;

        public override int Priority => priority;

        public override Task InitializeAsync()
        {
            if (!IsInitialized) return Task.CompletedTask;
            Debug.Log("开始初始化...");
            // _gameStateMachine = new GameStateMachine();
            // _gameStateMachine.Init();
            //
            // _eventHandler = new GameEventHandler(EventService);
            Debug.Log($"EventService注入：{EventService}");
            base.InitializeAsync();
            Debug.Log("初始化完成。");
            IsInitialized = true;
            return Task.CompletedTask;
        }

        protected override void RegisterEventListener()
        {
            _eventHandler.Subscribe<GameStartEvent>(OnGameStart);
        }

        private void OnGameStart(GameStartEvent @event)
        {
            LogService.Info("游戏开始", Name);
        }

        public override void Shutdown()
        {
            _eventHandler.Dispose();
            _gameStateMachine.ClearStates();
            LogService.Info($"[{GetType().Name}] 关闭", Name);
        }

        private class GameEventHandler : EventHandlerBase
        {
            public GameEventHandler(EventService eventService) : base(eventService)
            {
            }
        }
    }
}