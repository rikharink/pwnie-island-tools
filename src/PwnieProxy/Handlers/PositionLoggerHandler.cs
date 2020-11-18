using PwnieProxy.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var position = IPacket.FromByteArray<Position>(data);
                Console.WriteLine("Position: " + position);
            }
            return data;
        }
    }
}
