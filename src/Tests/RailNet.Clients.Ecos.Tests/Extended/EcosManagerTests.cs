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


        [SetUp]
        public void SetUp()
        {
            clientMock = new Mock<IBasicClient>();
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
                .ReturnsAsync(new BasicResponse(new[] {
                    "<REPLY get(1, info)>",
                    "1 ECoS",
                    "1 ProtocolVersion[0.2]",
                    "1 ApplicationVersion[4.0.2]",
                    "1 HardwareVersion[2.0]",
                    "<END 0 (OK)>" }));

            var result = await subject.UpdateInfo();

            clientMock.Verify(x => x.Get(EcosId, "info"));
            Assert.That(result, Is.True);

            Assert.That(subject.ApplicationVersion, Is.EqualTo("4.0.2"));
            Assert.That(subject.ProtocolVersion, Is.EqualTo("0.2"));
            Assert.That(subject.HardwareVersion, Is.EqualTo("2.0"));
        }
    }
}
