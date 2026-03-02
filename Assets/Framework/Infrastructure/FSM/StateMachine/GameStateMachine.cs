using System;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.State;

namespace BH.Framework.Infrastructure.FSM.StateMachine
{
    /// <summary>
    /// 游戏主状态机
    /// </summary>
    [Serializable]
    public class GameStateMachine : StateMachineBase<GameStateType>
    {
        public void Init()
        {
            // 注册所有的状态
            // RegisterState(GameStateType.Loading, new LoadingState());
            // RegisterState(GameStateType.MainMenu, new MainMenuState());
            // RegisterState(GameStateType.Playing, new PlayingState());
            // RegisterState(GameStateType.Paused, new PausedState());
            //
            // // 切换到初始状态
            // initialState = GameStateType.Loading;
            // ChangeState(initialState);
        }

        // public override void OnUpdate()
        // {
        //     base.OnUpdate(); // 调用基类的更新逻辑
        // }
        //
        // public override void OnFixedUpdate()
        // {
        //     base.OnFixedUpdate();
        // }
        //
        // public override void OnLateUpdate()
        // {
        //     base.OnLateUpdate();
        // }
    }
}