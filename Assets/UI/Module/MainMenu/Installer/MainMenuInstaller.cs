using BH.UI.Managers;
using BH.UI.Module.MainMenu.Flow;
using BH.UI.Module.MainMenu.Model;
using BH.UI.Module.MainMenu.ViewModel;
using Zenject;

namespace BH.UI.Module.MainMenu.Installer
{
    public class MainMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // 绑定UI管理器（全局单例）
            Container.BindInterfacesAndSelfTo<UIFactory>().AsSingle().NonLazy();
            Container.Bind<UIManager>()
                .FromComponentInHierarchy()
                .AsSingle();

            // // 绑定Model
            Container.BindInterfacesAndSelfTo<MainMenuModel>().AsSingle().NonLazy();

            // 绑定ViewModel
            Container.BindInterfacesAndSelfTo<MainMenuViewModel>().AsSingle().NonLazy();

            // 绑定流程控制
            Container.BindInterfacesAndSelfTo<MainMenuFlow>().AsSingle().NonLazy();
        }
    }
}