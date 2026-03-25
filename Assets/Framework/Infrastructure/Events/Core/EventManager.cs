using System;
using BH.Framework.Infrastructure.Events.Interfaces;
using UnityEngine;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Core
{
    public class EventManager : MonoBehaviour
    {
        [Inject] private IEventService _eventService;
        
        [SerializeField] private int maxEventsPerFrame = 1000;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            // 每帧调用处理函数
            _eventService?.ProcessChannels(maxEventsPerFrame);
        }
    }
}