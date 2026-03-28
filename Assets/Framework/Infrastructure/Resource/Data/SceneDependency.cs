namespace BH.Framework.Infrastructure.Resource.Data
{
    /// <summary>
    /// 场景依赖项（加载场景时需同步加载的资源）
    /// </summary>
    public class SceneDependency
    {
        /// <summary>依赖资源名称（AssetKeys 常量）</summary>
        public string ResourceName { get; set; }
        
        /// <summary>是否为常驻资源</summary>
        public bool IsPersistent { get; set; } = false;

        public string Name { get; set; }
        public object Type { get; set; }
        public string Address { get; set; }
    }
}