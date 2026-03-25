using System;

namespace BH.UI.Base
{
    /// <summary>
    /// 所有Model的基类，封装数据存储和基础逻辑
    /// </summary>
    public abstract class ModelBase : IDisposable
    {
        protected bool IsDisposed;

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
        }
    }
}