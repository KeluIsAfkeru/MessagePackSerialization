using System.Runtime.InteropServices;

namespace SerializationBench
{
    public unsafe class MemoryBlock : IDisposable
    {
        public byte* Block { get; private set; }
        public int Size { get; private set; }
        public int UsedSize { get; set; }

        public MemoryBlock(int initialSize)
        {
            Size = initialSize;
            UsedSize = 0;
            Block = (byte*)Marshal.AllocHGlobal(Size).ToPointer();
        }

        public void Resize(int newSize)
        {
            Size = newSize;
            Block = (byte*)Marshal.ReAllocHGlobal((IntPtr)Block, (IntPtr)newSize).ToPointer();
        }

        public void Release()
        {
            Marshal.FreeHGlobal((IntPtr)Block);
            Block = null;
        }

        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// 返回一个完整的缓冲区，包括未使用的
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            byte[] result = new byte[Size];
            Marshal.Copy((IntPtr)Block, result, 0, Size);
            return result;
        }

        /// <summary>
        /// 返回一个已经使用过的缓冲区
        /// </summary>
        /// <returns></returns>
        public byte[] ToUsedByteArray()
        {
            byte[] result = new byte[UsedSize]; 
            Marshal.Copy((IntPtr)Block, result, 0, UsedSize);  
            return result;
        }
    }
}
