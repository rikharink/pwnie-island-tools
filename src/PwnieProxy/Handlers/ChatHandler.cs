using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PwnieProxy.Packets;

namespace PwnieProxy.Handlers
{
    public class ChatHandler : IHandler
    {
        public InterceptionStream? Other { get; set; }
        public byte[] Handle(byte[] data)
        {
            List<byte> packet = new List<byte>();
            var opcode = IHandler.GetOpcode(data);
            if (opcode == Opcodes.Chat)
            {
                var start = 4;
                var end = BitConverter.ToInt16(data[2..4]) + start;
                var msg = Encoding.ASCII.GetString(data[start..end]);
                if (msg.StartsWith("!"))
                {
                    packet.AddRange(ParseCommand(msg.Substring(1), data, end));
                }
            }
            else
            {
                packet.AddRange(data);
            }
            return packet.ToArray();
        }

        public byte[] GetLocationPacket(float x, float y, float z) => ((IPacket)new PositionUpdate(x, y, z)).ToByteArray();
        public byte[] GetPickupPacket(int id) => ((IPacket)new Pickup(id)).ToByteArray();
        public byte[] GetDialogPacket(string dialog) => ((IPacket)new Dialog(dialog)).ToByteArray();
        public byte[] GetEventPackage(string messageTop, string messageBottom) => new Event(messageTop, messageBottom).ToByteArray();
        public (float x, float y, float z) GetCoordinates(string x, string y, string z) => (float.Parse(x), float.Parse(y), float.Parse(z));

        public byte[] ParseCommand(string commandString, byte[] data, int offset)
        {
            Console.WriteLine("COMMAND: " + commandString);
            var parts = commandString.Split(" ", 2);
            var command = parts[0];
            var args = parts[1];
            List<byte> packet = (data[offset..] ?? Array.Empty<byte>()).ToList();
            switch (command)
            {
                case "event":
                    var eventArgs = args.Split(";");
                    ((IHandler)this).QueueForOther(GetEventPackage(eventArgs[0], eventArgs[1]));
                    break;
                case "gbof":
                    packet.AddRange(GetLocationPacket(-43655, -55820, 322));
                    packet.AddRange(GetPickupPacket(1));
                    break;
                case "bear":
                    packet.AddRange(GetLocationPacket(3321278464, 1199301120, 1160980583));
                    packet.AddRange(GetPickupPacket(3));
                    break;
                case "cow":
                    packet.AddRange(GetLocationPacket(1185440256 - 200000, 1193349120, 1157976064 + 500000));
                    packet.AddRange(GetPickupPacket(9));
                    packet.AddRange(GetDialogPacket("Quest"));
                    packet.AddRange(GetDialogPacket("$END"));
                    break;
                case "egg1":
                    packet.AddRange(GetLocationPacket(-25045, 18085, 260));
                    packet.AddRange(GetPickupPacket(11));
                    break;
                case "egg2":
                    packet.AddRange(GetLocationPacket(-51570, -61215, 5020));
                    packet.AddRange(GetPickupPacket(12));
                    break;
                case "egg3":
                    packet.AddRange(GetLocationPacket(24512, 69682, 2659));
                    packet.AddRange(GetPickupPacket(13));
                    break;
                case "egg4":
                    packet.AddRange(GetLocationPacket(60453, -17409, 2939));
                    packet.AddRange(GetPickupPacket(14));
                    break;
                case "egg5":
                    packet.AddRange(GetLocationPacket(1522, 14966, 7022));
                    packet.AddRange(GetPickupPacket(15));
                    break;
                case "egg6":
                    packet.AddRange(GetLocationPacket(11604, -13130.9023438f, 411.0f));
                    packet.AddRange(GetPickupPacket(16));
                    break;
                case "egg7":
                    packet.AddRange(GetLocationPacket(-72667, -53567, 1645));
                    packet.AddRange(GetPickupPacket(17));
                    break;
                case "egg8":
                    packet.AddRange(GetLocationPacket(48404, 28117, 704));
                    packet.AddRange(GetPickupPacket(18));
                    break;
                case "egg9":
                    packet.AddRange(GetLocationPacket(65225, -5740, 4928));
                    packet.AddRange(GetPickupPacket(19));
                    break;
                case "egg10":
                    packet.AddRange(GetLocationPacket(-2778, -11035, 10504));
                    packet.AddRange(GetPickupPacket(11));
                    break;
            }
            return packet.ToArray();
        }

    }
}