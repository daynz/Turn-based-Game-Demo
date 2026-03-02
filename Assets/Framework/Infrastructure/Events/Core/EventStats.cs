using System;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件统计信息实体类
    /// 包含订阅数、处理数、性能耗时等统计数据
    /// </summary>
    public class EventStats
    {
        /// <summary>
        /// 当前订阅数量
        /// </summary>
        public int SubscriptionCount { get; set; }
            
        /// <summary>
        /// 总事件发布数量
        /// </summary>
        public int TotalEvents { get; set; }
            
        /// <summary>
        /// 已处理的事件数量
        /// </summary>
        public int HandledEvents { get; set; }
            
        /// <summary>
        /// 总处理耗时（毫秒）
        /// </summary>
        public double TotalProcessingTime { get; set; }
            
        /// <summary>
        /// 平均处理耗时（毫秒）
        /// </summary>
        public double AverageProcessingTime { get; set; }
            
        /// <summary>
        /// 最大处理耗时（毫秒）
        /// </summary>
        public double MaxProcessingTime { get; set; }
            
        /// <summary>
        /// 最后一次事件处理时间
        /// </summary>
        public DateTime LastEventTime { get; set; }

        /// <summary>
        /// 事件处理率（已处理数/总数）
        /// </summary>
        public double HandledRate => TotalEvents > 0 ? (double)HandledEvents / TotalEvents : 0;
    }
}