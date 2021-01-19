using Sark.Common.NativeArrayExtensions;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sark.Common.GridUtil
{
    public struct BitGrid2D : INativeDisposable
    {
        NativeBitArray data;
        int2 size;

        public NativeBitArray Array => data;
        public int Width => size.x;
        public int Height => size.y;
        public int2 Size => size;
        public int Length => data.Length;
        public bool IsCreated => data.IsCreated;

        public BitGrid2D(int2 size, Allocator allocator)
        {
            this.size = size;
            data = new NativeBitArray(size.x * size.y, allocator);
        }

        public BitGrid2D(int w, int h, Allocator allocator) :
            this(new int2(w, h), allocator)
        { }

        public bool this[int i]
        {
            get => data.IsSet(i);
            set => data.Set(i, value);
        }

        public bool this[int2 p]
        {
            get => this[PosToIndex(p.x, p.y)];
            set => this[PosToIndex(p.x, p.y)] = value;
        }

        public bool this[int x, int y]
        {
            get => this[PosToIndex(x,y)];
            set => this[PosToIndex(x,y)] = value;
        }

        public void MemClear()
        {
            Fill(false);
        }

        public void Fill(bool value)
        {
            data.SetBits(0, value, Length);
        }

        public int2 IndexToPos(int i) => Grid2D.IndexToPos(i, Width);
        public int PosToIndex(int2 pos) => PosToIndex(pos.x, pos.y);
        public int PosToIndex(int x, int y) => Grid2D.PosToIndex(x, y, Width);

        public bool InBounds(int2 pos) => Grid2D.InBounds(pos, Size);
        public bool InBounds(int i) => i >= 0 && i < data.Length;

        public void Dispose()
        {
            data.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return data.Dispose(inputDeps);
        }
    }

    public struct GridData2D<T>
        where T : unmanaged
    {
        NativeArray<T> data;
        int2 size;

        public NativeArray<T> Array => data;
        public int Width => size.x;
        public int Height => size.y;
        public int2 Size => size;
        public int Length => data.Length;
        public bool IsCreated => data.IsCreated;

        public GridData2D(NativeArray<T> data, int2 size)
        {
            this.data = data;
            this.size = size;
        }

        public GridData2D(NativeArray<T> data, int w, int h) :
            this(data, new int2(w, h))
        { }

        public T this[int i]
        {
            get => data[i];
            set => data[i] = value;
        }

        public T this[int2 p]
        {
            get => Get(p.x, p.y);
            set => Set(p.x,p.y, value);
        }

        public T this[int x, int y]
        {
            get => Get(x, y);
            set => Set(x, y, value);
        }

        public T Get(int x, int y) => data[PosToIndex(x, y)];

        public T Set(int x, int y, T value) => data[PosToIndex(x, y)] = value;

        public void MemClear()
        {
            unsafe
            {
                data.MemClear();
            }
        }

        public void Fill(T value)
        {
            for (int i = 0; i < Length; ++i)
                data[i] = value;
        }

        public int2 IndexToPos(int i) => Grid2D.IndexToPos(i, Width);
        public int PosToIndex(int2 pos) => PosToIndex(pos.x, pos.y);
        public int PosToIndex(int x, int y) => Grid2D.PosToIndex(x, y, Width);

        public bool InBounds(int2 pos) => Grid2D.InBounds(pos, Size);
        public bool InBounds(int i) => i >= 0 && i < data.Length;

    } 
}
