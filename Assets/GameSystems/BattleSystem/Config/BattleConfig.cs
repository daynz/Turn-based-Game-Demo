using System.Collections.Generic;
using BH.Framework.Interfaces;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Config
{
    [CreateAssetMenu(fileName = "BattleConfig", menuName = "Honkai/Battle System/Config")]
    public class BattleConfig : ScriptableObject, IConfig
    {
        [SerializeField] public List<string> characterPartyId = new();
        [SerializeField] public List<string> monsterPartyId = new();
    }
}