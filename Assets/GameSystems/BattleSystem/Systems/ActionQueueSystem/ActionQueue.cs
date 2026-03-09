using System;
using System.Collections.Generic;
using System.Linq;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Singleton;
using BH.Framework.Utilities.Collections;
using BH.GameSystems.BattleSystem.Config;
using BH.GameSystems.BattleSystem.Events;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.ActionQueueSystem
{
    [Serializable]
    public class ActionQueue : MonoSingleton<ActionQueue>
    {
        private PriorityQueue<ActionEntity> _queue = new();
        private List<ICharacterData> _characterDataCache;

        public override void Initialize()
        {
        }

        private void RegisterEventListener()
        {
            //_eventHandler.AddSubscription<LoadUnitDataCompletedEvent>(GenerateCache);
        }

        private void GenerateCache(LoadUnitDataCompletedEvent @event)
        {
            //_characterDataCache = (List<ICharacterData>)@event.Data;
        }

        public void Init(BattleConfig config)
        {
            foreach (var id in config.characterPartyId)
            {
            }
        }

        public ActionEntity GetActionUnit()
        {
            return _queue.Top;
        }

        public void AddEntity(ActionEntity entity)
        {
            if (_queue.Contains(entity))
            {
                return;
            }

            _queue.Push(entity);
        }

        public ActionEntity RemoveEntity(string id)
        {
            var entity = QueryById(id);
            return _queue.Remove(entity) ? entity : null;
        }

        public void ModifyEntity(string id, int speed)
        {
            var list = _queue.GetSortedElements() as List<ActionEntity>;
            var entity = list?.FirstOrDefault(entity => entity.Id == id);
            _queue.Remove(entity);
            if (entity != null) entity.Speed = speed;
            _queue.Push(entity);
        }

        private ActionEntity QueryById(string id)
        {
            var list = _queue.GetSortedElements() as List<ActionEntity>;
            return list?.FirstOrDefault(entity => entity.Id == id);
        }

        private ActionEntity CreateActionEntity(BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> data)
        {
            return new ActionEntity(data.UnitId, (int)data.Data.RuntimeData.Speed, data);
        }

        public override string ToString()
        {
            var str = "";
            if (_queue.GetSortedElements() is List<ActionEntity> list)
                str = list.Aggregate(str, (current, entity) => current + (entity + "\n"));

            return str;
        }
    }
}