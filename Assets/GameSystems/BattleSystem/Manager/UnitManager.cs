using System;
using System.Collections.Generic;
using System.Linq;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Singleton;
using BH.GameSystems.BattleSystem.Config;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Events.EventUnits;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Runtime;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Factories;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Manager
{
    [Serializable]
    public class UnitManager : MonoSingleton<UnitManager>
    {
        public string Name => GetType().Name;

        /// <summary>
        /// 玩家队伍
        /// 存储参与战斗的玩家角色列表
        /// </summary>
        [SerializeField] private List<BattleCharacter> characterParty = new();

        /// <summary>
        /// 敌人队伍
        /// 存储参与战斗的敌方单位列表
        /// </summary>
        //[SerializeField] private List<BattleMonster> monsterParty = new();
        [SerializeField] private CharacterDatabase characterDatabase;
        //[SerializeField] private UnitDatabase<MonsterData> monsterDatabase;

        [SerializeField] private string characterDataRelativePath = "Data/characters.json";
        [SerializeField] private string monsterDataRelativePath = "Data/monsters.json";

        [Inject] private LogService _logService;
        [Inject] private EventService _eventService;

        public override async void Initialize()
        {
            try
            {
                characterDatabase = new CharacterDatabase();
                await characterDatabase.LoadFromJson();
                base.Initialize();
            }
            catch (Exception e)
            {
                throw; // TODO 处理异常
            }
        }

        public void Init(BattleConfig config)
        {
            foreach (var unitData in config.characterPartyId.Select(GetEntityFromDatabaseById<CharacterData>))
            {
                var characterFactory = new CharacterFactory(unitData.Prefab);
                characterParty.Add(characterFactory.CreateFromData(unitData));
            }

            _eventService.Publish(
                EventBuilder.Create<LoadUnitDataCompletedEvent>()
                    .WithSender(this)
                    .WithData(UnitFaction.Character)
                    .Build()
            );
        }

        private T GetEntityFromDatabaseById<T>(string id)
            where T : IUnitData<UnitSaveData, UnitRuntimeData>
        {
            if (string.IsNullOrEmpty(id))
            {
                _logService.Warning($"查找ID为空，类型：{typeof(T).Name}", Name);
                return default;
            }

            if (typeof(T) == typeof(CharacterData))
            {
                var data = characterDatabase.GetUnit(id);
                if (data is T result)
                    return result;
            }

            _logService.Error($"不支持的类型：{typeof(T).Name}", Name);
            return default;
        }
    }
}