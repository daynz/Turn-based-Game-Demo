using BH.UI.Scripts.Base;
using JetBrains.Annotations;
using UniRx;

namespace BH.UI.Scripts.Module.MainMenu.Model
{
    [UsedImplicitly]
    public class MainMenuModel : ModelBase
    {
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