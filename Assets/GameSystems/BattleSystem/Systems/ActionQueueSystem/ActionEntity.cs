using System;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.ActionQueueSystem
{
    [Serializable]
    public class ActionEntity : IComparable<ActionEntity>, IComparable, IActionEntity
    {
        [SerializeField] private string id;

        [SerializeField] private int speed;

        [SerializeField] private int actionValue;
        [SerializeField] private BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> owner;

        public ActionEntity()
        {
        }

        public ActionEntity(string id, int speed, BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> unit)
        {
            Id = id;
            Speed = speed;
            ActionValue = 10000 / speed;
            owner = unit;
        }

        public int CompareTo(ActionEntity other)
        {
            return ActionValue.CompareTo(other.ActionValue);
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var str = "Id: " + Id.ToString() + " Speed: " + Speed.ToString() + " ActionValue: " +
                      ActionValue.ToString();
            return str;
        }

        public string Id
        {
            get => id;
            private set => id = value;
        }

        public int Speed
        {
            get => speed;
            set => speed = value;
        }

        public int ActionValue
        {
            get => actionValue;
            set => actionValue = value;
        }

        public BattleUnit<UnitData<UnitSaveData, UnitRuntimeData>> Owner
        {
            get => owner;
            set => owner = value;
        }
    }
}