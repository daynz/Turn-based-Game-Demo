using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class LoadingState : StateBase
    {
        public override string Name => GetType().Name;
        [Inject] private LogService _logService;

        public override void Enter(IState prevState, object param)
        {
            base.Enter(prevState, param);
            _logService.Info("开始加载游戏。。。", Name);

            // 加载配置文件

            // 加载UI

            // 加载预制件

            // 检查更新

            // 读取存档
        }
    }
}