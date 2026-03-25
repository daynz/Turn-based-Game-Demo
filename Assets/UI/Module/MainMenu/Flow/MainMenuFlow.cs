using System;
using BH.Framework.Infrastructure.Resource.Data;
using BH.UI.Managers;
using BH.UI.Module.MainMenu.Model;
using BH.UI.Module.MainMenu.View;
using UnityEngine;
using Zenject;

namespace BH.UI.Module.MainMenu.Flow
{
    /// <summary>
    /// 主菜单流程控制
    /// </summary>
    public class MainMenuFlow : IInitializable
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly MainMenuModel _model;

        public async void Initialize()
        {
            try
            {
                var mainMenuView = await _uiManager.OpenUIAsync<MainMenuView>(AssetKeys.AddressableNames.MainMenuUI);
            }
            catch (Exception e)
            {
                Debug.Log($"主菜单加载失败 \n {e.StackTrace}");
            }
        }
    }
}