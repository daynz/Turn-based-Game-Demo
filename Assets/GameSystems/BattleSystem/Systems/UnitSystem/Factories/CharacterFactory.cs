using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Runtime;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Factories
{
    /// <summary>
    /// 角色专用工厂
    /// </summary>
    public class CharacterFactory : UnitFactory<BattleCharacter, CharacterData>
    {
        public CharacterFactory(GameObject characterPrefab) : base(characterPrefab)
        {
        }

        public override BattleCharacter CreateFromData(CharacterData data, Transform parent = null)
        {
            var character = base.CreateFromData(data, parent);
            // if (character)
            // {
            // }

            return character;
        }
    }
}