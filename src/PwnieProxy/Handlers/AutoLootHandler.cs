using System;
using PwnieProxy.Packets;

namespace PwnieProxy.Handlers
{
    public class AutoLootHandler : IHandler
    {
        public InterceptionStream? Other { get; set; }

        public byte[] Handle(byte[] data)
        {
            var opcode = IHandler.GetOpcode(data);
            if(opcode == Opcodes.ItemInfo)
            {

            }
            return data;
        }
    }
}