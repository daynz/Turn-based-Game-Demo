using System;
using BH.Framework.Events;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Interfaces;
using BH.GameSystems.BattleSystem.Config;
using BH.GameSystems.BattleSystem.Events;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace BH.GameSystems.BattleSystem.Service
{
    public class BattleService : IInitializable, IDisposable, ISubscribeEvents
    {
        [Inject] private ILogService _logService;
        [Inject] private IEventService _eventService;

        [Inject] private UnitService _unitService;
        private BuffService _buffService;
        [Inject] private TurnService _turnService;
        private SkillService _skillService;
        private SkillPointService _skillPointService;
        private ActionQueueService _actionQueueService;
        private DamageCalculatorService _damageCalculatorService;

        private BattleDataService _battleDataService;

        /// <summary>
        /// 是否战斗进行中
        /// </summary>
        private bool _isBattleActive;

        private bool _isBattleMode;
        private int _resourceLoadedCount = 3;

        /// <summary>
        /// 战斗配置
        /// </summary>
        private BattleConfig _battleConfig;

        public void Initialize()
        {
            _logService.Info("开始初始化");

            _isBattleActive = false;

            SubscribeEvents();

            _eventService.Publish(EventBuilder.CreateForEmptyData<BattleLoadResourceEvent>()
                .WithSender(this)
                .Build()
            );

            _logService.Info("初始化完成");
        }

        public void SubscribeEvents()
        {
            _eventService.Subscribe<GameModeTurnBattleEvent>(OnChangedTurnBattle);
            _eventService.Subscribe<BattleLoadResourceEvent>(OnLoadResource);
            _eventService.Subscribe<BattleLoadResourceCompletedEvent>(OnLoadResourceCompleted);
            _eventService.Subscribe<BattleStartEvent>(OnBattleStart);
        }

        private void OnChangedTurnBattle(GameModeTurnBattleEvent obj)
        {
            _isBattleMode = true;
        }

        private void OnLoadResource(BattleLoadResourceEvent @event)
        {
            //_battleConfig = (BattleConfig)_battleDataService.ConfigsCache["BattleConfig"];

            _eventService.Publish(EventBuilder.CreateForEmptyData<BattleLoadResourceCompletedEvent>()
                .WithSender(this)
                .Build()
            );
        }

        private void OnLoadResourceCompleted(BattleLoadResourceCompletedEvent @event)
        {
            --_resourceLoadedCount;
            if (_resourceLoadedCount != 0 || !_isBattleMode) return;
            _isBattleActive = true;
            _logService.Info("资源加载完毕");
            _eventService.Publish(EventBuilder.CreateForEmptyData<BattleStartEvent>()
                .WithSender(this)
                .Build()
            );
        }

        private void OnBattleStart(BattleStartEvent @event)
        {
            _logService.Info("战斗开始");
            _logService.Info($"{_unitService == null}");
            _logService.Info($"{_turnService == null}");

            _eventService.Publish(EventBuilder.CreateForEmptyData<TurnStartEvent>()
                .WithSender(this)
                .Build()
            );
        }

        public void Dispose()
        {
        }
    }
}