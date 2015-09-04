using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RailNet.Clients.Ecos.Tests
{
    [TestFixture]
    public class EcosClientTests
    {
        public EcosClient ec;

        [SetUp]
        public void SetUp()
        {
            ec = new EcosClient();
        }

        [Test]
        [Ignore("Needs ECoS connected to Network")]
        public async Task Connect()
        {
            Assert.False(ec.Connected);

            await ec.ConnectAsync("ecos");

            Assert.True(ec.Connected);

            ec.Disconnect();

            Assert.False(ec.Connected);
        }
    }
}
