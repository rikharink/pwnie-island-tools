using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PwnieProxy.Packets;

namespace PwnieProxy.Handlers
{
    public class PacketLogger : IHandler
    {
        public InterceptionStream? Other { get; set; }
        private readonly string name;

        public PacketLogger(string name)
        {
            this.name = name;
        }

        public byte[] Handle(byte[] data)
        {
            var opcode = IHandler.GetOpcode(data);
            switch (opcode)
            {
                case Opcodes.Chat:
                case Opcodes.Movement:
                case Opcodes.Nop:
                case Opcodes.Event:
                    break;
                default:
                    Console.WriteLine($"{name} Unknown packet [{opcode}]: {BitConverter.ToString(data)}");
                    break;
            }
            return data;
        }
    }
}