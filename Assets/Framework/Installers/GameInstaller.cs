using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Resource.Services;
using BH.Framework.Managers;
using BH.Framework.Services;
using Zenject;

namespace BH.Framework.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // ========== 1. 核心服务绑定（合并 LogService 的多契约绑定） ==========
            Container.BindInterfacesAndSelfTo<LogService>() // 具体实现：LogService
                .AsSingle() // 仅一次 AsSingle()，标记 LogService 单例
                .NonLazy();

            // ========== 2. 地址化服务绑定（同理合并） ==========
            Container.BindInterfacesAndSelfTo<AddressableService>()
                .AsSingle()
                .NonLazy();

            // ========== 3. 资源服务绑定 ==========
            Container.BindInterfacesAndSelfTo<ResourceService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<EventBus>()
                .AsSingle()
                .NonLazy();
            
            Container.BindInterfacesAndSelfTo<EventService>()
                .AsSingle()
                .NonLazy();

            // ========== 4. 游戏生命周期管理器绑定（同理合并） ==========
            Container.BindInterfacesAndSelfTo<GameService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle();

            // ========== 其他服务绑定（示例） ==========
            // Container.Bind<INetworkService>()
            //          .To<NetworkService>()
            //          .AsSingle()
            //          .Bind<IInitializable>();
        }
    }
}