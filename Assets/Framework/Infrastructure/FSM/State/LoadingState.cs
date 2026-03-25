using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class LoadingState : StateBase
    {
        public LoadingState(ILogService logService, IEventService eventService) : base(logService, eventService)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            LogService.Info("开始加载游戏。。。");

            // 加载配置文件

            // 加载UI

            // 加载预制件

            // 检查更新

            // 读取存档
        }
    }
}