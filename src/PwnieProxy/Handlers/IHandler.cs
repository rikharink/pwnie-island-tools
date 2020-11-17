using System;
using System.Linq;

namespace PwnieProxy.Handlers
{
    public interface IHandler
    {
        public void QueueForOther(byte[] data) => Other?.QueueMessage(data);
        InterceptionStream? Other { get; set; }
        byte[] Handle(byte[] data);

        public static string GetOpcode(byte[] data)
        {
            return BitConverter.ToString(new[] { data[1], data[0] });
        }
    }
}