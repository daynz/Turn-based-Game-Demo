using System;
using BH.Framework.Enums;
using UnityEngine;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Config
{
    [Serializable]
    public class EventChannelConfig
    {
        [SerializeField] public EventType channelType;
        [SerializeField] public EventPriority priority;
        [SerializeField] public int maxQueueSize = 1000;
        [SerializeField] public bool enabled = true;
    }
}