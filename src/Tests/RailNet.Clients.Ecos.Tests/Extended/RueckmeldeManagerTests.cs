using System.Linq;
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
    public class RueckmeldeManagerTests
    {
        private RueckmeldeManager subject;
        private Mock<IBasicClient> clientMock;
        private Subject<BasicEvent> eventObservable;

        [SetUp]
        public void SetUp()
        {
            eventObservable = new Subject<BasicEvent>();
            clientMock = new Mock<IBasicClient>();
            clientMock.Setup(x => x.EventObservable).Returns(eventObservable);

            subject = new RueckmeldeManager(clientMock.Object);
        }

        [Test]
        public async Task SubscribeOne()
        {
            clientMock.Setup(x => x.QueryObjects(FeedbackManagerId))
                .ReturnsAsync(new BasicResponse(new[] { "<REPLY queryObjects(26)>", "100", "<END 0 (OK)>" }));
            clientMock.Setup(x => x.Get(100, PortsS))
                .ReturnsAsync(new BasicResponse(new[] { "<REPLY get(100, ports)>", "100 ports[16]", "<END 0 (OK)>" }));
            clientMock.Setup(x => x.Request(100, ViewS, false))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY request(100, view)>", "<END 0 (OK)>"}));

            await subject.SubscribeAll();

            clientMock.Verify(x => x.QueryObjects(FeedbackManagerId), Times.Exactly(1));
            clientMock.Verify(x => x.Get(100, PortsS), Times.Exactly(1));
            clientMock.Verify(x => x.Request(100, ViewS, false), Times.Exactly(1));

            Assert.That(subject.Module.Count, Is.EqualTo(1));
            Assert.That(subject.Module.First().Value.Ports, Is.EqualTo(16));
            Assert.That(subject.Module.First().Value.Id, Is.EqualTo(100));
            Assert.That(subject.Module.Keys.First(), Is.EqualTo(100));
        }

        [Test]
        public async Task SubscribeMore()
        {
            clientMock.Setup(x => x.QueryObjects(FeedbackManagerId))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY queryObjects(26)>", "100", "101", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Get(100, PortsS))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY get(100, ports)>", "100 ports[16]", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Request(100, ViewS, false))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY request(100, view)>", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Get(101, PortsS))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY get(101, ports)>", "101 ports[8]", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Request(101, ViewS, false))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY request(101, view)>", "<END 0 (OK)>"}));

            await subject.SubscribeAll();

            clientMock.Verify(x => x.QueryObjects(FeedbackManagerId), Times.Exactly(1));
            clientMock.Verify(x => x.Get(100, PortsS), Times.Exactly(1));
            clientMock.Verify(x => x.Request(100, ViewS, false), Times.Exactly(1));
            clientMock.Verify(x => x.Get(101, PortsS), Times.Exactly(1));
            clientMock.Verify(x => x.Request(101, ViewS, false), Times.Exactly(1));

            Assert.That(subject.Module.Count, Is.EqualTo(2));

            Assert.That(subject.Module.First().Value.Ports, Is.EqualTo(16));
            Assert.That(subject.Module.First().Value.Id, Is.EqualTo(100));
            Assert.That(subject.Module.Keys.First(), Is.EqualTo(100));

            Assert.That(subject.Module.Last().Value.Ports, Is.EqualTo(8));
            Assert.That(subject.Module.Last().Value.Id, Is.EqualTo(101));
            Assert.That(subject.Module.Keys.Last(), Is.EqualTo(101));
        }

        [Test]
        public async Task Unsubscribe()
        {
            subject.Module.Add(100, new RueckmeldeModul(100, 16));
            subject.Module.Add(101, new RueckmeldeModul(101, 8));
            subject.Module.Add(102, new RueckmeldeModul(102, 16));

            clientMock.Setup(x => x.Release(100, ViewS))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY release(100, view)>", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Release(101, ViewS))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY release(101, view)>", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Release(102, ViewS))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY release(102, view)>", "<END 0 (OK)>"}));

            await subject.UnsubscribeAll();

            clientMock.Verify(x => x.Release(100, ViewS), Times.Exactly(1));
            clientMock.Verify(x => x.Release(101, ViewS), Times.Exactly(1));
            clientMock.Verify(x => x.Release(102, ViewS), Times.Exactly(1));

            Assert.That(subject.Module.Count, Is.EqualTo(0));
        }

        [Test]
        public void SetState()
        {
            var modul = new RueckmeldeModul(100, 16);

            subject.Module.Add(100, modul);

            eventObservable.OnNext(new BasicEvent(new[] {"<EVENT 100>", "100 state[0x0]", "<END 0 (OK)>"}));
            Assert.That(GetBelegtstatusOfModul(modul), Is.EqualTo("0000000000000000"));
            
            eventObservable.OnNext(new BasicEvent(new[] { "<EVENT 100>", "100 state[0x10]", "<END 0 (OK)>" }));
            Assert.That(GetBelegtstatusOfModul(modul), Is.EqualTo("0000100000000000"));

            eventObservable.OnNext(new BasicEvent(new[] { "<EVENT 100>", "100 state[0x861D]", "<END 0 (OK)>" }));
            Assert.That(GetBelegtstatusOfModul(modul), Is.EqualTo("1011100001100001"));

            eventObservable.OnNext(new BasicEvent(new[] { "<EVENT 100>", "100 state[0xFFFF]", "<END 0 (OK)>" }));
            Assert.That(GetBelegtstatusOfModul(modul), Is.EqualTo("1111111111111111"));
        }

        private static string GetBelegtstatusOfModul(RueckmeldeModul modul)
        {
            return modul.Rueckmelder.Aggregate("", (current, rueckmelder) => current + (rueckmelder.Belegt ? "1" : "0"));
        }
    }
}
