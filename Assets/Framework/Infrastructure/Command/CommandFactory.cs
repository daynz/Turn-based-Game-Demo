using System;
using System.Collections.Generic;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;

namespace BH.Framework.Infrastructure.Command
{
    /// <summary>
    /// 命令工厂类。
    /// 负责根据命令ID创建对应的命令实例。
    /// </summary>
    public static class CommandFactory
    {
        // 存储命令ID到创建函数的映射，便于动态注册和扩展
        private static readonly Dictionary<int, Func<ICommand>> CommandCreators = new();
        [Inject] private static LogService _logService;

        /// <summary>
        /// 静态构造函数，用于注册所有已知的命令类型。
        /// </summary>
        static CommandFactory()
        {
            // 在此处注册更多命令...
            //RegisterCommand<AttackCommand>();
            // RegisterCommand<MoveCommand>();
            // RegisterCommand<SkillCommand>();
        }

        /// <summary>
        /// 注册一个命令类型到工厂中。
        /// </summary>
        /// <typeparam name="T">实现了 ICommand 接口的命令类型。</typeparam>
        public static void RegisterCommand<T>() where T : ICommand, new()
        {
            var commandType = typeof(T);
            var instance = new T();
            var commandId = instance.CommandId;

            if (CommandCreators.ContainsKey(commandId))
            {
                _logService.Warning($"命令ID {commandId} 已存在，将被覆盖。", "CommandFactory");
            }

            CommandCreators[commandId] = () => new T();
            _logService.Info($"成功注册命令: {commandType.Name}, ID: {commandId}", "CommandFactory");
        }

        /// <summary>
        /// 根据命令ID创建命令实例。
        /// </summary>
        /// <param name="commandId">命令的唯一标识符。</param>
        /// <returns>创建的命令实例。</returns>
        /// <exception cref="ArgumentException">当找不到对应ID的命令时抛出。</exception>
        public static ICommand CreateCommand(int commandId)
        {
            if (CommandCreators.TryGetValue(commandId, out var creator))
            {
                return creator();
            }
            else
            {
                throw new ArgumentException($"无法找到ID为 {commandId} 的命令类型。");
            }
        }

        /// <summary>
        /// 从序列化的数据创建命令实例。
        /// 此方法首先创建命令对象，然后调用其反序列化方法。
        /// </summary>
        /// <param name="commandId">命令的唯一标识符。</param>
        /// <param name="data">序列化的命令数据。</param>
        /// <returns>反序列化后的命令实例。</returns>
        public static ICommand CreateCommandFromData(int commandId, byte[] data)
        {
            var command = CreateCommand(commandId);
            command.Deserialize(data);
            return command;
        }
    }
}