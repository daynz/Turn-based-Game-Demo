using System;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Runtime
{
    /// <summary>
    /// 战斗角色基类
    /// </summary>
    [Serializable]
    public class BattleCharacter : BattleUnit<CharacterData>, IBattleCharacter
    {
        ICharacterData IBattleUnit<ICharacterData>.Data => Data;

        void IBattleUnit<ICharacterData>.Initialize(ICharacterData configData)
        {
            if (configData is CharacterData concreteData)
            {
                Initialize(concreteData);
            }
            else
            {
                LOGService.Error(
                    $"BattleCharacter 初始化失败：传入的数据类型不是 CharacterData，实际类型为 {configData?.GetType()}",Name);
            }
        }

        void IBattleUnit<ICharacterData>.Cleanup() => Cleanup();

        public override void Initialize(CharacterData configData)
        {
            base.Initialize(configData);

            // 角色特有的初始化逻辑，例如战斗开始时满能量
            if (Data?.RuntimeData is { } runtime)
            {
                runtime.Energy = runtime.EnergyMax; // 设置初始能量为最大值
            }
        }
    }
}