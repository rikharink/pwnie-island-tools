using System.Runtime.InteropServices;

namespace PwnieProxy.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pickup : IPacket
    {
        public Pickup(int item)
        {
            Opcode = 0x6565;
            Item = item;
        }
        
        public ushort Opcode { get; set; }
        public int Item { get; set; }
    }
}