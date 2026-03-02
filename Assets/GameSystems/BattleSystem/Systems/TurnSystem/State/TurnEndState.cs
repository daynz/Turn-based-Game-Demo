using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Manager;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnEndState : BaseTurnState
    {
        public string Name => GetType().Name;
        [Inject] private LogService _logService;

        public override void Enter()
        {
            _logService.Info($"[TurnEnd] 回合结束处理", Name);

            // TODO:事件：回合结束

            //TurnManager.Instance.StateMachine.ChangeState(TurnPhase.Completed);
        }
    }
}