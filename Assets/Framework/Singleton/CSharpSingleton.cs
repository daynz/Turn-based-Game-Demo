using System;
using System.Threading;
using BH.Framework.Interfaces;

namespace BH.Framework.Singleton
{
    /// <summary>
    /// 纯 C# 逻辑单例基类<br/>
    /// 使用 Lazy&lt;T&gt; 实现线程安全的延迟初始化
    /// </summary>
    /// <typeparam name="T">必须是 CSharpSingleton&lt;T&gt; 的子类，且有无参构造函数</typeparam>
    public abstract class CSharpSingleton<T> : IInitializable
        where T : CSharpSingleton<T>, new()
    {
        private static readonly Lazy<T> LazyInstance = new(() =>
        {
            var instance = new T();
            instance.Initialize();
            return instance;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static T Instance => LazyInstance.Value;

        /// <summary>
        /// 指示单例实例是否已被创建（无论初始化成功与否）
        /// </summary>
        public static bool IsInstanceCreated => LazyInstance.IsValueCreated;

        /// <summary>
        /// 派生类实现此方法以执行初始化逻辑
        /// </summary>
        public virtual void Initialize()
        {
        }

        protected CSharpSingleton()
        {
        }
    }
}