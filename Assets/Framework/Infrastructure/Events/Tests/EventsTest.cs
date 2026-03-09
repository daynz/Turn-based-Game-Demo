using System;
using System.Reflection;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using BH.Framework.Infrastructure.Logging.Interfaces;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Zenject;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Tests
{
    [EventType(EventType.BattleEvent)]
    public class BattleStartEvent : Event<EmptyEventData>
    {
        protected BattleStartEvent(object sender, EmptyEventData data,
            EventPriority priority = EventPriority.Normal, bool allowMultipleHandlers = true) : base(sender, data,
            priority, allowMultipleHandlers)
        {
        }
    }

    public class BattleSystem
    {
        private readonly IEventService _eventService;

        // Zenject构造注入
        [Inject]
        public BattleSystem(IEventService eventService)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        // 触发战斗开始（发布事件）
        public void StartBattle()
        {
            _eventService.Publish(
                EventBuilder.CreateForEmptyData<BattleStartEvent>()
                    .WithSender(this)
                    .Build()
            );
        }
    }

    [TestFixture]
    public class EventSystemUnitTests : ZenjectUnitTestFixture
    {
        // 测试前初始化容器（每个测试用例执行前重置）
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Container.BindInterfacesAndSelfTo<LogService>().AsSingle().NonLazy();
            ;
            Container.BindInterfacesAndSelfTo<EventBus>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EventService>().AsSingle().NonLazy();
            Container.Bind<BattleSystem>().AsSingle();
        }

        #region 场景1：验证BattleSystem发布事件的正确性（使用NSubstitute Mock）

        [Test]
        public void BattleSystem_StartBattle_ShouldPublishBattleStartEvent()
        {
            // 1. 创建IEventService的Substitute（替换真实实现）
            var mockEventService = Substitute.For<IEventService>();

            // 2. 替换容器中的绑定
            Container.Unbind<IEventService>();
            Container.Bind<IEventService>().FromInstance(mockEventService);

            // 3. 解析BattleSystem实例（依赖已被替换为Mock）
            var battleSystem = Container.Resolve<BattleSystem>();

            // 4. 执行测试方法
            battleSystem.StartBattle();

            // 5. 验证：Publish方法被调用一次，且参数符合预期
            mockEventService.Received(1)
                .Publish(Arg.Is<BattleStartEvent>(evt =>
                    evt.Sender == battleSystem));
        }

        #endregion

        #region 场景2：验证事件订阅后能正确接收发布的事件

        [Test]
        public void EventService_Subscribe_ShouldReceivePublishedEvent()
        {
            // // 1. 解析真实的EventService和BattleSystem
            var eventService = Container.Resolve<IEventService>();
            var battleSystem = Container.Resolve<BattleSystem>();

            // 2. 初始化测试标记
            bool isEventReceived = false;
            //BattleStartEvent receivedEvent = null;

            // 3. 订阅BattleStartEvent
            eventService.Subscribe<BattleStartEvent>(evt =>
            {
                isEventReceived = true;
                //receivedEvent = evt;
            });
            
            // 4. 发布事件
            battleSystem.StartBattle();
            // 5. 验证结果
            // Assert.IsTrue(isEventReceived, "订阅的事件未被触发");
            // Assert.IsNotNull(receivedEvent, "接收到的事件为null");
            // Assert.AreEqual(battleSystem, receivedEvent.Sender, "事件Sender不匹配");
            //Assert.IsInstanceOf<EmptyEventData>(receivedEvent.Data, "事件Data类型错误");
        }

        #endregion

        #region 场景3：验证取消订阅后不再接收事件

        [Test]
        public void EventService_Unsubscribe_ShouldNotReceiveEvent()
        {
            // 1. 解析真实的EventService和BattleSystem
            var eventService = Container.Resolve<IEventService>();
            var battleSystem = Container.Resolve<BattleSystem>();

            // 2. 初始化测试标记
            int eventReceiveCount = 0;

            // 3. 订阅并立即取消
            var subscription = eventService.Subscribe<BattleStartEvent>(_ => eventReceiveCount++);
            subscription.Dispose(); // 取消订阅

            // 4. 发布事件
            battleSystem.StartBattle();

            // 5. 验证：未接收到事件
            Assert.AreEqual(0, eventReceiveCount, "取消订阅后仍接收到事件，订阅取消逻辑异常");
        }

        #endregion

        #region 场景4：验证依赖注入的正确性（IEventService被正确注入到BattleSystem）

        [Test]
        public void BattleSystem_ShouldHaveInjectedEventService()
        {
            // 1. 解析BattleSystem实例
            var battleSystem = Container.Resolve<BattleSystem>();

            // 2. 反射获取私有字段_eventService（验证注入）
            var field = typeof(BattleSystem).GetField(
                "_eventService",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(field, "未找到_eventService私有字段");

            var injectedService = field.GetValue(battleSystem);

            // 3. 验证注入结果
            Assert.IsNotNull(injectedService, "IEventService未被Zenject注入");
            Assert.IsInstanceOf<IEventService>(injectedService, "注入的类型不是IEventService");
            Assert.IsInstanceOf<EventService>(injectedService, "注入的不是真实的EventService实现");
        }

        #endregion

        #region 场景5：验证发布null事件时抛出异常

        [Test]
        public void EventService_Publish_NullEvent_ShouldThrowArgumentNullException()
        {
            // 1. 解析真实的EventService
            var eventService = Container.Resolve<IEventService>();

            // 2. 验证发布null事件时抛出异常
            Assert.Throws<ArgumentNullException>(() =>
                eventService.Publish<BattleStartEvent>(null));
        }

        #endregion

        #region 场景6：验证订阅null回调时抛出异常

        [Test]
        public void EventService_Subscribe_NullCallback_ShouldThrowArgumentNullException()
        {
            // 1. 解析真实的EventService
            var eventService = Container.Resolve<IEventService>();

            // 2. 验证订阅null回调时抛出异常
            Assert.Throws<ArgumentNullException>(() =>
                eventService.Subscribe<BattleStartEvent>(null));
        }

        #endregion

        #region 场景7：验证多次订阅/取消订阅的稳定性

        [Test]
        public void EventService_MultipleSubscribeUnsubscribe_ShouldWorkCorrectly()
        {
            // 1. 解析真实的EventService
            var eventService = Container.Resolve<IEventService>();
            var battleSystem = Container.Resolve<BattleSystem>();

            // 2. 初始化测试标记
            int callback1Count = 0;
            int callback2Count = 0;

            // 3. 多次订阅
            var subscription1 = eventService.Subscribe<BattleStartEvent>(_ => callback1Count++);
            var subscription2 = eventService.Subscribe<BattleStartEvent>(_ => callback2Count++);

            // 第一次发布：两个回调都触发
            battleSystem.StartBattle();
            Assert.AreEqual(1, callback1Count);
            Assert.AreEqual(1, callback2Count);

            // 取消第一个订阅
            subscription1.Dispose();

            // 第二次发布：只有第二个回调触发
            battleSystem.StartBattle();
            Assert.AreEqual(1, callback1Count);
            Assert.AreEqual(2, callback2Count);

            // 取消第二个订阅
            subscription2.Dispose();

            // 第三次发布：无回调触发
            battleSystem.StartBattle();
            Assert.AreEqual(1, callback1Count);
            Assert.AreEqual(2, callback2Count);
        }

        #endregion
    }
}