using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Extended;
using static RailNet.Clients.Ecos.Basic.StaticIds;
using static RailNet.Clients.Ecos.Basic.BefehlStrings;

namespace RailNet.Clients.Ecos.Tests.Extended
{
    [TestFixture]
    public class EcosManagerTests
    {
        private EcosManager subject;
        private Mock<IBasicClient> clientMock;
        private Subject<BasicEvent> eventObservable;


        [SetUp]
        public void SetUp()
        {
            eventObservable = new Subject<BasicEvent>();
            clientMock = new Mock<IBasicClient>();
            clientMock.Setup(x => x.EventObservable).Returns(eventObservable);

            subject = new EcosManager(clientMock.Object);
        }

        [Test]
        public async Task SetzeGo()
        {
            clientMock.Setup(x => x.Set(StaticIds.EcosId, "go"))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY set(1, go)>", "<END 0 (OK)>"}));

            await subject.Go();

            clientMock.Verify(x => x.Set(StaticIds.EcosId, "go"), Times.Exactly(1));
        }

        [Test]
        public async Task SetzeStop()
        {
            clientMock.Setup(x => x.Set(StaticIds.EcosId, "stop"))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY set(1, stop)>", "<END 0 (OK)>"}));

            await subject.Stop();

            clientMock.Verify(x => x.Set(StaticIds.EcosId, "stop"), Times.Exactly(1));
        }

        [Test]
        public async Task GetStatus()
        {
            clientMock.Setup(x => x.Get(StaticIds.EcosId, "status"))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY get(1, status)>", "1 status[GO]", "<END 0 (OK)>"}));

            var result = await subject.GetStatus();

            clientMock.Verify(x => x.Get(StaticIds.EcosId, "status"));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task UpdateInfo()
        {
            clientMock.Setup(x => x.Get(EcosId, "info"))
                .ReturnsAsync(new BasicResponse(new[]
                {
                    "<REPLY get(1, info)>",
                    "1 ECoS",
                    "1 ProtocolVersion[0.2]",
                    "1 ApplicationVersion[4.0.2]",
                    "1 HardwareVersion[2.0]",
                    "<END 0 (OK)>"
                }));

            var result = await subject.UpdateInfo();

            clientMock.Verify(x => x.Get(EcosId, "info"));
            Assert.That(result, Is.True);

            Assert.That(subject.ApplicationVersion, Is.EqualTo("4.0.2"));
            Assert.That(subject.ProtocolVersion, Is.EqualTo("0.2"));
            Assert.That(subject.HardwareVersion, Is.EqualTo("2.0"));
        }


        [Test]
        public async Task SubscribeUnsubscribe()
        {
            clientMock.Setup(x => x.Request(StaticIds.EcosId, "view", false))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY request(1, view)>", "<END 0 (OK)>"}));

            clientMock.Setup(x => x.Release(StaticIds.EcosId, "view"))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY release(1, view)>", "<END 0 (OK)>"}));

            await subject.SubscribeToStatus();

            bool falseSet = false, trueSet = false;

            subject.StatusObservable.Subscribe(x =>
            {
                if (x)
                    trueSet = true;
                else
                    falseSet = true;
            });

            eventObservable.OnNext(new BasicEvent(new[] {"<EVENT 1>", "1 status[GO]", "<END 0 (OK)>"}));

            Assert.True(trueSet);
            Assert.False(falseSet);
            trueSet = false;

            eventObservable.OnNext(new BasicEvent(new[] {"<EVENT 1>", "1 status[STOP]", "<END 0 (OK)>"}));

            Assert.False(trueSet);
            Assert.True(falseSet);
            falseSet = false;

            //Ungueltige Message  
            eventObservable.OnNext(new BasicEvent(new[] {"<EVENT 1>", "1 toeff[GO]", "<END 0 (OK)>"}));

            Assert.False(trueSet);
            Assert.False(falseSet);

            eventObservable.OnNext(new BasicEvent(new[] {"<EVENT 1>", "1 status[STOP]", "<END 0 (OK)>"}));

            Assert.False(trueSet);
            Assert.True(falseSet);
            falseSet = false;


            await subject.UnsubscribeFromStatus();


            eventObservable.OnNext(new BasicEvent(new[] {"<EVENT 1>", "1 toeff[GO]", "<END 0 (OK)>"}));

            Assert.False(trueSet);
            Assert.False(falseSet);
        }
    }
}
