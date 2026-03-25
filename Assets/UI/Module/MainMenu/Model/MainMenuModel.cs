using BH.UI.Base;
using JetBrains.Annotations;
using UniRx;

namespace BH.UI.Module.MainMenu.Model
{
    [UsedImplicitly]
    public class MainMenuModel : ModelBase
    {
        // 战斗按钮是否可交互
        public IReadOnlyReactiveProperty<bool> IsBattleBtnEnabled { get; private set; } =
            new ReactiveProperty<bool>(true);

        /// <summary>
        /// 设置战斗按钮状态
        /// </summary>
        public void SetBattleBtnState(bool isEnabled)
        {
            ((ReactiveProperty<bool>)IsBattleBtnEnabled).Value = isEnabled;
        }
    }
}