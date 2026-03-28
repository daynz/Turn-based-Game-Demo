using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BH.Framework.Utilities.Collections
{
    /// <summary>
    /// 线程安全的 LRU 缓存实现<br/>
    /// 支持容量限制、最近最少使用淘汰策略、带过期时间（TTL）的键值对缓存<br/>
    /// 适用于资源缓存、场景缓存、对象池、高频访问数据缓存等场景
    /// </summary>
    /// <typeparam name="TKey">缓存键类型</typeparam>
    /// <typeparam name="TValue">缓存值类型</typeparam>
    public class ExpiredLruCache<TKey, TValue> : IEnumerable
    {
        /// <summary>
        /// 线程同步锁对象，保证多线程并发安全
        /// </summary>
        private readonly object _lockObj = new();

        /// <summary>
        /// 缓存最大容量，超出时触发 LRU 淘汰
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        /// 缓存核心存储字典，提供 O(1) 查找能力
        /// </summary>
        private readonly Dictionary<TKey, CacheItem> _cacheMap;

        /// <summary>
        /// LRU 访问顺序链表，头部为最近使用，尾部为最少使用
        /// </summary>
        private readonly LinkedList<TKey> _lruList;

        /// <summary>
        /// 默认过期时间（毫秒），0 表示永不过期
        /// </summary>
        private readonly long _defaultTtl;

        /// <summary>
        /// 缓存条目被淘汰时触发（LRU 自动移除）
        /// </summary>
        public event Action<TKey, TValue> OnItemEvicted;

        #region 属性

        /// <summary>
        /// 当前缓存中的有效条目数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObj) return _cacheMap.Count;
            }
        }

        /// <summary>
        /// 缓存最大容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 默认过期时间（毫秒）
        /// </summary>
        public long DefaultTtl => _defaultTtl;

        #endregion

        /// <summary>
        /// 初始化 LRU 带过期缓存实例
        /// </summary>
        /// <param name="capacity">缓存最大容量</param>
        /// <param name="defaultTtl">默认过期时间（毫秒），0=永不过期</param>
        /// <exception cref="ArgumentOutOfRangeException">容量小于等于 0 时抛出异常</exception>
        public ExpiredLruCache(int capacity, long defaultTtl = 0)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "缓存容量必须大于 0");

            _capacity = capacity;
            _defaultTtl = defaultTtl;
            _cacheMap = new Dictionary<TKey, CacheItem>(capacity);
            _lruList = new LinkedList<TKey>();
        }

        /// <summary>
        /// 缓存条目实体
        /// 存储缓存值与过期时间戳
        /// </summary>
        private class CacheItem
        {
            /// <summary>
            /// 缓存值
            /// </summary>
            public TValue Value { get; }

            /// <summary>
            /// 过期时间（UtcNow Ticks）
            /// </summary>
            public long ExpireTimeTicks { get; }

            /// <summary>
            /// 构造缓存条目
            /// </summary>
            /// <param name="value">缓存值</param>
            /// <param name="expireTimeTicks">过期时间戳</param>
            public CacheItem(TValue value, long expireTimeTicks)
            {
                Value = value;
                ExpireTimeTicks = expireTimeTicks;
            }
        }

        #region 核心公共方法

        /// <summary>
        /// 尝试获取缓存值
        /// 自动检查过期，命中则刷新 LRU 访问顺序
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">获取到的缓存值（失败返回默认值）</param>
        /// <returns>是否成功获取有效缓存</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            lock (_lockObj)
            {
                value = default;

                if (!_cacheMap.TryGetValue(key, out var item))
                    return false;

                if (IsExpired(item.ExpireTimeTicks))
                {
                    RemoveInternal(key);
                    return false;
                }

                MoveToFirst(key);
                value = item.Value;
                return true;
            }
        }

        /// <summary>
        /// 添加缓存（使用默认过期时间）
        /// 若键已存在则覆盖
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void Add(TKey key, TValue value)
        {
            Add(key, value, _defaultTtl);
        }

        /// <summary>
        /// 添加缓存并指定自定义过期时间
        /// 自动处理容量超限后的 LRU 淘汰
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="ttlMilliseconds">自定义过期时间（毫秒）</param>
        public void Add(TKey key, TValue value, long ttlMilliseconds)
        {
            lock (_lockObj)
            {
                if (_cacheMap.ContainsKey(key))
                    RemoveInternal(key);

                if (_cacheMap.Count >= _capacity)
                    RemoveLast();

                var expireTicks = GetExpireTicks(ttlMilliseconds);
                var item = new CacheItem(value, expireTicks);

                _cacheMap[key] = item;
                _lruList.AddFirst(key);
            }
        }

        /// <summary>
        /// 获取已缓存的内容名称列表
        /// </summary>
        /// <returns>已缓存的内容名称列表</returns>
        public List<TKey> GetAllCacheName()
        {
            lock (_lockObj)
            {
                return _cacheMap.Keys.ToList();
            }
        }

        /// <summary>
        /// 主动移除指定缓存条目
        /// </summary>
        /// <param name="key">缓存键</param>
        public bool Remove(TKey key)
        {
            bool flag;
            lock (_lockObj)
            {
                flag = RemoveInternal(key);
            }

            return flag;
        }

        /// <summary>
        /// 清空所有缓存条目
        /// </summary>
        public void Clear()
        {
            lock (_lockObj)
            {
                _cacheMap.Clear();
                _lruList.Clear();
            }
        }

        /// <summary>
        /// 主动扫描并清理所有已过期的缓存条目
        /// </summary>
        public void ClearExpiredItems()
        {
            lock (_lockObj)
            {
                var keysToRemove = new List<TKey>();

                foreach (var (key, item) in _cacheMap)
                {
                    if (IsExpired(item.ExpireTimeTicks))
                        keysToRemove.Add(key);
                }

                foreach (var key in keysToRemove)
                    RemoveInternal(key);
            }
        }

        #endregion

        #region 内部工具方法

        /// <summary>
        /// 判断缓存条目是否已过期
        /// </summary>
        /// <param name="expireTicks">过期时间戳</param>
        /// <returns>是否过期</returns>
        private bool IsExpired(long expireTicks)
        {
            if (expireTicks <= 0) return false;
            return DateTime.UtcNow.Ticks > expireTicks;
        }

        /// <summary>
        /// 根据TTL毫秒数计算过期时间戳（UTC Ticks）
        /// </summary>
        /// <param name="ttlMs">过期时间（毫秒）</param>
        /// <returns>UTC过期时间戳</returns>
        private long GetExpireTicks(long ttlMs)
        {
            if (ttlMs <= 0)
                return 0;

            const long ticksPerMillisecond = 10000; // 1ms = 10000 ticks
            return DateTime.UtcNow.Ticks + ttlMs * ticksPerMillisecond;
        }

        /// <summary>
        /// 将指定键移动到 LRU 链表头部（标记为最近使用）
        /// </summary>
        /// <param name="key">缓存键</param>
        private void MoveToFirst(TKey key)
        {
            _lruList.Remove(key);
            _lruList.AddFirst(key);
        }

        /// <summary>
        /// 内部移除方法
        /// </summary>
        /// <param name="key">缓存键</param>
        private bool RemoveInternal(TKey key)
        {
            if (!_cacheMap.Remove(key, out var item)) return false;
            OnItemEvicted?.Invoke(key, item.Value);
            _lruList.Remove(key);
            return true;
        }

        /// <summary>
        /// 移除 LRU 链表尾部（最少使用）条目
        /// </summary>
        private void RemoveLast()
        {
            if (_lruList.Last == null) return;
            var lastKey = _lruList.Last.Value;
            RemoveInternal(lastKey);
        }

        #endregion

        /// <summary>
        /// 返回遍历缓存键值对的枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        public IEnumerator GetEnumerator()
        {
            lock (_lockObj)
            {
                var copy = new Dictionary<TKey, CacheItem>(_cacheMap);
                foreach (var pair in copy)
                {
                    yield return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Value);
                }
            }
        }
    }
}