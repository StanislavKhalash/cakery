using NUnit.Framework;

namespace Core.Test
{
    [TestFixture]
    public class OracleFixture
    {
        [Test]
        public void GetAnswer_Returns42()
        {
            var sut = new Oracle();
            var result = sut.GetAnswer();
            Assert.That(result, Is.EqualTo(42));
        }
    }
}
