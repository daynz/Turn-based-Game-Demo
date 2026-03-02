namespace BH.Framework.Infrastructure.FSM.Interfaces
{
    /// <summary>
    /// 有限状态机 - 状态机接口
    /// </summary>
    public interface IStateMachine<TStateType>
    {
        /// <summary>
        /// 当前激活的状态
        /// </summary>
        IState CurrentState { get; }

        /// <summary>
        /// 当前状态的ID
        /// </summary>
        TStateType CurrentStateType { get; }

        /// <summary>
        /// 切换到指定ID的新状态
        /// </summary>
        /// <param name="newStateType">新状态的ID</param>
        /// <param name="param">传递给新状态的参数</param>
        void ChangeState(TStateType newStateType, object param = null);

        /// <summary>
        /// 获取一个已注册的状态实例
        /// </summary>
        /// <param name="stateType">要获取的状态ID</param>
        /// <returns>对应的状态实例，如果不存在则返回null</returns>
        IState GetState(TStateType stateType);
    }
}