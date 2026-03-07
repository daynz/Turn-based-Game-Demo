using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Installers
{
    public class EventInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EventBus>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EventService>().AsSingle().NonLazy();
            Container.Bind<IEventService>().To<EventService>().AsSingle().NonLazy();
        }
    }
}