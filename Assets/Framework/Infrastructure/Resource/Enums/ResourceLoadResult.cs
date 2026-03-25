using System;

namespace BH.Framework.Infrastructure.Resource.Data
{
        /// <summary>
    /// 资源加载结果通用封装类
    /// 统一管理所有类型资源的加载状态、结果、错误信息
    /// </summary>
    /// <typeparam name="T">资源类型（Unity Object/配置类/SceneInstance 等）</typeparam>
    public class ResourceLoadResult<T>
    {
        /// <summary>
        /// 加载成功的资源实例（失败/取消时为默认值）
        /// </summary>
        public T Asset { get; set; } = default;

        /// <summary>
        /// 资源加载状态
        /// </summary>
        public ResourceLoadStatus Status { get; set; } = ResourceLoadStatus.Failed;

        /// <summary>
        /// 错误描述信息（仅 Status=Failed 时有值）
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 加载异常对象（仅 Status=Failed 时有值）
        /// </summary>
        public Exception Exception { get; set; } = null;

        /// <summary>
        /// 加载耗时（毫秒）
        /// </summary>
        public long LoadDurationMs { get; set; } = 0;

        /// <summary>
        /// 资源唯一标识（AssetKeys.AddressableNames 常量值）
        /// </summary>
        public string ResourceKey { get; set; } = string.Empty;

        /// <summary>
        /// 快速创建成功结果
        /// </summary>
        /// <param name="resourceKey">资源标识</param>
        /// <param name="asset">资源实例</param>
        /// <param name="durationMs">加载耗时</param>
        /// <returns>成功结果实例</returns>
        public static ResourceLoadResult<T> Success(string resourceKey, T asset, long durationMs = 0)
        {
            return new ResourceLoadResult<T>
            {
                ResourceKey = resourceKey,
                Asset = asset,
                Status = ResourceLoadStatus.Success,
                LoadDurationMs = durationMs,
                ErrorMessage = string.Empty,
                Exception = null
            };
        }

        /// <summary>
        /// 快速创建失败结果
        /// </summary>
        /// <param name="resourceKey">资源标识</param>
        /// <param name="errorMsg">错误信息</param>
        /// <param name="exception">异常对象</param>
        /// <returns>失败结果实例</returns>
        public static ResourceLoadResult<T> Fail(string resourceKey, string errorMsg, Exception exception = null)
        {
            return new ResourceLoadResult<T>
            {
                ResourceKey = resourceKey,
                Asset = default,
                Status = ResourceLoadStatus.Failed,
                ErrorMessage = errorMsg,
                Exception = exception,
                LoadDurationMs = 0
            };
        }

        /// <summary>
        /// 快速创建取消结果
        /// </summary>
        /// <param name="resourceKey">资源标识</param>
        /// <returns>取消结果实例</returns>
        public static ResourceLoadResult<T> Cancelled(string resourceKey)
        {
            return new ResourceLoadResult<T>
            {
                ResourceKey = resourceKey,
                Asset = default,
                Status = ResourceLoadStatus.Cancelled,
                ErrorMessage = "加载操作被取消",
                Exception = null,
                LoadDurationMs = 0
            };
        }

        /// <summary>
        /// 快速创建已加载结果（缓存命中）
        /// </summary>
        /// <param name="resourceKey">资源标识</param>
        /// <param name="asset">缓存资源实例</param>
        /// <returns>已加载结果实例</returns>
        public static ResourceLoadResult<T> AlreadyLoaded(string resourceKey, T asset)
        {
            return new ResourceLoadResult<T>
            {
                ResourceKey = resourceKey,
                Asset = asset,
                Status = ResourceLoadStatus.AlreadyLoaded,
                ErrorMessage = string.Empty,
                Exception = null,
                LoadDurationMs = 0
            };
        }

        /// <summary>
        /// 检查是否加载成功
        /// </summary>
        /// <returns>成功返回true，否则返回false</returns>
        public bool IsSuccess()
        {
            return Status == ResourceLoadStatus.Success || Status == ResourceLoadStatus.AlreadyLoaded;
        }

        /// <summary>
        /// 转换为字符串（便于日志输出）
        /// </summary>
        /// <returns>结果描述字符串</returns>
        public override string ToString()
        {
            return $"[ResourceLoadResult] Key: {ResourceKey}, Status: {Status}, Duration: {LoadDurationMs}ms, Error: {ErrorMessage}";
        }
    }
}