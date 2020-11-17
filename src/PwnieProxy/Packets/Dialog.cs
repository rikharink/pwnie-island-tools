using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PwnieProxy.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Dialog : IPacket
    {
        public Dialog(string message)
        {
            Opcode = 0x3e23;
            Length = (ushort)message.Length;
            Message = ASCIIEncoding.ASCII.GetBytes(message);
        }

        public ushort Opcode { get; }
        public ushort Length { get; }
        public byte[] Message { get; }

        public byte[] ToByteArray()
        {
            var bytes = new byte[2 + 2 + Length];
            var offset = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(Opcode), 0, bytes, offset, 2);
            offset += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, bytes, offset, 2);
            offset += 2;
            Buffer.BlockCopy(Message, 0, bytes, offset, Length);
            return bytes;
        }
    }
}