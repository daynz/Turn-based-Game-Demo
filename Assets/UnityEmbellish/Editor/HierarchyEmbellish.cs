using UnityEditor;
using UnityEngine;

namespace UnityEmbellish.Editor
{
    [InitializeOnLoad]
    public class HierarchyEmbellish
    {
        static HierarchyEmbellish()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            // 找到对应的 GameObject
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!go) return;

            // 跳过被折叠的父物体（可选）
            if (!go.activeInHierarchy) return;

            // 根据名称/标签/层/组件 决定样式
            //var isUI = go.name.StartsWith("UI_") || go.layer == LayerMask.NameToLayer("UI");
            if (go.name.StartsWith("---"))
            {
                var bgColor = new Color(1f, 0.3f, 0.3f, 1f);
                var bgRect = selectionRect;
                bgRect.x += 16; // 避开折叠箭头
                bgRect.width -= 16;
                EditorGUI.DrawRect(bgRect, bgColor);

                var style = new GUIStyle(EditorStyles.label)
                {
                    normal =
                    {
                        textColor = new Color(0, 0, 0, 1)
                    }
                };
                bgRect.x += 16;
                GUI.Label(bgRect, go.name[3..], style);
            }
        }
    }
}