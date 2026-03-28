using BH.UI.Scripts.Managers;
using BH.UI.Scripts.Module.Gacha.Model;
using JetBrains.Annotations;
using Zenject;

namespace BH.UI.Scripts.Module.Gacha.Flow
{
    /// <summary>
    /// 流程控制
    /// </summary>
    [UsedImplicitly]
    public class GachaFlow : IInitializable
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly GachaModel _model;

        public void Initialize()
        {
        }
    }
}