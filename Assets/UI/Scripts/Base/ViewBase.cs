using UniRx;
using UnityEngine;

namespace BH.UI.Scripts.Base
{
    /// <summary>
    /// 所有View的基类，封装UI绑定、依赖注入、订阅管理
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        protected CompositeDisposable ViewDisposables = new();
        protected bool IsInitialized;

        protected virtual void Awake()
        {
            // 自动初始化UI绑定
            if (!IsInitialized)
            {
                BindUI();
                IsInitialized = true;
            }
        }

        /// <summary>
        /// 绑定UI元素和ViewModel事件（子类实现）
        /// </summary>
        protected abstract void BindUI();

        /// <summary>
        /// 显示View（可重写添加动画）
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏View（可重写添加动画）
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            // 自动释放订阅
            ViewDisposables?.Dispose();
        }
    }
}