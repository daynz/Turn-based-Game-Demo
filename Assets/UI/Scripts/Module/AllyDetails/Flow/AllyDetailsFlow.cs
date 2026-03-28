using BH.UI.Scripts.Managers;
using BH.UI.Scripts.Module.AllyDetails.Model;
using JetBrains.Annotations;
using Zenject;

namespace BH.UI.Scripts.Module.AllyDetails.Flow
{
    /// <summary>
    /// 流程控制
    /// </summary>
    [UsedImplicitly]
    public class AllyDetailsFlow : IInitializable
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly AllyDetailsModel _model;
        public void Initialize()
        {
            
        }
    }
}
