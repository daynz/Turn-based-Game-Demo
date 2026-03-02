using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class PausedState : StateBase
    {
        public override void Enter(IState prevState, object param)
        {
            base.Enter(prevState, param);
            Debug.Log("游戏已暂停");
            // 显示暂停UI
        }

        public override void Exit(IState nextState)
        {
            base.Exit(nextState);
            Debug.Log("游戏恢复");
            // 隐藏暂停UI
        }
    }
}