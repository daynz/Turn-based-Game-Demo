using System;
using System.IO;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.DI.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using Google.Protobuf;

namespace BH.Framework.Services
{
    [Serializable]
    [AutoRegisterService]
    public class ProtobufService : IService
    {
        public int Priority => (int)PriorityOrder.ProtobufService;
        public string Name => GetType().Name;
        public bool IsInitialized { get; private set; }

        [field: Inject] private LogService LogService { get; set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// 从字节数组反序列化一个 Protobuf 消息对象。
        /// </summary>
        /// <typeparam name="T">目标 Protobuf 消息类型，必须实现 IMessage&lt;T&gt; 接口。</typeparam>
        /// <param name="data">包含 Protobuf 数据的字节数组。</param>
        /// <returns>反序列化后的消息对象；如果失败则返回 null。</returns>
        public T DeserializeFromBytes<T>(byte[] data) where T : IMessage<T>, new()
        {
            if (data == null || data.Length == 0)
            {
                LogService.Error("输入的字节数组为空或长度为0，无法反序列化。", Name);
                return default;
            }

            try
            {
                var message = new T();
                using (var stream = new MemoryStream(data))
                {
                    message.MergeFrom(stream.ToArray());
                }

                LogService.Info($"成功从字节数组反序列化 Protobuf 消息: {typeof(T).Name}", Name);
                return message;
            }
            catch (InvalidProtocolBufferException ex)
            {
                LogService.Error($"反序列化 Protobuf 消息时发生错误: {typeof(T).Name}\n{ex.Message}\n{ex.StackTrace}", Name);
                return default;
            }
            catch (Exception ex)
            {
                LogService.Error($"反序列化 Protobuf 消息时发生未知错误: {typeof(T).Name}\n{ex.Message}\n{ex.StackTrace}", Name);
                return default;
            }
        }

        /// <summary>
        /// 将一个 Protobuf 消息对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的 Protobuf 消息类型。</typeparam>
        /// <param name="message">要序列化的消息对象。</param>
        /// <returns>序列化后的字节数组；如果失败则返回 null。</returns>
        public byte[] SerializeToBytes<T>(T message) where T : IMessage<T>
        {
            if (message == null)
            {
                LogService.Error("输入的消息对象为 null，无法序列化。", Name);
                return null;
            }

            try
            {
                using var output = new MemoryStream();
                using var codedOutput = new CodedOutputStream(output.ToArray());
                message.WriteTo(codedOutput);
                codedOutput.Flush(); // 确保所有数据都写入底层流
                var data = output.ToArray();
                LogService.Info($"成功将 Protobuf 消息序列化为字节数组: {typeof(T).Name}", Name);
                return data;
            }
            catch (Exception ex)
            {
                LogService.Error($"序列化 Protobuf 消息时发生错误: {typeof(T).Name}\n{ex.Message}\n{ex.StackTrace}", Name);
                return null;
            }
        }
    }
}