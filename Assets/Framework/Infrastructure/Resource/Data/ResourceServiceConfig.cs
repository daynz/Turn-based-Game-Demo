using System;

namespace BH.Framework.Infrastructure.Resource.Data
{
    /// <summary>
    /// 资源服务核心配置类
    /// 集中管理所有可配置参数，支持外部注入/序列化/热更调整
    /// </summary>
    [Serializable]
    public class ResourceServiceConfig
    {
        #region 缓存配置
        /// <summary>
        /// 常驻资源缓存最大容量（LRU淘汰阈值）
        /// 超过该值时自动淘汰最久未使用的资源
        /// </summary>
        public int PersistentCacheMaxCount { get; set; } = 200;

        /// <summary>
        /// 配置文件缓存最大容量
        /// 超过该值时触发LRU淘汰（仅EnableConfigCacheAutoClean=true时生效）
        /// </summary>
        public int ConfigCacheMaxCount { get; set; } = 50;

        /// <summary>
        /// 是否启用配置缓存自动清理
        /// </summary>
        public bool EnableConfigCacheAutoClean { get; set; } = true;

        /// <summary>
        /// 配置缓存过期时间（秒）
        /// 超过该时间未访问的配置将被清理（0表示永不超时）
        /// </summary>
        public int ConfigCacheExpireSeconds { get; set; } = 0;

        /// <summary>
        /// 是否启用常驻缓存的引用计数保护
        /// 启用后仅当引用计数为0时才会被LRU淘汰
        /// </summary>
        public bool EnablePersistentCacheRefCountProtection { get; set; } = true;
        #endregion

        #region 加载配置
        /// <summary>
        /// 预加载批次大小
        /// 控制并行加载的资源数量，避免同时加载过多导致卡顿
        /// </summary>
        public int PreloadBatchSize { get; set; } = 10;

        /// <summary>
        /// 默认加载超时时间（秒）
        /// 超过该时间未完成的加载任务将被取消
        /// </summary>
        public int DefaultLoadTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否启用加载超时保护
        /// </summary>
        public bool EnableLoadTimeoutProtection { get; set; } = true;

        /// <summary>
        /// 最大并行加载任务数
        /// 限制同时进行的异步加载任务数量，避免线程池耗尽
        /// </summary>
        public int MaxParallelLoadTasks { get; set; } = 20;
        #endregion

        #region 日志/调试配置
        /// <summary>
        /// 是否启用详细加载日志
        /// 启用后会输出每个资源的加载/释放详情
        /// </summary>
        public bool EnableDetailedLoadLogs { get; set; } = false;

        /// <summary>
        /// 是否启用性能统计
        /// 启用后会记录每个资源的加载耗时、缓存命中率等数据
        /// </summary>
        public bool EnablePerformanceStatistics { get; set; } = false;

        /// <summary>
        /// 性能统计输出间隔（秒）
        /// 仅EnablePerformanceStatistics=true时生效
        /// </summary>
        public int PerformanceStatisticsInterval { get; set; } = 60;
        #endregion

        #region 容错配置
        /// <summary>
        /// 加载失败重试次数
        /// </summary>
        public int LoadFailedRetryCount { get; set; } = 1;

        /// <summary>
        /// 重试间隔（毫秒）
        /// </summary>
        public int RetryIntervalMs { get; set; } = 500;

        /// <summary>
        /// 场景激活失败是否自动卸载
        /// </summary>
        public bool AutoUnloadSceneOnActivateFailed { get; set; } = true;
        #endregion

        #region 便捷方法
        /// <summary>
        /// 验证配置合法性
        /// </summary>
        /// <returns>合法返回true，否则返回false</returns>
        public bool Validate()
        {
            var isValid = true;

            // 校验数值范围
            if (PersistentCacheMaxCount < 0)
            {
                PersistentCacheMaxCount = 200; // 重置为默认值
                isValid = false;
            }

            if (ConfigCacheMaxCount < 0)
            {
                ConfigCacheMaxCount = 50;
                isValid = false;
            }

            if (PreloadBatchSize < 1)
            {
                PreloadBatchSize = 10;
                isValid = false;
            }

            if (DefaultLoadTimeoutSeconds < 5)
            {
                DefaultLoadTimeoutSeconds = 30;
                isValid = false;
            }

            if (LoadFailedRetryCount < 0)
            {
                LoadFailedRetryCount = 1;
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void ResetToDefault()
        {
            // 缓存配置
            PersistentCacheMaxCount = 200;
            ConfigCacheMaxCount = 50;
            EnableConfigCacheAutoClean = true;
            ConfigCacheExpireSeconds = 0;
            EnablePersistentCacheRefCountProtection = true;

            // 加载配置
            PreloadBatchSize = 10;
            DefaultLoadTimeoutSeconds = 30;
            EnableLoadTimeoutProtection = true;
            MaxParallelLoadTasks = 20;

            // 日志/调试配置
            EnableDetailedLoadLogs = false;
            EnablePerformanceStatistics = false;
            PerformanceStatisticsInterval = 60;

            // 容错配置
            LoadFailedRetryCount = 1;
            RetryIntervalMs = 500;
            AutoUnloadSceneOnActivateFailed = true;
        }
        #endregion
    }
}