using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Singleton;
using BH.GameSystems.BattleSystem.Config;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Events.EventTurn;
using BH.GameSystems.BattleSystem.Systems.ActionQueueSystem;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Core;
using UnityEngine;
using EventType = BH.Framework.Enums.EventType;

namespace BH.GameSystems.BattleSystem.Manager
{
    /// <summary>
    /// 回合系统
    /// </summary>
    [Serializable]
    public class TurnManager : MonoSingleton<TurnManager>
    {
        // [SerializeField] private TurnData currentTurn;
        // [SerializeField] private TurnContext turnContext = new();
        //
        // /// <summary>
        // /// 回合历史
        // /// </summary>
        // [SerializeField] private List<TurnData> turnHistory = new();
        //
        // /// <summary>
        // /// 是否回合进行中
        // /// </summary>
        // [SerializeField] private bool isTurnInProgress;
        //
        // [SerializeField] private TurnStateMachine stateMachine = new();
        //
        // private readonly TurnSystemEventHandler _eventHandler = new();
        // [Inject] private LogService _logService;
        // [Inject] private EventService _eventService;
        //
        // // 挂起操作计数器
        // private int _pendingOperations = 0;
        //
        // // 是否已请求结束回合
        // private bool _endRequested = false;
        //
        // public TurnStateMachine StateMachine => stateMachine;
        //
        // public TurnData CurrentTurn
        // {
        //     get => currentTurn;
        //     set => currentTurn = value;
        // }
        //
        // public TurnContext TurnContext
        // {
        //     get => turnContext;
        //     set => turnContext = value;
        // }
        //
        // public List<TurnData> TurnHistory => turnHistory;
        //
        //
        // public override void Initialize()
        // {
        // }
        //
        // /// <summary>
        // /// 初始化回合系统
        // /// </summary>
        // public void Init(BattleConfig config)
        // {
        //     CurrentTurn = null;
        //     turnHistory.Clear();
        //     isTurnInProgress = false;
        //     RegisterEventListener();
        //
        //     _logService.Info("回合系统初始化完成");
        // }
        //
        // /// <summary>
        // /// 注册事件监听器
        // /// </summary>
        // private void RegisterEventListener()
        // {
        //     _eventHandler.AddSubscription<TurnStartEvent>(TurnStart);
        // }
        //
        // private void TurnStart(TurnStartEvent @event)
        // {
        //     CurrentTurn = new TurnData
        //     {
        //         Unit = ActionQueue.Instance.GetActionUnit().Owner,
        //         TurnNumber = TurnHistory.Count,
        //         IsCompleted = false,
        //     };
        //     TurnHistory.Add(CurrentTurn);
        //
        //     StateMachine.ChangeState(TurnPhase.Preparation);
        // }
        //
        // /// <summary>
        // /// 增加一个挂起操作
        // /// </summary>
        // public void AddPendingOperation()
        // {
        //     _pendingOperations++;
        // }
        //
        // /// <summary>
        // /// 完成一个挂起操作
        // /// </summary>
        // public void OperationCompleted()
        // {
        //     if (_pendingOperations <= 0)
        //     {
        //         _logService.Warning("[TurnManager] OperationCompleted 调用次数过多");
        //         return;
        //     }
        //
        //     _pendingOperations--;
        //
        //     if (_pendingOperations == 0 && _endRequested)
        //     {
        //         StateMachine.ChangeState(TurnPhase.End);
        //     }
        // }
        //
        // /// <summary>
        // /// 请求结束回合
        // /// </summary>
        // public void RequestEndTurn()
        // {
        //     if (_endRequested) return;
        //
        //     _endRequested = true;
        //
        //     if (_pendingOperations == 0)
        //     {
        //         StateMachine.ChangeState(TurnPhase.End);
        //     }
        // }
        //
        // private void TurnEnd()
        // {
        //     TurnContext.Cleanup();
        //     _eventService.Publish(
        //         EventBuilder.Create<TurnStartEvent>()
        //             .WithSender(this)
        //             .Build()
        //     );
        // }
        //
        // /// <summary>
        // /// 获取回合历史
        // /// </summary>
        // public List<TurnData> GetTurnHistory(int maxCount = 10)
        // {
        //     int startIndex = Mathf.Max(0, turnHistory.Count - maxCount);
        //     int count = Mathf.Min(maxCount, turnHistory.Count - startIndex);
        //
        //     return turnHistory.GetRange(startIndex, count);
        // }
        //
        // /// <summary>
        // /// 获取总回合数
        // /// </summary>
        // public int GetTotalTurnCount()
        // {
        //     return turnHistory.Count;
        // }
        //
        // // /// <summary>
        // // /// 获取角色的回合统计数据
        // // /// </summary>
        // // public TurnStats GetCharacterTurnStats(BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> unit)
        // // {
        // //     var stats = new TurnStats
        // //     {
        // //         Unit = unit,
        // //         TotalTurns = 0,
        // //         AverageActionCount = 0f,
        // //         TotalDamageDealt = 0,
        // //         TotalHealingDone = 0
        // //     };
        // //
        // //     if (!unit) return stats;
        // //
        // //     int totalActions = 0;
        // //
        // //     foreach (var turn in turnHistory)
        // //     {
        // //         if (turn.Unit == unit)
        // //         {
        // //             stats.TotalTurns++;
        // //
        // //             // 统计伤害和治疗
        // //         }
        // //     }
        // //
        // //     if (stats.TotalTurns > 0)
        // //     {
        // //         stats.AverageActionCount = (float)totalActions / stats.TotalTurns;
        // //     }
        // //
        // //     return stats;
        // // }
        //
        // public void ClearTurn()
        // {
        //     TurnContext.Cleanup();
        // }
        //
        // /// <summary>
        // /// 清理回合系统
        // /// </summary>
        // public void Cleanup()
        // {
        //     CurrentTurn = null;
        //     _pendingOperations = 0;
        //     _endRequested = false;
        //     turnHistory.Clear();
        // }
        //
        // private class TurnSystemEventHandler : EventHandlerBase
        // {
        //     public override EventType[] ListenEventTypes => new[] { EventType.BattleEvent };
        // }
    }
}