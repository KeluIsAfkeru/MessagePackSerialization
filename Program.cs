using MessagePack;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text;

namespace SerializationBench
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 创建10000个Cell实例
            Console.WriteLine("加载初始数据中...");
            var count = 1;

            var cells = new Cell[count];
            ObjectPool<Cell> pool = new ObjectPool<Cell>(count);

            Parallel.For(0, count, i =>
            {
                var cell = pool.Get();
                cells[i] = cell;
                // 不要立即返回对象到池中，因为我们仍然在使用它。
                // 当完成后或在适当的时机，可以调用 pool.Return(cell)。
            });

            Console.WriteLine("加载完成，开始使用自编内存管理器序列化");

            #region 使用自研的内存管理器进行序列化与反序列化操作
            // 创建内存管理器
            MemoryManager<byte> memoryManager = new MemoryManager<byte>(1024);

            // 计时器开始
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // 写入Cell列表的长度
            memoryManager.WriteData((uint)cells.Length);

            // 将每个Cell的属性依次写入MemoryManager
            foreach (Cell cell in cells)
            {
                memoryManager.WriteData(cell.X);
                memoryManager.WriteData(cell.Y);
                memoryManager.WriteData(cell.R);
                memoryManager.WriteData(cell.ID);
                memoryManager.WriteData(cell.Color);
                memoryManager.WriteData((uint)Encoding.UTF8.GetByteCount(cell.Name));
                memoryManager.WriteData(cell.Name);
            }

            // 计时器停止
            stopwatch.Stop();
            Console.WriteLine($"Serialization and writing time: {stopwatch.ElapsedMilliseconds} ms");

            // 计时器重新开始
            stopwatch.Restart();

            // 从MemoryManager中读取Cell列表的长度
            int cellCount = (int)memoryManager.ReadData<uint>(0);

            // 根据长度和之前的类型顺序反序列化Cell列表
            List<Cell> deserializedCells = new List<Cell>();
            int currentPosition = sizeof(uint);
            for (int i = 0; i < cellCount; i++)
            {
                double x = memoryManager.ReadData<float>(currentPosition);
                currentPosition += sizeof(float);

                double y = memoryManager.ReadData<float>(currentPosition);
                currentPosition += sizeof(float);

                double r = memoryManager.ReadData<float>(currentPosition);
                currentPosition += sizeof(float);

                int id = (int)memoryManager.ReadData<uint>(currentPosition);
                currentPosition += sizeof(uint);

                byte color = memoryManager.ReadData<byte>(currentPosition);
                currentPosition += sizeof(byte);

                int nameByteCount = (int)memoryManager.ReadData<uint>(currentPosition);
                currentPosition += sizeof(uint);

                string name = memoryManager.ReadString(currentPosition, nameByteCount);
                currentPosition += nameByteCount;

                //deserializedCells.Add(new Cell(x, y, r, id, color, name));
            }

            // 计时器停止
            stopwatch.Stop();
            Console.WriteLine($"Reading and deserialization time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"size: {memoryManager.memoryBlock.UsedSize} byte");

            #endregion

            #region msgpack
            //// 配置MessagePack序列化器
            Console.WriteLine("下面使用msgpack序列化");
            var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

            stopwatch.Reset();

            // 开始计时序列化过程
            stopwatch.Start();

            // 将Cell实例序列化为字节数组
            var serialized = MessagePackSerializer.Serialize(cells, options);

            // 停止计时
            stopwatch.Stop();

            Console.WriteLine($"Serialization took {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Pack size {serialized.Length} byte");

            // 重置计时器
            stopwatch.Reset();

            // 开始计时反序列化过程
            stopwatch.Start();

            // 将字节数组反序列化为Cell实例
            var deserialized = MessagePackSerializer.Deserialize<List<Cell>>(serialized, options);

            // 停止计时
            stopwatch.Stop();

            Console.WriteLine($"Deserialization took {stopwatch.ElapsedMilliseconds} ms");

            // 打印结果
            Console.WriteLine($"Original count: {count}");
            Console.WriteLine($"Deserialized count: {deserialized.Count}");
            #endregion

            Console.ReadLine();
        }
    }

    public class ObjectPool<T> where T : new()
    {
        private ConcurrentBag<T> _objects = new ConcurrentBag<T>();
        private int _capacity;

        public ObjectPool(int capacity)
        {
            _capacity = capacity;
        }

        public T Get()
        {
            if (_objects.TryTake(out T item))
                return item;
            return new T();
        }

        public void Return(T item)
        {
            if (_objects.Count < _capacity)
                _objects.Add(item);
        }
    }

}