
using BH.UI.Scripts.Module.Gacha.Flow;
using BH.UI.Scripts.Module.Gacha.Model;
using BH.UI.Scripts.Module.Gacha.ViewModel;
using Zenject;

namespace BH.UI.Scripts.Module.Gacha.Installer
{
    public class GachaInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // 绑定Model
            Container.BindInterfacesAndSelfTo<GachaModel>().AsSingle().NonLazy();

            // 绑定ViewModel
            Container.BindInterfacesAndSelfTo<GachaViewModel>().AsSingle().NonLazy();

            // 绑定流程控制
            Container.BindInterfacesAndSelfTo<GachaFlow>().AsSingle().NonLazy();
        }
    }
}
