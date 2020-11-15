using System.Runtime.InteropServices;

namespace PwnieProxy.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Position
    {
        public readonly ushort identifier;
        public readonly int x;
        public readonly int y;
        public readonly int z;
        public readonly ushort roll;
        public readonly ushort yaw;
        public readonly ushort pitch;
        public readonly byte movementDirection;
        public readonly byte strafingDirection;
    }
}