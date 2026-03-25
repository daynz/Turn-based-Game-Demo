using UnityEngine;
using Zenject;

namespace BH.Installers
{
    public class BattleSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("BattleSystemInstaller 开始注入");
            //Container.BindInterfacesAndSelfTo<UnitService>().AsSingle().NonLazy();
            //Container.BindInterfacesAndSelfTo<TurnService>().AsSingle().NonLazy();
            
            //Container.BindInterfacesAndSelfTo<BattleService>().AsSingle().NonLazy();
        }
    }
}