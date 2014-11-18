using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Extended;
using RailNet.Core;

namespace RailNet.Clients.Ecos.Tests.Extended
{
    [TestFixture]
    public class SchaltartikelManagerTests
    {
        private SchaltartikelManager subject;
        private Mock<IBasicClient> clientMock;


        [SetUp]
        public void SetUp()
        {
            clientMock = new Mock<IBasicClient>();
            subject = new SchaltartikelManager(clientMock.Object);
        }

        [Test]
        public async Task SetzeAdresseTest()
        {
            clientMock.Setup(x => x.Set(11, "switch", "DCC1g"))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY set(11, switch[DCC1g])>", "<END 0 (OK)>"}));
            clientMock.Setup(x => x.Set(11, "switch", "MOT1r"))
                .ReturnsAsync(new BasicResponse(new[] {"<REPLY set(11, switch[MOT1r])>", "<END 0 (OK)>"}));

            await subject.SetzeAdresse(1, true, Digitalsystem.DCC);
            clientMock.Verify(x => x.Set(11, "switch", "DCC1g"), Times.Exactly(1));

            await subject.SetzeAdresse(1, false, Digitalsystem.Motorola);
            clientMock.Verify(x => x.Set(11, "switch", "MOT1r"), Times.Exactly(1));
        }
    }
}
