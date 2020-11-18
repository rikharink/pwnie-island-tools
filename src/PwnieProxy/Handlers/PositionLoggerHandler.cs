using PwnieProxy.Packets;
using System;

namespace PwnieProxy.Handlers
{
    public class PositionLoggerHandler : IHandler
    {
        public InterceptionStream? Other { get; set; }

        public byte[] Handle(byte[] data)
        {
            var opcode = BitConverter.ToString(data[0..2]);
            if(opcode == Opcodes.Movement)
            {
                var position = IPacket.FromByteArray<PositionUpdate>(data);
                Console.WriteLine("Position: " + position);
            }
            return data;
        }
    }
}
