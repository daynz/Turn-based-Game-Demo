namespace BH.Framework.Infrastructure.Command
{
    /// <summary>
    /// 命令模式的抽象基类。
    /// 实现了 ICommand 接口的通用部分，并提供了默认的空实现。
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// 获取命令的唯一标识符。
        /// </summary>
        public abstract int CommandId { get; }

        /// <summary>
        /// 执行此命令。
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// 撤销此命令的效果。
        /// 默认实现为空，子类可根据需要重写此方法。
        /// </summary>
        public virtual void Undo()
        {
            // 默认不支持撤销
        }

        /// <summary>
        /// 将命令的数据序列化为字节数组。
        /// 子类必须实现此方法。
        /// </summary>
        /// <returns>序列化后的字节数组。</returns>
        public abstract byte[] Serialize();

        /// <summary>
        /// 从字节数组反序列化命令数据。
        /// 子类必须实现此方法。
        /// </returns>
        /// <param name="data">包含命令数据的字节数组。</param>
        public abstract void Deserialize(byte[] data);
    }
}