using System;
using System.Runtime.InteropServices;

namespace ClipboardMachinery.Core.Services.Clipboard {

    internal static class BinaryStructConverter {

        public static T FromByteArray<T>(byte[] bytes) where T : struct {
            IntPtr ptr = IntPtr.Zero;

            try {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            } finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct {
            IntPtr ptr = IntPtr.Zero;

            try {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            } finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

    }

}
