namespace BH.Framework.Infrastructure.Command
{
    /// <summary>
    /// 命令模式的通用接口。
    /// 定义了所有命令对象必须实现的基本方法。
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 获取命令的唯一标识符，用于反序列化和网络传输。
        /// </summary>
        int CommandId { get; }

        /// <summary>
        /// 执行此命令。
        /// </summary>
        void Execute();

        /// <summary>
        /// 撤销此命令的效果。
        /// 如果命令不支持撤销，则此方法为空实现。
        /// </summary>
        void Undo();

        /// <summary>
        /// 将命令的数据序列化为字节数组。
        /// 用于网络传输或持久化存储。
        /// </summary>
        /// <returns>序列化后的字节数组。</returns>
        byte[] Serialize();

        /// <summary>
        /// 从字节数组反序列化命令数据。
        /// 注意：此方法通常用于在反序列化时重建命令对象的内部状态，
        /// 因此它会修改当前对象的属性，而不是返回一个新对象。
        /// <param name="data">包含命令数据的字节数组。</param>
        /// </summary>
        void Deserialize(byte[] data);
    }
}