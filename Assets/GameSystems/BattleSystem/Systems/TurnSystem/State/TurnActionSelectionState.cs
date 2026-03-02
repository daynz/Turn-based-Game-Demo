using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Manager;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnActionSelectionState : BaseTurnState
    {
        public string Name => GetType().Name;
        [Inject] private LogService _logService;
        public override void Enter()
        {
            _logService.Info("[TurnPreparation] 等待行动选择",Name);

            // 触发UI显示行动选项
            //EventManager.Instance.Publish();
        }

        public override void HandleInput()
        {
            // 根据输入生成行动数据

            // 检查行动是否可执行

            // 可执行则切换状态，并将操作数据传递给TurnManager
            //TurnManager.Instance.StateMachine.ChangeState(TurnPhase.ActionExecution);

            // 不可执行则拒绝
        }
    }
}