using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
//一个自制的提供高效内存管理的动态数组
//暂未用上目前
namespace SerializationBench
{
    //值类型
    public unsafe class DynamicArraySrtuct<T> where T : unmanaged
    {
        private T* _buffer;
        private int _count;
        private int _capacity;

        public DynamicArraySrtuct(int initialCapacity = 4)
        {
            _buffer = (T*)Marshal.AllocHGlobal(sizeof(T) * initialCapacity);
            _count = 0;
            _capacity = initialCapacity;
        }

        public void Add(T item)
        {
            if (_count == _capacity)
            {
                _capacity *= 2;
                T* newBuffer = (T*)Marshal.AllocHGlobal(sizeof(T) * _capacity);
                Buffer.MemoryCopy(_buffer, newBuffer, _capacity * sizeof(T), _count * sizeof(T));
                Marshal.FreeHGlobal((IntPtr)_buffer);
                _buffer = newBuffer;
            }

            _buffer[_count] = item;
            ++_count;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();

                return _buffer[index];
            }

            set
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();

                _buffer[index] = value;
            }
        }

        public int Count => _count;


        ~DynamicArraySrtuct()
        {
            Marshal.FreeHGlobal((IntPtr)_buffer);
        }
    }
    [MessagePackObject]
    public class DynamicArray<T>: IEnumerable<T>, IMessagePackSerializationCallbackReceiver
    {
        [IgnoreMember]
        private T[] _buffer;
        [Key(0)]
        public List<T> Items { get; set; }
        [IgnoreMember]
        private int _count;
        [IgnoreMember]
        private int _capacity;

        public DynamicArray(int initialCapacity = 4)
        {
            _buffer = new T[initialCapacity];
            _count = 0;
            _capacity = initialCapacity;
        }

        public void Add(T item)
        {
            if (_count == _capacity)
            {
                _capacity *= 2;
                T[] newBuffer = new T[_capacity];
                Array.Copy(_buffer, newBuffer, _count);
                _buffer = newBuffer;
            }

            _buffer[_count] = item;
            ++_count;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();

            Array.Copy(_buffer, index + 1, _buffer, index, _count - index - 1);

            --_count;

            // Shrinks the array if needed
            if (_count <= _capacity / 4)
            {
                _capacity /= 2;
                T[] newBuffer = new T[_capacity];
                Array.Copy(_buffer, newBuffer, _count);
                _buffer = newBuffer;
            }
        }

        public void OnBeforeSerialize()
        {
            Items = new List<T>(_buffer.Take(_count));
        }

        public void OnAfterDeserialize()
        {
            _count = Items.Count;
            _capacity = Items.Count * 2;
            _buffer = new T[_capacity];
            Items.CopyTo(_buffer);
            Items = null;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();

                return _buffer[index];
            }

            set
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();

                _buffer[index] = value;
            }
        }

        // 添加 Clear 方法
        public void Clear()
        {
            _count = 0;
            _capacity = 4;
            _buffer = new T[_capacity];
        }

        // 添加 AddRange 方法
        public void AddRange(IEnumerable<T> items)
        {
            // 获取要添加的项目数量
            int itemsCount = items.Count();

            // 如果需要更多空间，则将容量增加到足够大的值，以避免频繁的实际分配
            if (_count + itemsCount > _capacity)
            {
                while (_count + itemsCount > _capacity)
                {
                    _capacity *= 2;
                }

                T[] newBuffer = new T[_capacity];
                Array.Copy(_buffer, newBuffer, _count);
                _buffer = newBuffer;
            }

            // 将数据复制到数组中
            foreach (var item in items)
            {
                _buffer[_count++] = item;
            }
        }

        [IgnoreMember]
        public int Count => _count;

        // Implementation for the GetEnumerator method.
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[i];
            }
        }

        // Explicit interface implementation for the non-generic IEnumerator
        // Required as IEnumerable<T> inherits from IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
