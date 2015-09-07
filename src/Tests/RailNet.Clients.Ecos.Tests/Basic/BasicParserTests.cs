using NUnit.Framework;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Tests.Basic
{
    [TestFixture]
    public class BasicParserTests
    {
        [Test]
        public void TryGetParameterFromContentTest()
        {
            var result = BasicParser.TryGetParameterFromContent("name2", "blablabla name1[Train] name2[Zug]");

            Assert.That(result, Is.EqualTo("Zug"));
        }

        [Test]
        public void TryGetParameterFromContentTestFails()
        {
            var result = BasicParser.TryGetParameterFromContent("name3", "blablabla name1[Train] name2[Zug]");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TryGetParameterFromContentTestChars()
        {
            var result = BasicParser.TryGetParameterFromContent("name2", "blablabla name1[Train] name2[Z[[[ug]");

            Assert.That(result, Is.EqualTo("Z[[[ug"));
        }

        [Test]
        public void TryGetParameterWithQuotationFromContentTest()
        {
            var result = BasicParser.TryGetParameterFromContent("name2", "blablabla name1[\"Train\"] name2[\"Zug\"]",
                true);

            Assert.That(result, Is.EqualTo("Zug"));
        }

        [Test]
        public void TryGetParameterWithQuotationFromContentTestFails()
        {
            var result = BasicParser.TryGetParameterFromContent("name3", "blablabla name1[\"Train\"] name2[\"Zug\"]",
                true);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TryGetParameterWithQuotationFromContentTestChars()
        {
            var result = BasicParser.TryGetParameterFromContent("name2", "blablabla name1[\"Train\"] name2[\"Z[[[ug\"]",
                true);

            Assert.That(result, Is.EqualTo("Z[[[ug"));
        }
    }
}
