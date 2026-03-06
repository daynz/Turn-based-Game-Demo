using System;
using BH.Framework.Services;
using UnityEngine;
using Zenject;

namespace BH.Framework.Managers
{
    [Serializable]
    public class GameManager : MonoInstaller
    {
        [Inject] private GameService _gameLifecycleService;
        
        // 按钮尺寸（宽、高）
        public Vector2 buttonSize = new Vector2(200, 60);

        // 按钮之间的垂直间距
        public float buttonSpacing = 20f;

        private void OnGUI()
        {
            // 1. 计算第一个按钮的初始Y坐标（屏幕中心 - 总按钮区域高度的一半）
            // 总按钮区域高度 = 3个按钮高度 + 2个间距
            float totalButtonAreaHeight = 3 * buttonSize.y + 2 * buttonSpacing;
            float startY = (Screen.height - totalButtonAreaHeight) / 2f;

            // 2. 统一设置按钮样式（居中对齐、字体大小）
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            GUI.skin.button.fontSize = 20;

            // 3. 计算每个按钮的中心X坐标（屏幕宽度的一半）
            float centerX = Screen.width / 2f - buttonSize.x / 2f;

            // 4. 绘制第一个按钮
            if (GUI.Button(new Rect(centerX, startY, buttonSize.x, buttonSize.y), "回合制战斗模式"))
            {
                Debug.Log("点击了按钮1");
            }

            // 5. 绘制第二个按钮（Y坐标 = 第一个按钮Y + 按钮高度 + 间距）
            float secondButtonY = startY + buttonSize.y + buttonSpacing;
            if (GUI.Button(new Rect(centerX, secondButtonY, buttonSize.x, buttonSize.y), "按钮2"))
            {
                Debug.Log("点击了按钮2");
            }

            // 6. 绘制第三个按钮（Y坐标 = 第二个按钮Y + 按钮高度 + 间距）
            float thirdButtonY = secondButtonY + buttonSize.y + buttonSpacing;
            if (GUI.Button(new Rect(centerX, thirdButtonY, buttonSize.x, buttonSize.y), "按钮3"))
            {
                Debug.Log("点击了按钮3");
            }
        }
    }
}