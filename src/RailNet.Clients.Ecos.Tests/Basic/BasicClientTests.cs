using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Tests.Basic
{
    [TestFixture]
   public class BasicClientTests
    {
        private BasicClient client;

        [SetUp]
        public void SetUp()
        {
            client = new BasicClient();
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public async void ThrowNotImplemented()
        {
           await client.QueryObjects(1);
        }
    }
}
