using BH.Framework.Interfaces;
using UnityEngine;

namespace BH.Framework.Singleton
{
    /// <summary>
    /// 单例模式基类
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour
        where T : MonoSingleton<T>
    {
        private static T _instance;
        private static bool _isQuitting;
        protected bool IsDestroy;

        public static T Instance
        {
            get
            {
                if (!_isQuitting) return _instance;
                Debug.LogWarning($"[MonoSingleton] 应用正在退出，无法获取 {typeof(T).Name} 实例");
                return null;
            }
        }

        protected virtual void Awake()
        {
            if (_isQuitting || _instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        public virtual void Initialize()
        {
            Debug.Log($"[{typeof(T).Name}] 初始化");
        }

        /// <summary>
        /// 销毁方法
        /// </summary>
        public virtual void Destroy()
        {
            Debug.Log($"[{typeof(T).Name}] 已销毁");
        }
    }
}