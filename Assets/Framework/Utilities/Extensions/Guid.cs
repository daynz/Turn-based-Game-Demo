using UnityEngine;

namespace BH.Framework.Utilities.Extensions
{
    /// <summary>
    /// 整数型GUID生成器
    /// </summary>
    public static class Guid
    {
        // 静态计数器，存储当前最新的ID值
        private static int _currentId = 0;
    
        // 线程锁对象，保证多线程环境下ID生成的唯一性
        private static readonly object LockObj = new object();

        /// <summary>
        /// 生成一个新的唯一int类型ID
        /// </summary>
        /// <returns>唯一的int类型ID</returns>
        public static int GenerateId()
        {
            // 加锁保证线程安全（即使在协程/多线程中调用也不会重复）
            lock (LockObj)
            {
                // 先自增再返回，避免从0开始（也可以根据需求改为返回后自增）
                _currentId++;
                return _currentId;
            }
        }

        /// <summary>
        /// 获取当前最新的ID值（仅用于查看，不生成新ID）
        /// </summary>
        /// <returns>当前最大的ID值</returns>
        public static int GetCurrentMaxId()
        {
            lock (LockObj)
            {
                return _currentId;
            }
        }

        /// <summary>
        /// 重置ID计数器（仅建议在测试/初始化场景使用）
        /// </summary>
        /// <param name="newStartId">重置后的起始ID，默认0</param>
        public static void ResetCounter(int newStartId = 0)
        {
            lock (LockObj)
            {
                _currentId = newStartId;
                Debug.Log($"Guid计数器已重置，新起始ID: {newStartId}");
            }
        }
    }
}