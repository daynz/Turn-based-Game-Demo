using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Base;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.State;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.Core
{
    [Serializable]
    public class TurnStateMachine
    {
        public string Name => GetType().Name;
        [SerializeField] private TurnPhase currentPhase;
        private ITurnState _currentState;
        private Dictionary<TurnPhase, ITurnState> _states;
        public TurnPhase CurrentPhase => currentPhase;
        [Inject] private LogService _logService;

        public TurnStateMachine()
        {
            _states = new Dictionary<TurnPhase, ITurnState>
            {
                { TurnPhase.NotStarted, new BaseTurnState() },
                { TurnPhase.Preparation, new TurnPreparationState() },
                { TurnPhase.ActionSelection, new TurnActionSelectionState() },
                { TurnPhase.ActionExecution, new TurnActionExecutionState() },
                { TurnPhase.End, new TurnEndState() },
                { TurnPhase.Completed, new TurnCompletedState() }
            };
            currentPhase = TurnPhase.NotStarted;
            _currentState = _states[currentPhase];
        }

        public void ChangeState(TurnPhase newPhase)
        {
            if (!CanChangeState(newPhase))
            {
                _logService.Warning($"非法阶段切换: {currentPhase} -> {newPhase}", Name);
                return;
            }

            _currentState?.Exit();
            _currentState = _states[newPhase];
            _currentState?.Enter();
        }

        void Update()
        {
            _currentState?.Update();
        }

        /// <summary>
        /// 检查状态切换合法性
        /// </summary>
        private bool CanChangeState(TurnPhase newPhase)
        {
            // 定义允许的转换
            return currentPhase switch
            {
                TurnPhase.NotStarted => newPhase == TurnPhase.Preparation,
                TurnPhase.Preparation => newPhase is TurnPhase.ActionSelection or TurnPhase.End,
                TurnPhase.ActionSelection => newPhase is TurnPhase.ActionExecution or TurnPhase.End,
                TurnPhase.ActionExecution => newPhase is TurnPhase.ActionSelection or TurnPhase.End,
                TurnPhase.End => newPhase == TurnPhase.Completed,
                //TurnPhase.Completed => false,
                _ => false
            };
        }
    }
}