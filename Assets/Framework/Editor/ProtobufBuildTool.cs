using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace BH.Framework.Editor
{
    public static class ProtobufBuildTool
    {
        // protoc 编译器的路径 (如果未加入环境变量，请填写完整路径)
        private const string
            PROTOC_PATH =
                @"E:\Unity\BH6\Assets\Plugins\protobuf\protobuf\bin\protoc.exe"; // 如果加入了环境变量，直接写 "protoc"
        // private const string PROTOC_PATH = "D:/protoc/bin/protoc.exe"; // Windows 示例，填写您的实际路径

        // .proto 文件存放的根目录
        private const string PROTO_DEFINITIONS_DIR = "Assets/ProtobufFile/ProtoDefinitions/";

        // 生成的 C# 代码存放的目录
        private const string GENERATED_CS_DIR = "Assets/ProtobufFile/Generated/";

        [MenuItem("Tools/Protobuf/Compile All")]
        public static void CompileAllProtos()
        {
            if (!Directory.Exists(PROTO_DEFINITIONS_DIR))
            {
                UnityEngine.Debug.LogError($"Proto 定义目录不存在: {PROTO_DEFINITIONS_DIR}");
                return;
            }

            // 确保生成目录存在
            Directory.CreateDirectory(GENERATED_CS_DIR);

            // 获取所有 .proto 文件
            var protoFiles = Directory.GetFiles(PROTO_DEFINITIONS_DIR, "*.proto", SearchOption.AllDirectories);

            foreach (string protoFile in protoFiles)
            {
                CompileProto(protoFile);
            }

            // 刷新 Unity 资源，使新生成的脚本生效
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("所有 .proto 文件编译完成。");
        }

        private static void CompileProto(string protoFilePath)
        {
            try
            {
                var args =
                    $"--csharp_out=\"{GENERATED_CS_DIR}\" --proto_path=\"{PROTO_DEFINITIONS_DIR}\" \"{protoFilePath}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = PROTOC_PATH,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"编译失败: {protoFilePath}\n{error}");
                }
                else
                {
                    UnityEngine.Debug.Log($"编译成功: {Path.GetFileName(protoFilePath)}");
                    if (!string.IsNullOrEmpty(output))
                    {
                        UnityEngine.Debug.Log(output);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"启动 protoc 编译器时发生异常: {e.Message}");
            }
        }
    }
}