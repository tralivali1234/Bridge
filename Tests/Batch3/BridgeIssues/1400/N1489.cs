using Bridge.Test;

namespace Bridge.ClientTest.Batch3.BridgeIssues
{
    [Category(Constants.MODULE_ISSUES)]
    [TestFixture(TestNameFormat = "#1489 - {0}")]
    public class Bridge1489
    {
        enum Enum : long
        {
            A = 1L,
            B = 2L
        }

        [Test]
        public void TestLongEnum()
        {
            Enum @enum = Enum.A;
            Assert.AreEqual("B", (@enum + 1).ToString());
            Assert.AreEqual("B", (++@enum).ToString());
        }
    }
}