using System;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.DI.Interfaces;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Logging.Core;
using UnityEngine;

namespace BH.Framework.Services
{
    public abstract class ServiceBase : IService
    {
        public virtual string Name => GetType().Name;
        [field: Inject] protected LogService LogService { get; set; } = null;
        
        [field: Inject] protected EventService EventService { get; set; } = null;

        public abstract int Priority { get; }
        public bool IsInitialized { get; protected set; }

        public virtual Task InitializeAsync()
        {
            try
            {
                RegisterEventListener();
                Debug.Log($"[{Name}] 基础服务初始化完成。");
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }

        protected virtual void RegisterEventListener()
        {
        }

        public virtual void Shutdown()
        {
            // 基础服务的通用清理逻辑
            LogService.Debug($"基础服务已关闭。",Name);
        }
    }
}