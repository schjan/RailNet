using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Network;

namespace RailNet.Clients.Ecos.Tests.Basic
{
    [TestFixture]
    public class NachrichtenDispoTests
    {
        private Mock<INetworkClient> mock;
        private NachrichtenDispo dispo;

        [SetUp]
        public void SetUp()
        {
            mock = new Mock<INetworkClient>();
            mock.Setup(x => x.Connected).Returns(true);

            dispo = new NachrichtenDispo(mock.Object);
        }

        [Test]
        public async void TestDispoTimeout()
        {
            var result = await dispo.SendeBefehlAsync("queryObjects(10, name)");

            Assert.That(result.Error, Is.EqualTo("Timeout!"));
        }


        [Test]
        public async void TestDispoVerarbeiten()
        {
            const string befehl = "queryObjects(10, name)";
            var dings = dispo.SendeBefehlAsync(befehl);

            mock.Raise(x => x.MessageReceivedEvent += null,
                new MessageReceivedEventArgs(
                    new[]
                    {
                        "<REPLY queryObjects(10, name)>",
                        "1000 name[\"V 160 026\"]",
                        "<END 0 (OK)>"
                    }));

            var result = await dings;

            mock.Verify(x => x.SendMessage(befehl), Times.Exactly(1));
            Assert.That(result.Befehl, Is.EqualTo(befehl));
        }
    }
}
