using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Interfaces;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 全局事件类型缓存
    /// </summary>
    public static class GlobalEventTypeCache
    {
        #region 缓存容器（私有化，仅内部访问）

        /// <summary>
        /// 事件类型 → EventType枚举 缓存（核心缓存）
        /// Key: 事件类Type（如 PlayerLoginEvent）
        /// Value: [EventType] 特性标记的枚举值
        /// </summary>
        private static readonly Dictionary<Type, Enums.EventType> TypeToEventTypeCache = new();

        /// <summary>
        /// 事件类型 → 构造函数 缓存（优化反射性能）
        /// Key: 事件类Type（如 PlayerLoginEvent）
        /// Value: 匹配 EventBase 规范的构造函数
        /// </summary>
        private static readonly Dictionary<Type, ConstructorInfo> EventConstructorCache = new();

        /// <summary>
        /// 读写分离锁（读多写少场景最优）
        /// 读操作：EnterReadLock（允许多线程并发读）
        /// 写操作：EnterWriteLock（排他锁，仅单线程写）
        /// </summary>
        private static readonly ReaderWriterLockSlim CacheLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        #endregion

        #region 核心API：获取事件类型（从缓存/反射）

        /// <summary>
        /// 获取事件类对应的 EventType 枚举值（优先从缓存读取）
        /// </summary>
        /// <param name="eventType">事件类Type（需继承 EventBase(TData) 并标记 [EventType]）</param>
        /// <returns>EventType枚举值 | null（未标记特性/非法类型）</returns>
        /// <exception cref="ArgumentNullException">eventType为空时抛出</exception>
        public static Enums.EventType? GetEventTypeFromCache(Type eventType)
        {
            // 1. 空值校验
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType), "事件类型不能为空");

            // 2. 优先读缓存（读锁，并发安全）
            CacheLock.EnterReadLock();
            try
            {
                if (TypeToEventTypeCache.TryGetValue(eventType, out var cachedType))
                    return cachedType;
            }
            finally
            {
                CacheLock.ExitReadLock();
            }

            // 3. 缓存未命中 → 反射获取 + 写入缓存（写锁，排他）
            CacheLock.EnterWriteLock();
            try
            {
                // 双重检查：避免多线程同时写缓存
                if (TypeToEventTypeCache.TryGetValue(eventType, out var cachedType))
                    return cachedType;

                // 4. 校验事件类型合法性（必须继承 EventBase<TData>）
                if (!IsValidEventClass(eventType))
                {
                    // 非法类型不缓存，直接返回null
                    return null;
                }

                // 5. 反射获取 [EventType] 特性
                var eventTypeAttr = eventType.GetCustomAttribute<EventTypeAttribute>(inherit: true);
                if (eventTypeAttr == null)
                {
                    // 未标记特性的事件类不缓存
                    return null;
                }

                // 6. 写入缓存
                TypeToEventTypeCache.Add(eventType, eventTypeAttr.EventType);
                return eventTypeAttr.EventType;
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
        }

        #endregion

        #region 核心API：获取事件构造函数（从缓存/反射）

        /// <summary>
        /// 获取事件类的规范构造函数（优先从缓存读取）
        /// 构造函数签名：protected 事件类(object sender, TData data, EventPriority priority, bool allowMultipleHandlers)
        /// </summary>
        /// <typeparam name="TEvent">事件类类型</typeparam>
        /// <typeparam name="TData">事件数据类型</typeparam>
        /// <returns>匹配的构造函数 | null（无合法构造函数）</returns>
        public static ConstructorInfo GetEventConstructor<TEvent, TData>()
            where TEvent : Event<TData>
            where TData : IEventData
        {
            var eventType = typeof(TEvent);

            // 1. 优先读缓存（读锁）
            CacheLock.EnterReadLock();
            try
            {
                if (EventConstructorCache.TryGetValue(eventType, out var ctor))
                    return ctor;
            }
            finally
            {
                CacheLock.ExitReadLock();
            }

            // 2. 缓存未命中 → 反射获取 + 写入缓存（写锁）
            CacheLock.EnterWriteLock();
            try
            {
                // 双重检查
                if (EventConstructorCache.TryGetValue(eventType, out var ctor))
                    return ctor;

                // 3. 定义构造函数参数类型（匹配 EventBase 规范）
                var constructorParams = new[]
                {
                    typeof(object), // sender
                    typeof(TData), // data
                    typeof(Enums.EventPriority), // priority
                    typeof(bool) // allowMultipleHandlers
                };

                // 4. 反射获取 private/protected 构造函数
                ctor = eventType.GetConstructor(
                    bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
                    binder: null,
                    types: constructorParams,
                    modifiers: null);

                // 5. 写入缓存（即使ctor为null也缓存，避免重复反射）
                EventConstructorCache.Add(eventType, ctor);
                return ctor;
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
        }

        #endregion

        #region 辅助API：缓存管理

        /// <summary>
        /// 清空所有缓存（热更/动态加载程序集时使用）
        /// </summary>
        public static void ClearAllCache()
        {
            CacheLock.EnterWriteLock();
            try
            {
                TypeToEventTypeCache.Clear();
                EventConstructorCache.Clear();
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清空指定事件类型的缓存（单类型更新时使用）
        /// </summary>
        /// <param name="eventType">事件类Type</param>
        public static void ClearCacheForType(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            CacheLock.EnterWriteLock();
            try
            {
                TypeToEventTypeCache.Remove(eventType);
                EventConstructorCache.Remove(eventType);
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 获取缓存统计信息（监控/调试用）
        /// </summary>
        /// <returns>缓存键值对数量</returns>
        public static (int EventTypeCacheCount, int ConstructorCacheCount) GetCacheStats()
        {
            CacheLock.EnterReadLock();
            try
            {
                return (TypeToEventTypeCache.Count, EventConstructorCache.Count);
            }
            finally
            {
                CacheLock.ExitReadLock();
            }
        }

        #endregion

        #region 私有辅助方法：事件类型合法性校验

        /// <summary>
        /// 校验类型是否为合法的事件类（必须继承 EventBase(TData)）
        /// </summary>
        /// <param name="type">待校验类型</param>
        /// <returns>true=合法 | false=非法</returns>
        private static bool IsValidEventClass(Type type)
        {
            // 1. 排除接口/抽象类/值类型
            if (type.IsInterface || type.IsAbstract || type.IsValueType)
                return false;

            // 2. 递归检查基类是否为 EventBase<T>
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Event<>))
                {
                    // 确认泛型参数实现 IEventData
                    var dataType = baseType.GetGenericArguments()[0];
                    return typeof(IEventData).IsAssignableFrom(dataType);
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        #endregion

        #region 资源释放

        /// <summary>
        /// 释放锁资源（应用关闭时调用）
        /// </summary>
        public static void Dispose()
        {
            CacheLock.Dispose();
        }

        #endregion
    }
}