using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class MainMenuState : StateBase
    {
        public override void Enter(IState prevState, object param)
        {
            base.Enter(prevState, param);
            Debug.Log("显示主菜单 UI");
        }

        public override void Exit(IState nextState)
        {
            base.Exit(nextState);
            Debug.Log("隐藏主菜单 UI");
        }
    }
}