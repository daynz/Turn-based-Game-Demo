using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Interfaces;
using BH.GameSystems.BattleSystem.Events;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Runtime;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Monsters.Runtime;
using Zenject;
using IInitializable = Zenject.IInitializable;

namespace BH.GameSystems.BattleSystem.Service
{
    public class UnitService : IInitializable, IDisposable, ISubscribeEvents
    {
        [Inject] private ILogService _logService;
        [Inject] private IEventService _eventService;

        /// <summary>
        /// 玩家队伍
        /// 存储参与战斗的玩家角色列表
        /// </summary>
        private List<BattleCharacter> characterParty = new();

        /// <summary>
        /// 敌人队伍
        /// 存储参与战斗的敌方单位列表
        /// </summary>
        private List<BattleMonster> monsterParty = new();

        private CharacterDatabase characterDatabase;
        //private UnitDatabase<MonsterData> monsterDatabase;

        public void Initialize()
        {
            _logService.Info("开始初始化");

            SubscribeEvents();

            _logService.Info("初始化完成");
        }

        public void SubscribeEvents()
        {
            _eventService.Subscribe<BattleLoadResourceEvent>(OnLoadResource);
        }

        private void OnLoadResource(BattleLoadResourceEvent @event)
        {
            _eventService.Publish(EventBuilder.CreateForEmptyData<BattleLoadResourceCompletedEvent>()
                .WithSender(this)
                .Build()
            );
        }

        private T GetEntityFromDatabaseById<T>(string id)
            where T : IUnitData<UnitSaveData, UnitRuntimeData>
        {
            if (string.IsNullOrEmpty(id))
            {
                _logService.Warning($"查找ID为空，类型：{typeof(T).Name}");
                return default;
            }

            if (typeof(T) == typeof(CharacterData))
            {
                var data = characterDatabase.GetUnit(id);
                if (data is T result)
                    return result;
            }

            _logService.Error($"不支持的类型：{typeof(T).Name}");
            return default;
        }

        public void Dispose()
        {
        }
    }
}