using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Extended;
using RailNet.Core;
using static RailNet.Clients.Ecos.Basic.StaticIds;
using static RailNet.Clients.Ecos.Basic.BefehlStrings;

namespace RailNet.Clients.Ecos.Tests.Extended
{
    [TestFixture]
    public class RueckmeldeManagerTests
    {
        private RueckmeldeManager subject;
        private Mock<IBasicClient> clientMock;


        [SetUp]
        public void SetUp()
        {
            clientMock = new Mock<IBasicClient>();
            subject = new RueckmeldeManager(clientMock.Object);
        }

        [Test]
        public async Task SubscribeAll()
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
    }
}
