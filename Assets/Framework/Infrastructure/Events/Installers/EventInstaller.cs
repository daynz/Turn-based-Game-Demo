using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Installers
{
    public class EventInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindFactory<EventType, EventChannel, EventChannel.Factory>().FromNew();
            Container.BindInterfacesAndSelfTo<EventBus>().AsSingle().NonLazy();
            Container.Bind<IEventService>().To<EventService>().AsSingle().NonLazy();
        }
    }
}