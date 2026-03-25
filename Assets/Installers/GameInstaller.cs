using BH.Framework.Infrastructure.Events.Installers;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Resource.Manager;
using BH.Framework.Infrastructure.Resource.Services;
using BH.Framework.Managers;
using BH.Framework.Services;
using UnityEngine;
using Zenject;

namespace BH.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // ========== 日志服务服务绑定 ==========
            Container.BindInterfacesAndSelfTo<LogService>()
                .AsSingle()
                .NonLazy();

            Debug.Log("依赖注入 LogService");

            // ========== Addressable服务绑定 ==========
            Container.BindInterfacesAndSelfTo<AddressableService>()
                .AsSingle()
                .NonLazy();

            Debug.Log("依赖注入 AddressableService");

            // ========== 资源服务绑定 ==========
            Container.BindInterfacesAndSelfTo<ResourceService>()
                .AsSingle()
                .NonLazy();

            Debug.Log("依赖注入 AddressableService");

            Container.Bind<ResourceManager>()
                .FromComponentInHierarchy()
                .AsSingle();

            Debug.Log("依赖注入 ResourceManager");

            Container.BindInterfacesAndSelfTo<SceneService>()
                .AsSingle()
                .NonLazy();

            Debug.Log("依赖注入 SceneService");

            Container.Bind<SceneManager>()
                .FromComponentInHierarchy()
                .AsSingle();

            Debug.Log("依赖注入 SceneManager");

            Container.Install<EventInstaller>();
            
            // ========== 游戏生命周期管理器绑定 ==========
            Container.BindInterfacesAndSelfTo<GameService>()
                .AsSingle()
                .NonLazy();

            Debug.Log("依赖注入 GameService");

            Container.Bind<GameManager>()
                .FromComponentInHierarchy()
                .AsSingle();

            Debug.Log("依赖注入 GameManager");
        }
    }
}