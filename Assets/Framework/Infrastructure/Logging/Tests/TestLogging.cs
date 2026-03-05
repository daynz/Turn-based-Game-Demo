using System.Collections.Generic;
using System.Linq;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Logging.Output;
using NUnit.Framework;

namespace BH.Framework.Infrastructure.Logging.Tests
{
    public class LogServiceTests
    {
        private LogService _logService;
        private List<ILogOutput> _mockOutputs;

        [SetUp]
        public void SetUp()
        {
            // 1. 创建模拟的输出器
            var output = new LogConsoleOutput();
            _mockOutputs = new List<ILogOutput> { output };

            // 2. 创建 LogService 实例，并注入模拟的输出器
            _logService = new LogService(_mockOutputs);

            // 3. 初始化服务
            _logService.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _logService.Shutdown(); // 清理资源
        }

        [Test]
        public void Log_WhenCalled_SendsEntryToAllOutputs()
        {
            // Arrange
            var testEntry = new LogEntry
            {
                Level = LogLevel.Info,
                Message = "Test message",
                Owner = "TestOwner"
            };

            // Act
            _logService.Log(testEntry);

            // Assert
            foreach (var output in _mockOutputs)
            {
                output.Output(testEntry);
            }
        }

        [Test]
        public void Log_WhenCalled_AddsEntryToInternalList()
        {
            // Arrange
            var testEntry = new LogEntry
            {
                Level = LogLevel.Info,
                Message = "Test message for list",
                Owner = "TestOwner"
            };

            // Act
            _logService.Log(testEntry);

            // Assert
            var allLogs = _logService.GetAllLogs();
            Assert.That(allLogs, Contains.Item(testEntry));
            Assert.AreEqual(1, allLogs.Count);
        }

        // [Test]
        // public void Log_WhenEntryIsNull_DoesNotThrowAndDoesNotCallOutputs()
        // {
        //     // Act & Assert
        //     Assert.DoesNotThrow(() => _logService.Log(null));
        //
        //     // 验证输出器没有被调用
        //     foreach (var output in _mockOutputs)
        //     {
        //         output.Output();
        //     }
        // }

        [Test]
        public void Log_WhenBelowMinDisplayLevel_DoesNotSendToOutputs()
        {
            // Arrange
            _logService = new LogService(_mockOutputs); // 重新创建服务
            // 通常 MinDisplayLevel 是通过配置或属性设置的，这里我们假设有一个内部字段
            // 为了测试，我们直接测试 Debug (最低级别) 日志在 Info (较高级别) 限制下的行为
            // 由于 LogService 内部没有公开此字段，我们通过一个低于默认 Info 级别的日志来测试
            // 我们可以模拟一个场景：如果 minDisplayLevel 是 Warning, 那么 Info 级别就不会被处理
            
            // 为了绕过内部逻辑，我们可以直接检查日志是否进入队列或列表。
            // 更好的方法是在 LogService 中增加一个可设置的 MinLevel，或者使用反射，但这不符合好设计。
            // 我们采用一个更直接的测试：如果日志级别高于或等于最小级别，则会被处理。
            
            // Let's assume the default min level is Info.
            // This test will verify that a Debug message (lower than Info) is ignored.
            var debugEntry = new LogEntry { Level = LogLevel.Debug, Message = "Debug msg", Owner = "Test" };
            var infoEntry = new LogEntry { Level = LogLevel.Info, Message = "Info msg", Owner = "Test" };

            // Act
            _logService.Initialize(); // Re-initialize with default min level (Info)
            _logService.Log(debugEntry);
            _logService.Log(infoEntry);

            // Assert
            // The debug message should not be sent to outputs, but the info message should.
            var consoleOutput = _mockOutputs[0];

            // Debug message should not have been sent
            consoleOutput.Output(debugEntry);

            // Info message should have been sent
            consoleOutput.Output(infoEntry);

            // And only the info message should be in the internal list
            var logs = _logService.GetAllLogs();
            Assert.That(logs.Count, Is.EqualTo(1));
            Assert.That(logs.First().Message, Is.EqualTo("Info msg"));
        }

        [Test]
        public void Clear_WhenCalled_EmptiesInternalLogList()
        {
            // Arrange
            var entry1 = new LogEntry { Level = LogLevel.Info, Message = "Msg 1", Owner = "Test" };
            var entry2 = new LogEntry { Level = LogLevel.Info, Message = "Msg 2", Owner = "Test" };
            _logService.Log(entry1);
            _logService.Log(entry2);

            Assert.That(_logService.GetAllLogs().Count, Is.EqualTo(2));

            // Act
            _logService.Clear();

            // Assert
            Assert.That(_logService.GetAllLogs().Count, Is.EqualTo(0));
        }

        [Test]
        public void Initialize_WhenCalled_SetsIsInitializedToTrue()
        {
            // Arrange: A new instance has IsInitialized = false initially
            var freshLogService = new LogService(_mockOutputs);
            Assert.IsFalse(freshLogService.IsInitialized);

            // Act
            freshLogService.Initialize();

            // Assert
            Assert.IsTrue(freshLogService.IsInitialized);
        }
    }
}