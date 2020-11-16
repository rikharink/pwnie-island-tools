using NUnit.Framework;
using PwnieProxy.Packets;

namespace PwnieProxy.Test
{
    public class PacketTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PositionTest()
        {
            var position = new Position
            {
                Opcode = 0x6d76,
                X = 20,
                Y = 40,
                Z = 0,
                Roll = 0,
                Yaw = 0,
                Pitch = 0,
                MovementDirection = 0x7f,
                StrafingDirection = 0x00
            };

            var bytes = ((IPacket)position).ToByteArray();
            Assert.NotNull(bytes);
            Assert.AreEqual(22, bytes!.Length);
            var unpackedPosition = IPacket.FromByteArray<Position>(bytes);
            Assert.AreEqual(position, unpackedPosition);
        }
    }
}