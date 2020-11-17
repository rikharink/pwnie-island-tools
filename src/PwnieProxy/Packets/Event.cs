using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PwnieProxy.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Event : IPacket
    {
        public Event(string messageTop, string messageBottom)
        {
            Opcode = 0x6576;
            TopLength = (ushort)messageTop.Length;
            MessageTop = ASCIIEncoding.ASCII.GetBytes(messageTop);
            BottomLength = (ushort)messageBottom.Length;
            MessageBottom = ASCIIEncoding.ASCII.GetBytes(messageBottom);
        }

        public ushort Opcode { get; }
        public ushort TopLength { get; }
        public byte[] MessageTop { get; }
        public ushort BottomLength { get; }
        public byte[] MessageBottom { get; }

        public byte[] ToByteArray()
        {
            var bytes = new byte[2 + 2 + TopLength + 2 + BottomLength];
            int offset = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(Opcode), 0, bytes, offset, 2);
            offset += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(TopLength), 0, bytes, offset, 1);
            offset += 2;
            Buffer.BlockCopy(MessageTop, 0, bytes, offset, TopLength);
            offset += TopLength;
            Buffer.BlockCopy(BitConverter.GetBytes(BottomLength), 0, bytes, offset, 1);
            offset += 2;
            Buffer.BlockCopy(MessageBottom, 0, bytes, offset, BottomLength);
            return bytes;
        }
    }
}