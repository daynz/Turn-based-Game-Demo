using System;

namespace BH.Framework.Infrastructure.Resource.Data
{
    /// <summary>
    /// 资源加载状态枚举
    /// 覆盖所有资源加载场景的状态分类
    /// </summary>
    public enum ResourceLoadStatus
    {
        /// <summary>
        /// 加载成功
        /// </summary>
        Success,

        /// <summary>
        /// 加载失败（异常/资源不存在等）
        /// </summary>
        Failed,

        /// <summary>
        /// 加载被取消（令牌触发取消）
        /// </summary>
        Cancelled,

        /// <summary>
        /// 资源已加载（缓存命中，无需重复加载）
        /// </summary>
        AlreadyLoaded
    }
}