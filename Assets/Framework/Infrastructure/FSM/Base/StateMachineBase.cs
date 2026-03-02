using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.FSM.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.FSM.Base
{
    /// <summary>
    /// 有限状态机 - 抽象状态机基类
    /// </summary>
    /// <typeparam name="TStateType">状态的类型</typeparam>
    [Serializable]
    public abstract class StateMachineBase<TStateType> : IStateMachine<TStateType>
    {
        #region Properties and Fields

        public IState CurrentState { get; private set; }
        public TStateType CurrentStateType { get; private set; }

        private readonly Dictionary<TStateType, IState> _states = new();
        private IState _previousState = null;
        [SerializeField] protected TStateType initialState;

        #endregion

        #region Public Methods

        /// <summary>
        /// 注册一个状态实例到此状态机
        /// </summary>
        /// <param name="stateType">状态ID</param>
        /// <param name="state">状态实例</param>
        protected void RegisterState(TStateType stateType, IState state)
        {
            if (_states.ContainsKey(stateType))
            {
                Debug.LogWarning($"[FSM] 状态ID {stateType} 已存在，将被覆盖。");
            }

            _states[stateType] = state;
        }

        /// <summary>
        /// 注销一个状态
        /// </summary>
        /// <param name="stateType">要注销的状态ID</param>
        public void UnregisterState(TStateType stateType)
        {
            _states.Remove(stateType);
            if (CurrentStateType.Equals(stateType))
            {
                CurrentState = null;
                CurrentStateType = default;
            }
        }

        public virtual void ChangeState(TStateType newStateType, object param = null)
        {
            if (!_states.TryGetValue(newStateType, out var newState))
            {
                Debug.LogError($"[FSM] 尝试切换到未注册的状态: {newStateType}");
                return;
            }

            CurrentState?.Exit(newState);

            _previousState = CurrentState;
            var oldState = CurrentState;

            CurrentState = newState;
            CurrentStateType = newStateType;

            newState.Enter(oldState, param);
        }

        public IState GetState(TStateType stateType)
        {
            _states.TryGetValue(stateType, out var state);
            return state;
        }

        #endregion

        #region Lifecycle Methods (To be called from MonoBehaviour)

        /// <summary>
        /// 在MonoBehaviour的Update中调用
        /// </summary>
        public virtual void OnUpdate()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// 在MonoBehaviour的FixedUpdate中调用
        /// </summary>
        public virtual void OnFixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        /// <summary>
        /// 在MonoBehaviour的LateUpdate中调用
        /// </summary>
        public virtual void OnLateUpdate()
        {
            CurrentState?.LateUpdate();
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// 清理状态机，释放所有状态的引用
        /// </summary>
        public virtual void ClearStates()
        {
            _states.Clear();
            CurrentState = null;
            CurrentStateType = default;
            _previousState = null;
        }

        #endregion
    }
}