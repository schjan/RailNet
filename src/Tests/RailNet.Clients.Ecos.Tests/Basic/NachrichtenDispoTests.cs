using System;
using System.Collections.Generic;
using System.IO;
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
        const string command = "queryObjects(10, name)";

        [SetUp]
        public void SetUp()
        {
            mock = new Mock<INetworkClient>();
            mock.Setup(x => x.Connected).Returns(true);

            dispo = new NachrichtenDispo(mock.Object);
        }

        [Test]
        [ExpectedException(typeof(IOException))]
        public async void SendCommandThrowsNotConnected()
        {
            mock.Setup(x => x.Connected).Returns(false);

            await dispo.SendCommandAsync(command);
        }

        [Test]
        public async void SendCommandAsync()
        {
            var dings = dispo.SendCommandAsync(command);

            mock.Raise(x => x.MessageReceivedEvent += null,
                new MessageReceivedEventArgs(
                    new[]
                    {
                        "<REPLY queryObjects(10, name)>",
                        "1000 name[\"V 160 026\"]",
                        "<END 0 (OK)>"
                    }));

            var result = await dings;

            mock.Verify(x => x.SendMessage(command), Times.Exactly(1));
            Assert.That(result.Command, Is.EqualTo(command));
        }

        [Test]
        [ExpectedException(typeof (TimeoutException))]
        public async void SendCommandAsyncTimeout()
        {
            
            var dings = dispo.SendCommandAsync(command);
            
            var result = await dings;

            mock.Verify(x => x.SendMessage(command), Times.Exactly(1));
            Assert.That(result.Command, Is.EqualTo(command));
        }

        [Test]
        [Ignore("How is this possible?")]
        public async void Send2CommandsAsync()
        {
            var first = dispo.SendCommandAsync(command);
            var second = dispo.SendCommandAsync(command);

            mock.Raise(x => x.MessageReceivedEvent += null,
                new MessageReceivedEventArgs(
                    new[]
                    {
                        "<REPLY queryObjects(10, name)>",
                        "FIRST",
                        "<END 0 (OK)>"
                    }));


            mock.Raise(x => x.MessageReceivedEvent += null,
                new MessageReceivedEventArgs(
                    new[]
                    {
                        "<REPLY queryObjects(10, name)>",
                        "SECOND",
                        "<END 0 (OK)>"
                    }));

            var result1 = await first;
            var result2 = await second;

            mock.Verify(x => x.SendMessage(command), Times.Exactly(2));

            Assert.That(result1.Command, Is.EqualTo(command));
            Assert.That(result2.Command, Is.EqualTo(command));

            Assert.That(result1.Content[1], Is.EqualTo("FIRST"));
            Assert.That(result2.Content[1], Is.EqualTo("SECOND"));
        }
    }
}
