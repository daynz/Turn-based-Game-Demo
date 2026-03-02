using BH.Framework.Singleton;
using BH.GameSystems.BattleSystem.Config;
using BH.GameSystems.BattleSystem.Systems.DamageCalculatorSystem;

namespace BH.GameSystems.BattleSystem.Manager
{
    public class DamageCalculatorManager : MonoSingleton<DamageCalculatorManager>
    {
        public override void Initialize()
        {
        }

        public void Init(BattleConfig config)
        {
        }

        public DamageData CalculateDamage()
        {
            throw new System.NotImplementedException();
        }
    }
}