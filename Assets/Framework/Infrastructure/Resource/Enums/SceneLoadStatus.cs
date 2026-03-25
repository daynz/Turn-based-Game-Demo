namespace BH.Framework.Infrastructure.Resource.Enums
{
    /// <summary>
    /// 场景加载状态枚举
    /// </summary>
    public enum SceneLoadStatus
    {
        /// <summary>未加载</summary>
        Unloaded,
        /// <summary>加载中</summary>
        Loading,
        /// <summary>已加载（未激活）</summary>
        Loaded,
        /// <summary>已激活</summary>
        Activated,
        /// <summary>卸载中</summary>
        Unloading,
        /// <summary>加载失败</summary>
        Failed,
        /// <summary>预加载中</summary>
        Preloading
    }
}