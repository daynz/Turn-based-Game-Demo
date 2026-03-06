using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Installers
{
    public class EventInstaller : Installer<EventInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EventBus>().AsSingle();

            Container.BindInterfacesAndSelfTo<EventService>().AsSingle();

            Container.Bind<IEventService>().To<EventService>().AsSingle();
        }
    }
}