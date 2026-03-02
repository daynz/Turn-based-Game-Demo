namespace BH.Framework.Infrastructure.FSM.Interfaces
{
    /// <summary>
    /// 有限状态机 - 状态接口
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 进入此状态时调用
        /// </summary>
        /// <param name="prevState">上一个状态</param>
        /// <param name="param">传入的参数</param>
        void Enter(IState prevState, object param);

        /// <summary>
        /// 离开此状态时调用
        /// </summary>
        /// <param name="nextState">下一个状态</param>
        void Exit(IState nextState);

        /// <summary>
        /// 状态更新时调用 (通常在MonoBehaviour的Update中)
        /// </summary>
        void Update();

        /// <summary>
        /// 状态固定更新时调用 (通常在MonoBehaviour的FixedUpdate中)
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 状态物理更新时调用 (通常在MonoBehaviour的LateUpdate中)
        /// </summary>
        void LateUpdate();
    }
}