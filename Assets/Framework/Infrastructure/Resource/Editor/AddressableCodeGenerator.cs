using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace BH.Framework.Infrastructure.Resource.Editor
{
    /// <summary>
    /// Addressables 代码生成器
    /// 编辑时生成 AssetKeys 常量类，包含标签、AddressableName 对应的静态常量
    /// </summary>
    public class AddressableCodeGenerator : EditorWindow
    {
        // 生成代码的保存路径（可在编辑器窗口调整）
        private string _outputPath = "Assets/Framework/Infrastructure/Resource/Data/AssetKeys.cs";

        // Addressables 设置文件路径
        private const string AddressableSettingsPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

        [MenuItem("Tools/Addressables/Generate AssetKeys Code")]
        public static void ShowWindow()
        {
            GetWindow<AddressableCodeGenerator>("Addressables 代码生成");
        }

        private void OnGUI()
        {
            GUILayout.Label("Addressables 代码生成配置", EditorStyles.boldLabel);

            // 输出路径输入框
            _outputPath = EditorGUILayout.TextField("生成路径", _outputPath);

            // 生成按钮
            if (GUILayout.Button("生成 AssetKeys 代码") && !string.IsNullOrEmpty(_outputPath))
            {
                GenerateAssetKeysCode();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("成功", "AssetKeys 代码生成完成！", "确定");
            }
        }

        /// <summary>
        /// 核心逻辑：扫描 Addressables 配置，生成代码
        /// </summary>
        private void GenerateAssetKeysCode()
        {
            // 加载 Addressables 设置
            var addressableSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(AddressableSettingsPath);
            if (!addressableSettings)
            {
                EditorUtility.DisplayDialog("错误", "未找到 Addressables 设置文件！", "确定");
                return;
            }

            // 构建代码字符串
            var codeBuilder = new StringBuilder();
            // 头部注释 + 命名空间
            codeBuilder.AppendLine("// 此文件由 AddressableCodeGenerator 自动生成，请勿手动修改！");
            codeBuilder.AppendLine("namespace BH.Framework.Infrastructure.Resource.Data");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    /// <summary>");
            codeBuilder.AppendLine("    /// Addressables 资源键常量（包含 AddressableName、标签）");
            codeBuilder.AppendLine("    /// </summary>");
            codeBuilder.AppendLine("    public static class AssetKeys");
            codeBuilder.AppendLine("    {");

            // 1. 生成 AddressableName 静态嵌套类（基于 Addressables 分组中的资源项）
            codeBuilder.AppendLine("        /// <summary>");
            codeBuilder.AppendLine("        /// AddressableName（资源唯一标识）");
            codeBuilder.AppendLine("        /// </summary>");
            codeBuilder.AppendLine("        public static class AddressableNames");
            codeBuilder.AppendLine("        {");
            foreach (var group in addressableSettings.groups)
            {
                foreach (var entry in group.entries)
                {
                    // 过滤空名称/特殊字符，生成合法的常量名（驼峰命名）
                    var constantName = GetValidConstantName(entry.address);
                    codeBuilder.AppendLine($"            public const string {constantName} = \"{entry.address}\";");
                }
            }

            codeBuilder.AppendLine("        }");

            // 2. 生成 Label 静态嵌套类（基于所有标签）
            codeBuilder.AppendLine("\n        /// <summary>");
            codeBuilder.AppendLine("        /// Label（资源标签）");
            codeBuilder.AppendLine("        /// </summary>");
            codeBuilder.AppendLine("        public static class Labels");
            codeBuilder.AppendLine("        {");
            var allLabels = addressableSettings.GetLabels();
            foreach (var label in allLabels)
            {
                if (string.IsNullOrEmpty(label)) continue;
                var constantName = GetValidConstantName(label);
                codeBuilder.AppendLine($"            public const string {constantName} = \"{label}\";");
            }

            codeBuilder.AppendLine("        }");

            // 闭合主类和命名空间
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("}");

            // 写入文件（自动创建目录）
            var directory = Path.GetDirectoryName(_outputPath);
            if (!Directory.Exists(directory))
            {
                if (directory != null) Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_outputPath, codeBuilder.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 转换为合法的 C# 常量名（移除特殊字符、替换空格/斜杠等，生成帕斯卡驼峰命名）
        /// </summary>
        private string GetValidConstantName(string originalName)
        {
            if (string.IsNullOrEmpty(originalName)) return "Unnamed";

            // 替换特殊字符为下划线
            var validName = originalName
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(".", "_")
                .Replace("-", "_")
                .Replace(" ", "_")
                .Replace(":", "_")
                .Replace("*", "_")
                .Replace("?", "_");

            // 分割为单词并转为驼峰
            var words = validName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word)) continue;
                // 首字母大写
                if (word.Length == 1)
                {
                    // 单字符直接转大写
                    sb.Append(char.ToUpper(word[0]));
                }
                else
                {
                    // 首字符大写 + 剩余字符原样拼接
                    sb.Append(char.ToUpper(word[0])).Append(word.AsSpan(1));
                }
            }

            validName = sb.ToString();

            // 确保以字母开头（如果以数字/特殊字符开头，添加前缀）
            if (validName.Length > 0 && !char.IsLetter(validName[0]))
            {
                validName = $"Key{validName}";
            }

            // 兜底：如果处理后为空则返回默认值
            return string.IsNullOrEmpty(validName) ? "Unnamed" : validName;
        }
    }
}