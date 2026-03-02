using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.FSM.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;

namespace BH.Framework.Infrastructure.FSM.Base
{
    /// <summary>
    /// 有限状态机 - 抽象状态基类
    /// </summary>
    public abstract class StateBase : IState
    {
        public string Name => GetType().Name;
        protected string StateName => GetType().Name;

        [Inject] private readonly EventService _eventManager;
        [Inject] private readonly LogService _logger;

        public virtual void Enter(IState prevState, object param)
        {
            _logger.Info($"进入状态: {StateName} | 上一状态: {(prevState?.GetType().Name ?? "None")}", Name);
        }

        public virtual void Exit(IState nextState)
        {
            _logger.Info($"离开状态: {StateName} | 下一状态: {(nextState?.GetType().Name ?? "None")}", Name);
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