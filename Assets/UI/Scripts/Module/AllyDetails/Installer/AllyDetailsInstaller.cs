using BH.UI.Scripts.Module.AllyDetails.Flow;
using BH.UI.Scripts.Module.AllyDetails.Model;
using BH.UI.Scripts.Module.AllyDetails.ViewModel;
using Zenject;

namespace BH.UI.Scripts.Module.AllyDetails.Installer
{
    public class AllyDetailsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // 绑定Model
            Container.BindInterfacesAndSelfTo<AllyDetailsModel>().AsSingle().NonLazy();

            // 绑定ViewModel
            Container.BindInterfacesAndSelfTo<AllyDetailsViewModel>().AsSingle().NonLazy();

            // 绑定流程控制
            Container.BindInterfacesAndSelfTo<AllyDetailsFlow>().AsSingle().NonLazy();
        }
    }
}
