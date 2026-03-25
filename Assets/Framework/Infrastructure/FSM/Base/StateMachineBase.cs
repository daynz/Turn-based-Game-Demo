using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
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

        protected readonly ILogService _logService;
        protected readonly IEventService _eventService;
        protected IState _currentState;

        public IState CurrentState
        {
            get => _currentState;
            protected set => _currentState = value;
        }

        public TStateType CurrentStateType { get; private set; }

        protected readonly Dictionary<TStateType, IState> States = new();
        protected IState _previousState = null;
        [SerializeField] protected TStateType initialState;

        #endregion

        protected StateMachineBase(ILogService logService, IEventService eventService)
        {
            _logService = logService;
            _eventService = eventService;
        }
        
        #region Public Methods

        /// <summary>
        /// 注册一个状态实例到此状态机
        /// </summary>
        /// <param name="stateType">状态ID</param>
        /// <param name="state">状态实例</param>
        protected void RegisterState(TStateType stateType, IState state)
        {
            if (States.ContainsKey(stateType))
            {
                Debug.LogWarning($"[FSM] 状态ID {stateType} 已存在，将被覆盖。");
            }

            States[stateType] = state;
        }

        /// <summary>
        /// 注销一个状态
        /// </summary>
        /// <param name="stateType">要注销的状态ID</param>
        public void UnregisterState(TStateType stateType)
        {
            States.Remove(stateType);
            if (CurrentStateType.Equals(stateType))
            {
                CurrentState = null;
                CurrentStateType = default;
            }
        }

        public virtual void ChangeState(TStateType newStateType, object param = null)
        {
            if (!CanChangeState(newStateType))
            {
                _logService.Warning($"非法阶段切换: {CurrentState} -> {newStateType}");
                return;
            }

            _currentState?.Exit();
            _currentState = States[newStateType];
            _currentState?.Enter();
        }

        public IState GetState(TStateType stateType)
        {
            States.TryGetValue(stateType, out var state);
            return state;
        }
        
        protected virtual bool CanChangeState(TStateType newStateType)
        {
            // 定义允许的转换
            return true;
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
            States.Clear();
            CurrentState = null;
            CurrentStateType = default;
            _previousState = null;
        }

        #endregion
    }
}