using BH.Framework.Infrastructure.Events.Core;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Installers
{
    [UsedImplicitly]
    public class EventInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindFactory<EventType, EventChannel, EventChannel.Factory>().FromNew();
            Container.BindInterfacesAndSelfTo<EventBus>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EventService>().AsSingle().NonLazy();
            Debug.Log("依赖注入 EventService");
            Container.Bind<EventManager>().FromComponentInHierarchy().AsSingle();
            Debug.Log("依赖注入 EventManager");
        }
    }
}