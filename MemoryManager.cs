using System.Runtime.InteropServices;
using System.Text;

namespace SerializationBench
{
    // 一个通用的内存管理器，用于管理非托管内存块
    public unsafe class MemoryManager<M> : IDisposable where M : unmanaged
    {
        public MemoryBlock memoryBlock;   // 内存块
        private int currentPosition;       // 当前写入位置
        private readonly int chunkSize;  // 内存块扩增的大小，可以根据需求进行调整
        private bool disposed = false;     // 标记是否已经释放

        // 构造函数，参数为内存块的初始大小
        public MemoryManager(int initialSize)
        {
            if (initialSize <= 0) throw new ArgumentOutOfRangeException(nameof(initialSize));
            memoryBlock = new MemoryBlock(initialSize * sizeof(M));
            currentPosition = 0;
            chunkSize = 1024 * sizeof(M);
        }

        public void WriteData<T>(T data) where T : unmanaged
        {
            if (disposed) throw new ObjectDisposedException("MemoryManager");

            while (currentPosition * sizeof(M) + sizeof(T) > memoryBlock.Size)
            {
                memoryBlock.Resize(memoryBlock.Size + chunkSize);
            }

            *(T*)(memoryBlock.Block + currentPosition * sizeof(M)) = data;
            currentPosition += sizeof(T) / sizeof(M);
            memoryBlock.UsedSize = Math.Max(memoryBlock.UsedSize, currentPosition * sizeof(M));  // 更新已使用的内存长度
        }

        public unsafe void WriteData(string data)
        {
            if (disposed) throw new ObjectDisposedException("MemoryManager");

            int byteCount = Encoding.UTF8.GetByteCount(data);

            while (currentPosition + byteCount > memoryBlock.Size)
            {
                memoryBlock.Resize(memoryBlock.Size + chunkSize);
            }

            fixed (char* pStr = data)
            {
                byte* pDest = memoryBlock.Block + currentPosition;
                Encoding.UTF8.GetBytes(pStr, data.Length, pDest, byteCount);
            }
            currentPosition += byteCount;
            memoryBlock.UsedSize = Math.Max(memoryBlock.UsedSize, currentPosition * sizeof(M));  // 更新已使用的内存长度
        }


        // 从内存块中读取数据
        public T ReadData<T>(int position) where T : unmanaged
        {
            if (disposed) throw new ObjectDisposedException("MemoryManager");
            int realPosition = position * sizeof(M);
            if (realPosition < 0 || realPosition >= currentPosition * sizeof(M)) throw new ArgumentOutOfRangeException(nameof(position));

            // 从指定位置读取数据
            T data = *(T*)(memoryBlock.Block + realPosition);
            return data;
        }

        public unsafe string ReadString(int startPosition, int byteCount)
        {
            if (disposed) throw new ObjectDisposedException("MemoryManager");
            if (startPosition < 0 || startPosition + byteCount > currentPosition * sizeof(M)) throw new ArgumentOutOfRangeException(nameof(startPosition));

            byte* pSrc = memoryBlock.Block + startPosition;
            int charCount = Encoding.UTF8.GetMaxCharCount(byteCount);
            char* pStr = stackalloc char[charCount];

            charCount = Encoding.UTF8.GetChars(pSrc, byteCount, pStr, charCount);

            return new string(pStr, 0, charCount);
        }

        // 释放内存块
        public void ReleaseMemory()
        {
            if (disposed) throw new ObjectDisposedException("MemoryManager");

            memoryBlock.Release();
            memoryBlock = null;
            currentPosition = 0;
        }

        // Dispose方法，用于释放资源
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    memoryBlock?.Release();
                }
                // 释放非托管资源
                memoryBlock = null;
                currentPosition = 0;
                disposed = true;
            }
        }

        // 析构函数，当垃圾回收器回收对象时会自动调用
        ~MemoryManager()
        {
            // 不要改变此代码。 把清理代码放在 'Dispose(bool disposing)' 方法中
            Dispose(disposing: false);
        }

        // IDisposable接口的Dispose方法，手动释放资源
        public void Dispose()
        {
            // 不要改变此代码。 把清理代码放在 'Dispose(bool disposing)' 方法中
            Dispose(disposing: true);
            // 通知垃圾回收器不要调用析构函数
            GC.SuppressFinalize(this);
        }
    }
}
