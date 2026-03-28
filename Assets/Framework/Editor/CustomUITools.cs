using System.IO;
using UnityEditor;
using UnityEngine;

namespace BH.Framework.Editor
{
    public class CustomUITools : EditorWindow
    {
        private string _uiFileTarget = "E:/Unity/BH6/Assets/UI/Scripts/Module";
        private string _panelName;

        [MenuItem("Tools/UITools/添加UI面板代码")]
        private static void ShowWindow()
        {
            // 创建并显示窗口
            GetWindow<CustomUITools>("UI面板代码生成");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            // 标题提示
            GUILayout.Label("UI面板代码生成", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _panelName = EditorGUILayout.TextField("面板名称", _panelName);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            _uiFileTarget = EditorGUILayout.TextField("文件夹路径", _uiFileTarget);

            // 选择文件夹按钮
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                SelectFolder();
            }

            // 清空按钮
            if (GUILayout.Button("清空", GUILayout.Width(60)))
            {
                _uiFileTarget = "";
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            // 执行按钮
            using (new EditorGUI.DisabledScope(_uiFileTarget == null))
            {
                if (GUILayout.Button("生成代码文件", GUILayout.Height(30)))
                {
                    GenerateUICodeStructure();
                }
            }

            // 提示信息
            if (!string.IsNullOrEmpty(_uiFileTarget))
            {
                EditorGUILayout.HelpBox("已选择：\n" + _uiFileTarget, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("请先选择文件夹", MessageType.Warning);
            }
        }

        /// <summary>
        /// 打开文件夹选择面板
        /// </summary>
        private void SelectFolder()
        {
            // 默认打开路径：如果已有路径则用它，没有则打开 Assets 目录
            var defaultPath = string.IsNullOrEmpty(_uiFileTarget) ? Application.dataPath : _uiFileTarget;

            // 打开系统文件夹选择面板
            var selectPath = EditorUtility.SaveFolderPanel(
                "请选择文件夹", // 标题
                defaultPath, // 默认打开路径
                "" // 固定为空（选择文件夹不需要文件名）
            );

            // 如果用户选择了路径（不是取消）
            if (!string.IsNullOrEmpty(selectPath))
            {
                _uiFileTarget = selectPath;

                // 可选：将系统绝对路径转为 Unity 相对路径（如 D:/Project/Assets → Assets/）
                // _selectFolderPath = GetRelativePath(selectPath);
            }
        }

        // 生成UI预制代码
        private void GenerateUICodeStructure()
        {
            CreateAllScriptTemplates();

            EditorUtility.DisplayDialog("生成完成", "文件夹 + 代码模板已全部生成！", "确认");

            // 刷新编辑器
            AssetDatabase.Refresh();
        }

        private void CreateAllScriptTemplates()
        {
            CreateScriptToFile(
                _uiFileTarget + $"/{_panelName}/Flow/{_panelName}Flow.cs",
                GetFlowTemplate()
            );

            CreateScriptToFile(
                _uiFileTarget + $"/{_panelName}/Installer/{_panelName}Installer.cs",
                GetInstallerTemplate()
            );

            CreateScriptToFile(
                _uiFileTarget + $"/{_panelName}/Model/{_panelName}Model.cs",
                GetModelTemplate()
            );

            CreateScriptToFile(
                _uiFileTarget + $"/{_panelName}/View/{_panelName}View.cs",
                GetViewTemplate()
            );

            CreateScriptToFile(
                _uiFileTarget + $"/{_panelName}/ViewModel/{_panelName}ViewModel.cs",
                GetViewModelTemplate()
            );
        }

        private static void CreateScriptToFile(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                Debug.Log($"<color=yellow>已存在，跳过：</color> {path}");
                return;
            }

            File.WriteAllText(path, content);
            Debug.Log($"<color=cyan>生成脚本：</color> {path}");
        }

        private string GetFlowTemplate()
        {
            return @$"using BH.UI.Scripts.Managers;
using BH.UI.Scripts.Module.{_panelName}.Model;
using JetBrains.Annotations;
using Zenject;

namespace BH.UI.Scripts.Module.{_panelName}.Flow
{{
    /// <summary>
    /// 流程控制
    /// </summary>
    [UsedImplicitly]
    public class {_panelName}Flow : IInitializable
    {{
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly {_panelName}Model _model;
        public void Initialize()
        {{
            
        }}
    }}
}}
";
        }

        private string GetInstallerTemplate()
        {
            return @$"using BH.UI.Scripts.Module.{_panelName}.Flow;
using BH.UI.Scripts.Module.{_panelName}.Model;
using BH.UI.Scripts.Module.{_panelName}.ViewModel;
using Zenject;

namespace BH.UI.Scripts.Module.{_panelName}.Installer
{{
    public class {_panelName}Installer : MonoInstaller
    {{
        public override void InstallBindings()
        {{
            // 绑定Model
            Container.BindInterfacesAndSelfTo<{_panelName}Model>().AsSingle().NonLazy();

            // 绑定ViewModel
            Container.BindInterfacesAndSelfTo<{_panelName}ViewModel>().AsSingle().NonLazy();

            // 绑定流程控制
            Container.BindInterfacesAndSelfTo<{_panelName}Flow>().AsSingle().NonLazy();
        }}
    }}
}}
";
        }

        private string GetModelTemplate()
        {
            return @$"using BH.UI.Scripts.Base;
using JetBrains.Annotations;

namespace BH.UI.Scripts.Module.{_panelName}.Model
{{
    [UsedImplicitly]
    public class {_panelName}Model : ModelBase
    {{
        
    }}
}}
";
        }

        private string GetViewTemplate()
        {
            return @$"using BH.UI.Scripts.Base;
using JetBrains.Annotations;

namespace BH.UI.Scripts.Module.{_panelName}.View
{{
    [UsedImplicitly]
    public class {_panelName}View : ViewBase
    {{
        protected override void BindUI()
        {{
            
        }}
    }}
}}
";
        }

        private string GetViewModelTemplate()
        {
            return @$"using BH.UI.Scripts.Base;
using JetBrains.Annotations;

namespace BH.UI.Scripts.Module.{_panelName}.ViewModel
{{
    [UsedImplicitly]
    public class {_panelName}ViewModel : ViewModelBase
    {{
        public override void Initialize()
        {{
            
        }}
    }}
}}
";
        }
    }
}