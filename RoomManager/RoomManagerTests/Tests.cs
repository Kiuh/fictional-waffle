using RoomManager;

namespace RoomManagerTests
{
    public class Tests
    {
        [Test]
        public void TestUdpRange()
        {
            for (int i = 0; i < 128; i++)
            {
                var port = DockerNetworkClient.GetUdpPort();

                Assert.LessOrEqual(port, 34_000);
                Assert.GreaterOrEqual(port, 30_000);
            }
        }

        [Test]
        public void TestTcpRange()
        {
            for (int i = 0; i < 128; i++)
            {
                var port = DockerNetworkClient.GetTcpPort();

                Assert.LessOrEqual(port, 64_000);
                Assert.GreaterOrEqual(port, 60_000);
            }
        }
    }
}