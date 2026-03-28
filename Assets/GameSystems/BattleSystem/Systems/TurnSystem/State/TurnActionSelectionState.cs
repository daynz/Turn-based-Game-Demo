
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnActionSelectionState : StateBase
    {
        public TurnActionSelectionState(ILogService logService, IEventService eventService) : base(logService,
            eventService)
        {
        }

        public string Name => GetType().Name;
         private LogService _logService;

        public override void Enter()
        {
            _logService.Info("[TurnPreparation] 等待行动选择");

            // 触发UI显示行动选项
            //EventManager.Instance.Publish();
        }
    }
}