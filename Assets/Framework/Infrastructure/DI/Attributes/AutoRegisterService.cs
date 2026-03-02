using System;

namespace BH.Framework.Infrastructure.DI.Attributes
{
    /// <summary>
    /// 标记一个类为需要被自动注册的服务
    /// 该类必须实现 IService 接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AutoRegisterServiceAttribute : Attribute
    {
    }
}