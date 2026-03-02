using System;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Base
{
    [Serializable]
    public class UnitSaveData : IUnitSaveData
    {
        public string Name => GetType().Name;
        [SerializeField] [Tooltip("角色id")] [JsonProperty]
        protected string unitId = "";

        [Min(0)] [Tooltip("生命值")] [SerializeField] [JsonProperty]
        protected float health;

        [Min(0)] [Tooltip("攻击力")] [SerializeField] [JsonProperty]
        protected float attack;

        [Min(0)] [Tooltip("防御力")] [SerializeField] [JsonProperty]
        protected float defense;

        [Min(0)] [Tooltip("速度")] [SerializeField] [JsonProperty]
        protected float speed;

        [Inject] protected LogService LOGService = null;

        public string UnitId => unitId;

        public float Health
        {
            get => health;
            set => health = value;
        }

        public float Attack
        {
            get => attack;
            set => attack = value;
        }

        public float Defense
        {
            get => defense;
            set => defense = value;
        }

        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        /// <summary>
        /// 将当前对象序列化为 JSON 字符串
        /// </summary>
        /// <returns>JSON 字符串</returns>
        public virtual string ToJson()
        {
            // TODO: 添加格式化和空值处理，提升可读性和实用性
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化并覆盖当前对象的属性
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        public virtual void FromJson(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }
    }
}