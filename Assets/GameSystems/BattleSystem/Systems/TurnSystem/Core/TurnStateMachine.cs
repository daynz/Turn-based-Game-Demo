using System;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.State;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.Core
{
    [Serializable]
    public class TurnStateMachine : StateMachineBase<TurnStateType>
    {
        [SerializeField] private TurnStateType currentPhase;

        public TurnStateMachine(ILogService logService, IEventService eventService)
            : base(logService, eventService)
        {
            RegisterState(TurnStateType.Preparation, new TurnPreparationState(logService, eventService));
            RegisterState( TurnStateType.ActionSelection, new TurnActionSelectionState(logService, eventService));
            RegisterState(TurnStateType.ActionExecution, new TurnActionExecutionState(logService, eventService));
            RegisterState(TurnStateType.End, new TurnEndState(logService, eventService));
            RegisterState(TurnStateType.Completed, new TurnCompletedState(logService, eventService));
            
            currentPhase = TurnStateType.NotStarted;
            _currentState = States[currentPhase];
        }

        public void ChangeState(TurnStateType newPhase)
        {
            if (!CanChangeState(newPhase))
            {
                _logService.Warning($"非法阶段切换: {currentPhase} -> {newPhase}");
                return;
            }

            _currentState?.Exit();
            _currentState = States[newPhase];
            _currentState?.Enter();
        }

        /// <summary>
        /// 检查状态切换合法性
        /// </summary>
        protected override bool CanChangeState(TurnStateType newStateType)
        {
            // 定义允许的转换
            return currentPhase switch
            {
                TurnStateType.NotStarted => newStateType == TurnStateType.Preparation,
                TurnStateType.Preparation => newStateType is TurnStateType.ActionSelection or TurnStateType.End,
                TurnStateType.ActionSelection => newStateType is TurnStateType.ActionExecution or TurnStateType.End,
                TurnStateType.ActionExecution => newStateType is TurnStateType.ActionSelection or TurnStateType.End,
                TurnStateType.End => newStateType == TurnStateType.Completed,
                //TurnPhase.Completed => false,
                _ => false
            };
        }
    }
}