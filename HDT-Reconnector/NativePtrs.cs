using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HDT_Reconnector
{
    internal class NativePtrs : IDisposable
    {
        private List<IntPtr> ptrs = new List<IntPtr>();

        public void Add(IntPtr ptr)
        {
            ptrs.Add(ptr);
        }

        public IntPtr Add(int size)
        {
            var ptr = Marshal.AllocHGlobal(size);
            ptrs.Add(ptr);
            return ptr;
        }

        public IntPtr Add<T>(T value)
        {
            var ptr = Add(Marshal.SizeOf<T>());
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public IntPtr Add<T>(T[] values)
        {
            var valueSize = Marshal.SizeOf<T>();
            var ptr = Add(valueSize * values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                var tempPtr = new IntPtr(ptr.ToInt64() + i * valueSize);
                Marshal.StructureToPtr(values[i], tempPtr, false);
            }

            return ptr;
        }

        public void Dispose()
        {
            foreach (var pts in ptrs)
                Marshal.FreeHGlobal(pts);
        }
    }
}
