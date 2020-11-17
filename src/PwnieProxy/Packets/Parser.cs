using System;
using System.Collections.Generic;

namespace PwnieProxy.Packets
{
    public static class Parser
    {
        public static IList<IPacket> Parse(byte[] data)
        {
            var packets = new List<IPacket>();
            var opcode = IPacket.GetOpcode(data);
            switch (opcode)
            {
                case Opcodes.Nop:
                    break;
                case Opcodes.Movement:
                    packets.Add(IPacket.FromByteArray<PositionUpdate>(data[0..22]));
                    if (data.Length > 22)
                    {
                        packets.AddRange(Parse(data[22..]));
                    }
                    break;
                default:
                    packets.Add(new UnknownPacket(BitConverter.ToUInt16(data[0..2]), data[2..]));
                    break;
            }
            return packets;
        }
    }

    public struct UnknownPacket : IPacket
    {
        public UnknownPacket(ushort opcode, byte[] data)
        {
            Opcode = opcode;
            Data = data;
        }
        public ushort Opcode { get; }
        public byte[] Data { get; }
    }
}