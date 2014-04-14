using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RailNet.Clients.Ecos.Tests
{
    [TestFixture]
    public class RailClientTests
    {
        public RailClient rc;

        [SetUp]
        public void SetUp()
        {
            rc = new RailClient();
        }

        [Test]
        [Ignore("Needs ECoS connected to Network")]
        public async Task Connect()
        {
            Assert.False(rc.Connected);

            await rc.ConnectAsync("ecos");

            Assert.True(rc.Connected);

            rc.Disconnect();

            Assert.False(rc.Connected);
        }
    }
}
