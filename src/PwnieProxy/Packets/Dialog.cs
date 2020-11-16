using System;
using System.Collections.Generic;
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
            var bytes = new List<byte>(2 + 2 + Length);
            bytes.AddRange(BitConverter.GetBytes(Opcode));
            bytes.AddRange(BitConverter.GetBytes(Length));
            bytes.AddRange(Message);
            return bytes.ToArray();
        }
    }
}