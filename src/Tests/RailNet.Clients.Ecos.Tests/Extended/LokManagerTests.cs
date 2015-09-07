using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Extended;
using RailNet.Clients.Ecos.Extended.Lok;

namespace RailNet.Clients.Ecos.Tests.Extended
{
    [TestFixture]
    public class LokManagerTests
    {
        private LokManager subject;
        private Mock<IBasicClient> clientMock;
        private Subject<BasicEvent> eventObservable;


        [SetUp]
        public void SetUp()
        {
            eventObservable = new Subject<BasicEvent>();
            clientMock = new Mock<IBasicClient>();
            clientMock.Setup(x => x.EventObservable).Returns(eventObservable);

            subject = new LokManager(clientMock.Object);
        }

        [Test]
        public async Task QueryAll()
        {
            clientMock.Setup(x => x.QueryObjects(StaticIds.LokManagerId, "name", "addr", "protocol"))
                .ReturnsAsync(new BasicResponse(new[]
                {
                    "<REPLY queryObjects(10, name, addr, protocol)>",
                    "1000 name[\"ET 420\"] addr[5] protocol[MFX]",
                    "1001 name[\"ET 421\"] addr[6] protocol[DCC14]",
                    "1002 name[\"ET 422\"] addr[1] protocol[DCC128]",
                    "1003 name[\"ET 423\"] addr[90] protocol[MM14]",
                    "1004 name[\"ET 424\"] addr[55] protocol[DCC28]",
                    "1005 name[\"ET 425\"] addr[8] protocol[MFX]",
                    "<END 0 (OK)>"
                }));

            await subject.QueryAll();

            Assert.That(subject.Loks.Count, Is.EqualTo(6));

            Assert.That(subject.Loks[1000].Id, Is.EqualTo(1000));
            Assert.That(subject.Loks[1000].SpeedSteps, Is.EqualTo(127));
            Assert.That(subject.Loks[1000].Name, Is.EqualTo("ET 420"));

            Assert.That(subject.Loks[1001].Id, Is.EqualTo(1001));
            Assert.That(subject.Loks[1001].SpeedSteps, Is.EqualTo(14));
            Assert.That(subject.Loks[1001].Name, Is.EqualTo("ET 421"));

            Assert.That(subject.Loks[1002].Id, Is.EqualTo(1002));
            Assert.That(subject.Loks[1002].SpeedSteps, Is.EqualTo(128));
            Assert.That(subject.Loks[1002].Name, Is.EqualTo("ET 422"));

            Assert.That(subject.Loks[1003].Id, Is.EqualTo(1003));
            Assert.That(subject.Loks[1003].SpeedSteps, Is.EqualTo(14));
            Assert.That(subject.Loks[1003].Name, Is.EqualTo("ET 423"));

            Assert.That(subject.Loks[1004].Id, Is.EqualTo(1004));
            Assert.That(subject.Loks[1004].SpeedSteps, Is.EqualTo(28));
            Assert.That(subject.Loks[1004].Name, Is.EqualTo("ET 424"));

            Assert.That(subject.Loks[1005].Id, Is.EqualTo(1005));
            Assert.That(subject.Loks[1005].SpeedSteps, Is.EqualTo(127));
            Assert.That(subject.Loks[1005].Name, Is.EqualTo("ET 425"));
            //Assert.That(subject.Loks[1000].Adress, Is.EqualTo(1000));
        }
    }
}
