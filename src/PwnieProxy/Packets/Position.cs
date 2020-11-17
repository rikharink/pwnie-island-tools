using System.Runtime.InteropServices;

namespace PwnieProxy.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionUpdate : IPacket
    {
        public PositionUpdate(float x, float y, float z)
        {
            Opcode = 0x766d;
            X = x;
            Y = y;
            Z = z;
            Roll = 0;
            Yaw = 0;
            Pitch = 0;
            MovementDirection = 0;
            StrafingDirection = 0;
        }

        public ushort Opcode { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public ushort Roll { get; set; }
        public ushort Yaw { get; set; }
        public ushort Pitch { get; set; }
        public byte MovementDirection { get; set; }
        public byte StrafingDirection { get; set; }

        public override string ToString()
        {
            return $"Opcode: {Opcode} Position: ({X}, {Y}, {Z}) Roll: {Roll} Yaw: {Yaw}, Pitch: {Pitch} Movement: {MovementDirection} Strafe: {StrafingDirection}";
        }

    }
}