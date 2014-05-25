using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;
using RailNet.Clients.Ecos.Network;

namespace RailNet.Clients.Ecos.Tests.Basic
{
    [TestFixture]
    public class BasicClientTests
    {
        private Mock<INachrichtenDispo> mock;
        private BasicClient client;
        private Subject<BasicEvent> incomingEvents;

        [SetUp]
        public void SetUp()
        {
            mock = new Mock<INachrichtenDispo>();
            incomingEvents = new Subject<BasicEvent>();
            mock.Setup(x => x.IncomingEvents).Returns(incomingEvents);
            
            client = new BasicClient(mock.Object);
        }

        #region QueryObjects

        [Test]
        public async void Q_QueryObjects_Simple()
        {
            var result = await client.QueryObjects(10);

            mock.Verify(x => x.SendCommandAsync("queryObjects(10)"), Times.Once());
        }

        [Test]
        public async void Q_QueryObjects()
        {
            var result = await client.QueryObjects(10, "info");

            mock.Verify(x => x.SendCommandAsync("queryObjects(10, info)"), Times.Once());
        }

        [Test]
        public async void Q_QueryObjects_Size()
        {
            var result = await client.QueryObjectsSize(10);

            mock.Verify(x => x.SendCommandAsync("queryObjects(10, size)"), Times.Once());
        }

        [Test]
        public async void Q_QueryObjects_Range()
        {
            var result = await client.QueryObjects(10, 0, 2, "info");

            mock.Verify(x => x.SendCommandAsync("queryObjects(10, info, nr[0,2])"), Times.Once());
        }


        #endregion

        #region Set

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void Q_Set_Param_Exception()
        {
            await client.Set(5, "", "1");
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void Q_Set_Value_Exception()
        {
            await client.Set(5, "1", "");
        }

        [Test]
        public async void Q_Set()
        {
            const string query = "set(5, addr[3])";
            mock.Setup(x => x.SendCommandAsync(query)).ReturnsAsync(new BasicResponse(
                new[]
                {
                    "<REPLY " + query + ">",
                    "5 addr[3]",
                    "<END 0 (OK)>"
                }, query));

            var result = await client.Set(5, "addr", "3");

            mock.Verify(x => x.SendCommandAsync(query), Times.Once());
            Assert.That(result.Command, Is.EqualTo(query));
            Assert.That(result.Content[1], Is.EqualTo("5 addr[3]"));
        }


        [Test]
        public async void Q_Set_Params()
        {
            const string query = "set(5, addr[3], name[\"Big Boy\"])";

            var result = await client.Set(5, new Dictionary<string, string>()
            {
                {"addr", "3"},
                {"name", "Big Boy"}
            });

            mock.Verify(x => x.SendCommandAsync(query), Times.Once());
        }

        #endregion

        #region Get

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void Q_Get_Exception()
        {
            await client.Get(5);
        }

        [Test]
        public async void Q_Get()
        {
            const string query = "get(5, info)";
            mock.Setup(x => x.SendCommandAsync(query)).ReturnsAsync(new BasicResponse(
                new[]
                {
                    "<REPLY " + query + ">",
                    "5 name[\"Big Boy\"]",
                    "<END 0 (OK)>"
                }, query));

            var result = await client.Get(5, "info");

            mock.Verify(x => x.SendCommandAsync(query), Times.Once());
            Assert.That(result.Command, Is.EqualTo(query));
            Assert.That(result.Content[1], Is.EqualTo("5 name[\"Big Boy\"]"));
        }

        #endregion

        #region Create

        [Test]
        public async void Q_Create_Id()
        {
            const string query = "create(5)";

            await client.Create(5);

            mock.Verify(x => x.SendCommandAsync(query));
        }

        [Test]
        public async void Q_Create_Id_Append()
        {
            const string query = "create(5, append)";

            await client.Create(5, true);

            mock.Verify(x => x.SendCommandAsync(query));
        }


        [Test]
        public async void Q_Create_Params()
        {
            const string query = "create(5, name[\"Big Boy\"], addr[5])";

            await client.Create(5, new Dictionary<string, string>()
            {
                {"name", "Big Boy"},
                {"addr", "5"}
            });

            mock.Verify(x => x.SendCommandAsync(query));
        }

        [Test]
        public async void Q_Create_Params_Append()
        {
            const string query = "create(5, name[\"Big Boy\"], addr[5], append)";

            await client.Create(5, new Dictionary<string, string>()
            {
                {"name", "Big Boy"},
                {"addr", "5"}
            }, true);

            mock.Verify(x => x.SendCommandAsync(query));
        }

        #endregion

        #region Delete

        [Test]
        public async void Q_Delete()
        {
            const string query = "delete(5)";

            await client.Delete(5);

            mock.Verify(x => x.SendCommandAsync(query));
        }

        #endregion

        #region Request


        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void Q_Request_Exception()
        {
            await client.Request(5, "");
        }

        [Test]
        public async void Q_Request_Param()
        {
            const string query = "request(5, view)";

            await client.Request(5, "view");

            mock.Verify(x => x.SendCommandAsync(query));
        }

        [Test]
        public async void Q_Request_Param_Force()
        {
            const string query = "request(5, view, force)";

            await client.Request(5, "view", true);

            mock.Verify(x => x.SendCommandAsync(query));
        }

        #endregion

        #region Release

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void Q_Release_Exception()
        {
            await client.Release(5);
        }

        [Test]
        public async void Q_Release()
        {
            const string query = "release(5, view, control)";

            await client.Release(5, "view", "control");

            mock.Verify(x => x.SendCommandAsync(query));
        }

        #endregion

        #region Events

        [Test]
        public void E_CallsEventHandler()
        {
            bool hasReceived = false;

            client.EventReceived += (sender, args) =>
            {
                hasReceived = true;
                Assert.That(args.Event.Receiver, Is.EqualTo(1000));
                Assert.That(args.Event.ErrorNumber, Is.EqualTo(0));
            };

            Assert.IsFalse(hasReceived);

            incomingEvents.OnNext(new BasicEvent(new[] {"<EVENT 1000>", "50 X", "<END 0 (OK)>"}));

            Assert.IsTrue(hasReceived);
        }

        #endregion
    }
}
