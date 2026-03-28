using TMPro;
using UnityEditor;
using UnityEngine;

namespace BH.Framework.Editor
{
    /// <summary>
    /// 批量设置场景中所有TextMeshPro字体的编辑器工具
    /// </summary>
    public class TextMeshProTool : EditorWindow
    {
        // 用于选择的目标字体
        private TMP_FontAsset _targetFont;

        // 菜单栏入口
        [MenuItem("Tools/TextMeshProTools/批量设置所有TextMeshPro字体")]
        private static void ShowWindow()
        {
            // 创建并显示窗口
            GetWindow<TextMeshProTool>("TMP字体批量设置");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            // 标题提示
            EditorGUILayout.LabelField("批量替换场景中所有TextMeshPro字体", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 字体选择字段
            _targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "目标字体",
                _targetFont,
                typeof(TMP_FontAsset),
                false
            );

            GUILayout.Space(15);

            // 执行按钮
            using (new EditorGUI.DisabledScope(!_targetFont))
            {
                if (GUILayout.Button("一键替换所有TextMeshPro字体", GUILayout.Height(30)))
                {
                    BatchSetAllTMPFont();
                }
            }

            // 提示文字
            if (!_targetFont)
            {
                EditorGUILayout.HelpBox("请先选择一个TextMeshPro字体资源！", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("点击按钮后，会自动遍历场景中所有激活/未激活的TMP文本并替换字体", MessageType.Info);
            }
        }

        /// <summary>
        /// 批量设置所有TextMeshPro字体
        /// </summary>
        private void BatchSetAllTMPFont()
        {
            if (!_targetFont)
            {
                EditorUtility.DisplayDialog("错误", "请先选择目标字体！", "确定");
                return;
            }

            // 查找场景中所有TextMeshPro组件（包含未激活的物体）
            // true 参数代表查找所有对象，包括隐藏、未激活的
            TMP_Text[] allTmpTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();

            if (allTmpTexts.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "场景中未找到任何TextMeshPro组件！", "确定");
                return;
            }

            // 计数
            var successCount = 0;

            // 遍历所有TMP文本
            foreach (var tmpText in allTmpTexts)
            {
                // 过滤掉预制件、编辑器内置对象，只处理场景中的真实物体
                if (!IsSceneObject(tmpText.gameObject)) continue;

                // 设置字体
                tmpText.font = _targetFont;
            
                // 强制刷新文本显示
                tmpText.SetAllDirty();

                successCount++;
            }

            // 完成提示
            EditorUtility.DisplayDialog("完成", $"成功替换 {successCount} 个TextMeshPro组件的字体！", "确定");
        }

        /// <summary>
        /// 判断是否为场景中的真实物体（过滤预制件、编辑器对象）
        /// </summary>
        private bool IsSceneObject(GameObject obj)
        {
            // 排除预制件、编辑器资源、场景外对象
            return obj 
                   && !EditorUtility.IsPersistent(obj) 
                   && obj.hideFlags == HideFlags.None;
        }
    }
}