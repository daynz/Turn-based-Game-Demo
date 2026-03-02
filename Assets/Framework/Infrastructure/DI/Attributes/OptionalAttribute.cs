using System;

namespace BH.Framework.Infrastructure.DI.Attributes
{
    /// <summary>
    /// 标记服务为“可选依赖”，如果找不到不会报错
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OptionalAttribute : Attribute
    {
    }
}