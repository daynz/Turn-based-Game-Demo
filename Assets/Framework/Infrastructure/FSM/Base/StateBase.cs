using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.Framework.Infrastructure.FSM.Base
{
    /// <summary>
    /// 有限状态机 - 抽象状态基类
    /// </summary>
    public abstract class StateBase : IState
    {
        protected string StateName => GetType().Name;

        protected readonly IEventService EventService;
        protected readonly ILogService LogService;

        public StateBase(ILogService logService, IEventService eventService)
        {
            LogService = logService;
            EventService = eventService;
        }

        public virtual void Enter()
        {
            LogService.Info($"进入状态: {StateName}");
        }

        public virtual void Exit()
        {
            LogService.Info($"离开状态: {StateName}");
        }

        public virtual void Update()
        {
            // 子类重写此方法以实现每帧逻辑
        }

        public virtual void FixedUpdate()
        {
            // 子类重写此方法以实现固定频率物理更新逻辑
        }

        public virtual void LateUpdate()
        {
            // 子类重写此方法以实现延迟更新逻辑
        }
    }
}