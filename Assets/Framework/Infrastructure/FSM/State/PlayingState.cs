using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class PlayingState : StateBase
    {
        public override void Enter(IState prevState, object param)
        {
            base.Enter(prevState, param);
            Debug.Log("游戏开始！");
            // 开启游戏逻辑、玩家输入等
        }

        public override void Exit(IState nextState)
        {
            base.Exit(nextState);
            Debug.Log("游戏暂停或结束！");
            // 关闭游戏逻辑、玩家输入等
        }
    }
}