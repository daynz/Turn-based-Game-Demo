using BH.Framework.Singleton;

namespace BH.GameSystems.BattleSystem.Manager
{
    /// <summary>
    /// 战斗管理器
    /// </summary>
    public class BattleManager : MonoSingleton<BattleManager>
    {
        // #region 字段声明
        //
        // /// <summary>
        // /// 单位系统
        // /// </summary>
        // [SerializeField] private UnitManager unitManager;
        //
        // /// <summary>
        // /// 行动队列系统
        // /// </summary>
        // [SerializeField] private ActionQueue actionQueue;
        //
        // /// <summary>
        // /// 回合系统
        // /// </summary>
        // [SerializeField] private TurnManager turnManager;
        //
        // /// <summary>
        // /// 伤害计算器
        // /// </summary>
        // [SerializeField] private DamageCalculatorManager damageCalculator;
        //
        // /// <summary>
        // /// buff系统
        // /// </summary>
        // [SerializeField] private BuffManager buffManager;
        //
        // /// <summary>
        // /// 技能系统
        // /// </summary>
        // [SerializeField] private SkillManager skillManager;
        //
        // /// <summary>
        // /// 战技点系统
        // /// </summary>
        // [SerializeField] private SkillPointManager skillPointManager;
        //
        // /// <summary>
        // /// 当前战斗状态
        // /// </summary>
        // [SerializeField] private BattleState currentState;
        //
        // /// <summary>
        // /// 战斗配置
        // /// </summary>
        // [SerializeField] private BattleConfig battleConfig;
        //
        // /// <summary>
        // /// 行动方
        // /// </summary>
        // [SerializeField] private UnitFaction actionFaction;
        //
        // /// <summary>
        // /// 是否战斗进行中
        // /// </summary>
        // [SerializeField] private bool isBattleActive;
        //
        // [Inject] private LogService _logService;
        // [Inject] private EventService _eventService;
        //
        // /// <summary>
        // /// 事件处理器
        // /// </summary>
        // private readonly BattleSystemEventHandler _eventHandler = new();
        //
        // #endregion
        //
        // #region 属性
        //
        // /// <summary>
        // /// 获取或设置行动队列系统
        // /// </summary>
        // public ActionQueue ActionQueue
        // {
        //     get => actionQueue;
        //     set => actionQueue = value;
        // }
        //
        // /// <summary>
        // /// 获取或设置回合管理系统
        // /// </summary>
        // public TurnManager TurnManager
        // {
        //     get => turnManager;
        //     set => turnManager = value;
        // }
        //
        // /// <summary>
        // /// 获取或设置伤害计算器
        // /// </summary>
        // public DamageCalculatorManager DamageCalculator
        // {
        //     get => damageCalculator;
        //     set => damageCalculator = value;
        // }
        //
        // /// <summary>
        // /// 获取或设置buff管理系统
        // /// </summary>
        // public BuffManager BuffManager
        // {
        //     get => buffManager;
        //     set => buffManager = value;
        // }
        //
        // /// <summary>
        // /// 获取或设置技能管理系统
        // /// </summary>
        // public SkillManager SkillManager
        // {
        //     get => skillManager;
        //     set => skillManager = value;
        // }
        //
        // #endregion
        //
        // #region 初始化
        //
        // /// <summary>
        // /// 单例初始化
        // /// </summary>
        // public override void Initialize()
        // {
        //     //CurrentState = BattleState.NotStarted;
        // }
        //
        // /// <summary>
        // /// 初始化战斗
        // /// </summary>
        // /// <param name="config">战斗配置</param>
        // private void Init(BattleConfig config)
        // {
        //     if (isBattleActive)
        //     {
        //         _logService.Warning("[BattleManager] 战斗正在进行中，无法重新初始化");
        //         return;
        //     }
        //
        //     battleConfig = config;
        //     LoadBattleConfig(battleConfig);
        //
        //     // 初始化单位管理器
        //     unitManager.Init(battleConfig);
        //
        //     // 初始化行动队列
        //     ActionQueue.Init(battleConfig);
        //
        //     // 初始化回合系统
        //     TurnManager.Init(battleConfig);
        //
        //     // 初始化伤害计算器
        //     DamageCalculator.Init(battleConfig);
        //
        //     // 初始化buff管理器
        //     BuffManager.Init(battleConfig);
        //
        //     // 初始化技能管理器
        //     SkillManager.Init(battleConfig);
        //
        //     // 注册事件监听
        //     RegisterEventListener();
        //
        //     // 设置战斗状态
        //     //CurrentState = BattleState.Preparation;
        //     isBattleActive = true;
        //
        //     _eventService.Publish(
        //         EventBuilder.Create<BattleStartEvent>()
        //             .WithSender(this)
        //             .Build());
        //     _logService.Info("[BattleManager] 战斗初始化完成");
        // }
        //
        // /// <summary>
        // /// 加载战斗配置
        // /// </summary>
        // /// <param name="config">战斗配置对象</param>
        // private void LoadBattleConfig(BattleConfig config)
        // {
        // }
        //
        // /// <summary>
        // /// 注册事件监听器
        // /// </summary>
        // private void RegisterEventListener()
        // {
        //     _eventHandler.Subscript<BattleStartEvent>(StartBattle);
        // }
        //
        // #endregion
        //
        // #region 战斗流程控制
        //
        // /// <summary>
        // /// 开始战斗
        // /// </summary>
        // /// <param name="eventData">战斗开始事件数据</param>
        // private void StartBattle(BattleStartEvent eventData)
        // {
        //     if (!_eventService.IsInitialized)
        //     {
        //         _logService.Error("[BattleManager] EventManager 不可用，无法处理战斗开始事件");
        //         return;
        //     }
        //
        //     // 如果战斗已经开始或未激活，直接返回
        //     if (!isBattleActive || currentState != BattleState.Preparation)
        //     {
        //         _logService.Warning(
        //             $"[BattleManager] 战斗无法开始：isBattleActive={isBattleActive}, CurrentState={currentState}");
        //         return;
        //     }
        //
        //     // 保存事务开始前的状态
        //     var oldState = currentState;
        //
        //     try
        //     {
        //         //CurrentState = BattleState.InProgress;
        //         _logService.Info("[BattleManager] 战斗开始，状态已切换为 InProgress");
        //
        //         _eventService.Publish(
        //             EventBuilder.Create<TurnStartEvent>()
        //                 .WithSender(this)
        //                 .Build()
        //         );
        //
        //         _logService.Info("战斗开始事件处理成功");
        //     }
        //     catch (Exception ex)
        //     {
        //         // 发生异常时回滚状态，确保数据一致性
        //         currentState = oldState; // 直接回滚到旧状态（跳过属性避免再次记录）
        //         _logService.Error($"[BattleManager] 战斗开始事件处理失败，状态已回滚到 {oldState}。异常详情：{ex}");
        //
        //         try
        //         {
        //             // TODO: 发布一个失败事件，通知其他模块进行处理
        //             //EventManager.Instance?.Publish(new BattleStartFailedEvent(this, ex.Message));
        //         }
        //         catch
        //         {
        //             // 忽略二次异常，仅记录
        //             _logService.Error("[BattleManager] 发布战斗开始失败事件时发生异常，请检查事件系统");
        //         }
        //     }
        // }
        //
        // #endregion
        //
        // #region 战斗生命周期管理
        //
        // /// <summary>
        // /// 暂停战斗
        // /// 暂停战斗进程并冻结时间流速
        // /// </summary>
        // public void PauseBattle()
        // {
        //     // if (CurrentState != BattleState.InProgress) return;
        //     //
        //     // CurrentState = BattleState.Paused;
        //
        //     // 暂停所有战斗相关系统
        //     Time.timeScale = 0f;
        //
        //     //BattleEventSystem.Instance.Trigger(new BattlePausedEvent());
        //     _logService.Info("[BattleManager] 战斗已暂停");
        // }
        //
        // /// <summary>
        // /// 恢复战斗
        // /// 恢复战斗进程并恢复时间流速
        // /// </summary>
        // public void ResumeBattle()
        // {
        //     // if (CurrentState != BattleState.Paused) return;
        //     //
        //     // CurrentState = BattleState.InProgress;
        //
        //     // 恢复时间流逝
        //     Time.timeScale = 1f;
        //     //BattleEventSystem.Instance.Trigger(new BattleResumedEvent());
        //
        //     _logService.Info("[BattleManager] 战斗已恢复");
        // }
        //
        // /// <summary>
        // /// 结束战斗
        // /// 计算战斗结果并清理战斗状态
        // /// </summary>
        // public void EndBattle()
        // {
        //     // if (CurrentState == BattleState.Ended) return;
        //     //
        //     // CurrentState = BattleState.Ended;
        //     isBattleActive = false;
        //
        //     // todo: 计算战斗结果
        //
        //
        //     // 清理战斗状态
        //     Cleanup();
        //
        //     // Debug.Log($"战斗结束 - 结果: {result.Victor}");
        //
        //     // // todo触发战斗结束事件
        //     // BattleEventSystem.Instance.Trigger(new BattleEndedEvent
        //     // {
        //     //     Result = result,
        //     //     BattleDuration = _battleTimer
        //     // });
        // }
        //
        // /// <summary>
        // /// 强制结束战斗
        // /// 用于测试或特殊情况下的战斗结束
        // /// </summary>
        // /// <param name="victor">胜利方</param>
        // private void ForceEndBattle(BattleVictor victor)
        // {
        //     _logService.Info("[BattleManager] 战斗强制结束 - 胜利方: {victor}");
        // }
        //
        // /// <summary>
        // /// 清理战斗状态
        // /// 重置战斗相关数据和状态
        // /// </summary>
        // private void Cleanup()
        // {
        //     // 清理子系统
        //     _logService.Info("[BattleManager] 战斗清理完成");
        // }
        //
        // #endregion
        //
        // #region 公共接口
        //
        // /// <summary>
        // /// 检查战斗是否活跃
        // /// </summary>
        // /// <returns>战斗是否进行中</returns>
        // public bool IsBattleActive()
        // {
        //     return isBattleActive;
        // }
        //
        // /// <summary>
        // /// 获取战斗配置
        // /// </summary>
        // /// <returns>当前战斗配置</returns>
        // public BattleConfig GetBattleConfig()
        // {
        //     return battleConfig;
        // }
        //
        // /// <summary>
        // /// 重新开始战斗
        // /// 使用当前配置和角色列表重新初始化战斗
        // /// </summary>
        // public void RestartBattle()
        // {
        //     // 重新初始化战斗
        //     Init(battleConfig);
        // }
        //
        // /// <summary>
        // /// 设置战斗速度
        // /// 调整游戏时间流速以改变战斗播放速度
        // /// </summary>
        // /// <param name="speed">战斗速度倍率</param>
        // public void SetBattleSpeed(float speed)
        // {
        //     if (speed <= 0) return;
        //
        //     Time.timeScale = speed;
        //
        //     _logService.Info("[BattleManager] 战斗速度设置为: {speed}x");
        //
        //     // BattleEventSystem.Instance.Trigger(new BattleSpeedChangedEvent
        //     // {
        //     //     Speed = speed
        //     // });
        // }
        //
        // #endregion
        //
        // #region 测试方法
        //
        // /// <summary>
        // /// 手动开始战斗（用于测试）
        // /// 在Unity编辑器中右键调用此方法来快速启动战斗
        // /// </summary>
        // [ContextMenu("手动开始战斗")]
        // public void ManualStartBattle()
        // {
        //     Init(battleConfig);
        // }
        //
        // /// <summary>
        // /// 手动结束战斗（用于测试）
        // /// 在Unity编辑器中右键调用此方法来快速结束战斗（玩家胜利）
        // /// </summary>
        // [ContextMenu("手动结束战斗（玩家胜利）")]
        // public void ManualEndBattlePlayerWin()
        // {
        //     if (!isBattleActive) return;
        //
        //     ForceEndBattle(BattleVictor.Player);
        // }
        //
        // /// <summary>
        // /// 手动结束战斗（用于测试）
        // /// 在Unity编辑器中右键调用此方法来快速结束战斗（敌人胜利）
        // /// </summary>
        // [ContextMenu("手动结束战斗（敌人胜利）")]
        // public void ManualEndBattleEnemyWin()
        // {
        //     if (!isBattleActive) return;
        //
        //     ForceEndBattle(BattleVictor.Enemy);
        // }
        //
        // #endregion
        //
        //
        // #region 内部类型定义
        //
        // /// <summary>
        // /// 战斗系统事件处理器
        // /// 专门处理战斗相关的事件
        // /// </summary>
        // private class BattleSystemEventHandler : EventHandlerBase
        // {
        //     public override EventType[] ListenEventTypes => new[] { EventType.BattleEvent };
        // }
        //
        // #endregion
    }
}