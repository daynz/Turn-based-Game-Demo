using System;

namespace BH.Framework.Infrastructure.DI.Attributes
{
    /// <summary>
    /// 标记需要自动注入依赖的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
    }
}