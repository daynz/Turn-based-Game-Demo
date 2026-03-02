using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnActionExecutionState : BaseTurnState
    {
        public string Name => GetType().Name;
        [Inject] private LogService _logService;
        public override void Enter()
        {
            _logService.Info($"[TurnActionExecution] 执行行动",Name);
            
            // 事件发送操作包给各系统
        }
    }
    
}