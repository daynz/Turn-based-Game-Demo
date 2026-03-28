using System;
using UniRx;
using Zenject;

namespace BH.UI.Scripts.Base
{
    /// <summary>
    /// 所有ViewModel的基类，封装UniRx订阅管理、Model绑定
    /// </summary>
    public abstract class ViewModelBase : IInitializable, IDisposable
    {
        protected CompositeDisposable Disposables = new();
        protected bool IsDisposed;

        /// <summary>
        /// 初始化ViewModel（绑定Model、初始化状态）
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 释放资源（自动取消所有UniRx订阅）
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed) return;

            Disposables?.Dispose();
            IsDisposed = true;
        }
    }
}