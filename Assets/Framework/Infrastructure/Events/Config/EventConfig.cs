using System.Collections.Generic;
using BH.Framework.Enums;
using UnityEngine;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Config
{
    /// <summary>
    /// 事件系统配置
    /// </summary>
    [CreateAssetMenu(fileName = "EventConfig", menuName = "Honkai/Event System/Config")]
    public class EventConfig : ScriptableObject
    {
        [Header("事件系统设置")] [Tooltip("启用事件系统")] public bool enableEventSystem = true;

        [Tooltip("启用事件日志")] public bool enableLogging;

        [Tooltip("启用性能分析")] public bool enableProfiling = false;

        [Tooltip("记录事件历史")] public bool recordEventHistory = true;

        [Tooltip("每帧最大处理事件数")] [Range(10, 1000)]
        public int maxEventsPerFrame = 100;

        [Tooltip("事件处理间隔（秒）")] [Range(0.01f, 1.0f)]
        public float eventProcessInterval = 0.1f;

        [Header("事件通道配置")] [SerializeField] public List<EventChannelConfig> channelConfigs = new()
        {
            new EventChannelConfig
                { channelType = EventType.CriticalEvent, priority = EventPriority.Critical, maxQueueSize = 100 },
            new EventChannelConfig
                { channelType = EventType.BattleEvent, priority = EventPriority.High, maxQueueSize = 100 },
            new EventChannelConfig
                { channelType = EventType.UIEvent, priority = EventPriority.Normal, maxQueueSize = 100 },
            new EventChannelConfig
                { channelType = EventType.GameplayEvent, priority = EventPriority.Normal, maxQueueSize = 100 },
            new EventChannelConfig
                { channelType = EventType.SystemEvent, priority = EventPriority.Low, maxQueueSize = 100 },
            new EventChannelConfig
                { channelType = EventType.DebugEvent, priority = EventPriority.Background, maxQueueSize = 100 }
        };

        [Header("性能阈值")] [Tooltip("事件处理时间警告阈值（毫秒）")] [SerializeField]
        public float processingTimeWarningThreshold = 10f;

        [Tooltip("高频事件警告阈值（次/秒）")] [SerializeField]
        public int highFrequencyWarningThreshold = 100;

        [Header("调试设置")] public bool autoPrintStatsOnError = true;
        [SerializeField] public int maxEventHistorySize = 1000;
    }
}