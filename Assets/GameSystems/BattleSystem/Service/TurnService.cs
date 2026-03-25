using System;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Interfaces;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Events;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Core;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace BH.GameSystems.BattleSystem.Service
{
    public class TurnService : IInitializable, IDisposable, ISubscribeEvents
    {
        [Inject] private ILogService _logService;
        [Inject] private IEventService _eventService;

        private TurnContext _context;
        private TurnStateMachine _stateMachine;

        public void Initialize()
        {
            _context = new TurnContext();
            _stateMachine = new TurnStateMachine(_logService, _eventService);
        }

        public void SubscribeEvents()
        {
            _eventService.Subscribe<BattleLoadResourceEvent>(OnLoadResource);
            _eventService.Subscribe<TurnStartEvent>(OnBattleStart);
        }

        private void OnLoadResource(BattleLoadResourceEvent @event)
        {
            _eventService.Publish(EventBuilder.CreateForEmptyData<BattleLoadResourceCompletedEvent>()
                .WithSender(this)
                .Build()
            );
        }

        private void OnBattleStart(TurnStartEvent @event)
        {
            _stateMachine.ChangeState(TurnStateType.Preparation);
        }

        public void Dispose()
        {
        }
    }
}