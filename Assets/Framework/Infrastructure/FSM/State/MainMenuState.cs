using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class MainMenuState : StateBase
    {
        public MainMenuState(ILogService logService, IEventService eventService) : base(logService, eventService)
        {
        }

        public override void Enter()
        {
            Debug.Log("显示主菜单 UI");
        }

        public override void Exit()
        {
            Debug.Log("隐藏主菜单 UI");
        }
    }
}