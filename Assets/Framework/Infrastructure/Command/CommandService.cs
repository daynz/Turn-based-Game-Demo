using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Logging.Core;
using UnityEngine;

namespace BH.Framework.Infrastructure.Command
{
    /// <summary>
    /// 命令管理器。
    /// 统一管理所有命令的调度、执行、撤销和重做。
    /// </summary>
    [Serializable]
    //[AutoRegisterService]
    public class CommandService
    {
        #region 【成员变量】

        public string Name => GetType().Name;

        // 待执行的命令队列
        private readonly Queue<ICommand> _pendingCommands = new();

        // 已执行的命令栈，用于实现撤销功能
        private readonly Stack<ICommand> _executedCommands = new();

        // 最大可撤销步数限制
        [SerializeField] private int maxUndoSteps = 50;

        // 用于避免在执行命令时意外添加新命令导致的死循环
        private bool _isExecuting;

        [SerializeField] private int priority;
        private LogService _logService;

        #endregion

        public int Priority => priority;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        #region 【公共接口】

        /// <summary>
        /// 添加一个命令到待执行队列。
        /// </summary>
        /// <param name="command">待执行的命令。</param>
        public void AddCommand(ICommand command)
        {
            if (command == null) return;

            _pendingCommands.Enqueue(command);
            _logService.Info($"命令已添加至队列: {command.GetType().Name}", Name);
        }

        /// <summary>
        /// 立即执行一个命令，并将其放入已执行栈中。
        /// </summary>
        /// <param name="command">要立即执行的命令。</param>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null) return;

            command.Execute();
            PushExecutedCommand(command);
        }

        /// <summary>
        /// 执行队列中的所有待处理命令。
        /// 通常在每帧的固定更新或回合开始时调用。
        /// </summary>
        public void ProcessPendingCommands()
        {
            if (_isExecuting) return; // 防止在执行过程中递归调用

            _isExecuting = true;
            while (_pendingCommands.Count > 0)
            {
                var command = _pendingCommands.Dequeue();
                command.Execute();
                PushExecutedCommand(command);
            }

            _isExecuting = false;
        }

        /// <summary>
        /// 撤销上一次执行的命令。
        /// </summary>
        public bool TryUndo()
        {
            if (_executedCommands.Count == 0) return false;

            var command = _executedCommands.Pop();
            command.Undo();
            _logService.Info($"已撤销命令: {command.GetType().Name}", Name);
            return true;
        }

        /// <summary>
        /// 重做刚刚撤销的命令。
        /// 注意：当前实现较为简单，实际项目中可能需要一个单独的“撤销栈”来支持重做。
        /// 为了支持重做，可以维护一个“已撤销命令栈”。
        /// </summary>
        private Stack<ICommand> _undoneCommands = new Stack<ICommand>();

        public bool TryRedo()
        {
            if (_undoneCommands.Count == 0) return false;

            var command = _undoneCommands.Pop();
            command.Execute();
            _executedCommands.Push(command); // 重做后，该命令又回到了执行栈
            _logService.Info($"已重做命令: {command.GetType().Name}", Name);
            return true;
        }

        /// <summary>
        /// 清空所有待处理和已执行的命令。
        /// 通常在场景切换或战斗结束时调用。
        /// </summary>
        public void ClearAllCommands()
        {
            _pendingCommands.Clear();
            _executedCommands.Clear();
            _undoneCommands.Clear();
            _logService.Info("所有命令已被清空。", Name);
        }

        public void Shutdown()
        {
            ClearAllCommands();
        }

        #endregion

        #region 【私有辅助方法】

        /// <summary>
        /// 将已执行的命令压入执行栈。
        /// 同时处理撤销步数限制和为重做做准备。
        /// </summary>
        /// <param name="command">刚执行完的命令。</param>
        private void PushExecutedCommand(ICommand command)
        {
            // 如果支持重做，先清空“已撤销栈”，因为新的执行打断了重做链
            _undoneCommands.Clear();

            // 检查是否超过最大撤销步数
            if (_executedCommands.Count >= maxUndoSteps)
            {
                // 移除最旧的命令（可在此处进行对象池回收）
                _executedCommands.Pop();
            }

            _executedCommands.Push(command);
        }

        #endregion
    }
}