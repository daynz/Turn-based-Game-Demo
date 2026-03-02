using System.Collections.Generic;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Interfaces;
using BH.Framework.Singleton;
using BH.GameSystems.BattleSystem.Config;
using BH.GameSystems.BattleSystem.Events.EventBattle;

namespace BH.GameSystems.BattleSystem.Service
{
    public class BattleService : CSharpSingleton<BattleService>, IGameService
    {
        public string Name => GetType().Name;
        private ILogService _logger;
        private IEventService _events;

        private UnitService _unitService;
        private BuffService _buffService;
        private TurnService _turnService;
        private SkillService _skillService;
        private SkillPointService _skillPointService;
        private ActionQueueService _actionQueueService;
        private DamageCalculatorService _damageCalculatorService;

        private BattleDataService _battleDataService;

        private readonly List<IGameService> _services = new();
        [Inject] private LogService _logService;
        [Inject] private EventService _eventService;

        /// <summary>
        /// 是否战斗进行中
        /// </summary>
        private bool _isBattleActive;

        /// <summary>
        /// 战斗配置
        /// </summary>
        private BattleConfig _battleConfig;

        public override void Initialize()
        {
            _isBattleActive = false;
            base.Initialize();
        }

        public void Init()
        {
            if (_isBattleActive)
            {
                _logger.Warning("[BattleManager] 战斗正在进行中，无法重新初始化",Name);
                return;
            }

            _logger.Info($"[{GetType().Name}] 开始初始化战斗",Name);

            // 获取或创建服务实例
            _unitService = UnitService.Instance;
            _buffService = BuffService.Instance;
            _turnService = TurnService.Instance;
            _skillService = SkillService.Instance;
            _skillPointService = SkillPointService.Instance;
            _actionQueueService = ActionQueueService.Instance;
            _damageCalculatorService = DamageCalculatorService.Instance;
            _battleDataService = BattleDataService.Instance;

            // 构建内部服务列表用于统一初始化
            _services.Clear();
            _services.Add(_unitService);
            _services.Add(_buffService);
            _services.Add(_turnService);
            _services.Add(_skillService);
            _services.Add(_skillPointService);
            _services.Add(_actionQueueService);
            _services.Add(_damageCalculatorService);
            _services.Add(_battleDataService);

            foreach (var service in _services)
            {
                service?.Init();
            }

            _battleConfig = (BattleConfig)_battleDataService.ConfigsCache["BattleConfig"];

            _isBattleActive = true;

            _eventService.Publish(
                EventBuilder.Create<BattleStartEvent>()
                    .WithSender(this)
                    .Build());

            _logger.Info("[BattleService] 初始化完成，战斗已激活。",Name);
        }
    }
}