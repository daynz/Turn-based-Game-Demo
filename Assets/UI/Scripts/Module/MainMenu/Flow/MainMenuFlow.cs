using System.Threading.Tasks;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data;
using BH.UI.Scripts.Managers;
using BH.UI.Scripts.Module.MainMenu.Model;
using BH.UI.Scripts.Module.MainMenu.View;
using JetBrains.Annotations;
using Zenject;

namespace BH.UI.Scripts.Module.MainMenu.Flow
{
    /// <summary>
    /// 主菜单流程控制
    /// </summary>
    [UsedImplicitly]
    public class MainMenuFlow : IInitializable
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly MainMenuModel _model;
        [Inject] private readonly ILogService _logService;

        public void Initialize()
        {
            _ = InitializeUIAsync();
        }
        
        private async Task InitializeUIAsync()
        {
            var mainMenuView = await _uiManager.OpenUIAsync<MainMenuView>(AssetKeys.AddressableNames.UIMainMenu);
        }
    }
}