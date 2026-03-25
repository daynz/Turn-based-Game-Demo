using System;
using BH.Framework.Infrastructure.Resource.Enums;
using UnityEngine;

namespace BH.Framework.Infrastructure.Resource.Data
{
    /// <summary>
    /// 场景过渡参数
    /// </summary>
    public class SceneTransitionParams
    {
        /// <summary>过渡动画类型</summary>
        public SceneTransitionType TransitionType { get; set; } = SceneTransitionType.None;
        
        /// <summary>过渡时长（秒）</summary>
        public float Duration { get; set; } = 0.5f;
        
        /// <summary>过渡颜色（如淡入淡出的遮罩色）</summary>
        public Color TransitionColor { get; set; } = Color.black;
        
        /// <summary>自定义过渡回调（开始/更新/结束）</summary>
        public Action<float> OnCustomTransitionUpdate { get; set; }
        
        /// <summary>是否等待过渡完成后激活场景</summary>
        public bool WaitForTransition { get; set; } = true;
    }
}