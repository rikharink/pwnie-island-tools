using System;
using System.Runtime.InteropServices;

namespace PwnieProxy.Packets
{
    public interface IPacket
    {
        ushort Opcode { get; }

        public byte[] ToByteArray()
        {
            byte[] arr = null!;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(this);
                arr = new byte[size];
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr!;
        }

        public static string GetOpcode(byte[] data)
        {
            return BitConverter.ToString(new[] { data[1], data[0] });
        }

        public static T FromByteArray<T>(byte[] bytes)
        {
            T position;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                position = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T))!;
            }
            finally
            {
                handle.Free();
            }
            return position;
        }
    }
}
