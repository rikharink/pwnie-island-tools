using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PwnieProxy.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemInfo : IPacket
    {
        public ItemInfo(ushort id)
        {
            Opcode = 0x6b6d;
            ItemId = id;
        }
        
        public ushort Opcode { get; }
        public ushort ItemId { get; }

        public byte[] ToByteArray()
        {
            int packetSize = 0;
            byte[] arr = new byte[packetSize];
            return arr;
        }
    }
}