using BH.UI.Base;
using BH.UI.Managers;
using BH.UI.Module.MainMenu.Model;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace BH.UI.Module.MainMenu.ViewModel
{
    [UsedImplicitly]
    public class MainMenuViewModel : ViewModelBase
    {
        // 按钮点击事件（暴露给View）
        public readonly Subject<Unit> OnBattleClick = new();
        public readonly Subject<Unit> OnBattleSettingClick = new();
        public readonly Subject<Unit> OnSettingsClick = new();
        public readonly Subject<Unit> OnExitClick = new();

        // 状态流（暴露给View）
        public IReadOnlyReactiveProperty<bool> IsBattleBtnEnabled => _model.IsBattleBtnEnabled;

        [Inject] private readonly MainMenuModel _model;
        [Inject] private readonly UIManager _uiManager;

        public override void Initialize()
        {
            // 绑定按钮事件逻辑
            OnBattleClick.Subscribe(_ =>
            {
                _model.SetBattleBtnState(false); // 点击后禁用按钮
                Debug.Log("开始战斗！");
                // 示例：打开战斗配置UI
                // _uiManager.OpenUI<BattleConfigView>("BattleConfig/BattleConfig");
            }).AddTo(Disposables);

            OnBattleSettingClick.Subscribe(_ => { Debug.Log("打开战斗配置！"); }).AddTo(Disposables);

            OnSettingsClick.Subscribe(_ =>
            {
                Debug.Log("打开设置界面！");
                // _uiManager.OpenUI<SettingsView>("Settings/Settings", UILayer.Popup);
            }).AddTo(Disposables);

            OnExitClick.Subscribe(_ =>
            {
                Debug.Log("退出游戏！");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }).AddTo(Disposables);
        }
    }
}