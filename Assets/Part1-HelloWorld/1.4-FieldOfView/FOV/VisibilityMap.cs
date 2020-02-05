
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using RLTK.FieldOfView;
using System;
using Unity.Jobs;

namespace RLTKTutorial.Part1_4
{
    public struct VisibilityMap : IVisibilityMap, IDisposable
    {
        public int width;
        public int height;
        
        NativeArray<TileType> tiles;
        public NativeList<int2> visibleTiles;

        public float Distance(int2 a, int2 b) => math.distance(a, b);

        public bool IsInBounds(int2 p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

        public bool IsOpaque(int2 p)
        {
            if (IsInBounds(p) == false)
                return true;
            return tiles[p.y * width + p.x] == TileType.Wall;
        }

        public void SetVisible(int2 p)
        {
            if (!IsInBounds(p))
                return;
            visibleTiles.Add(p);
        }

        public VisibilityMap(int width, int height, NativeArray<TileType> mapTiles, Allocator allocator)
        {
            this.tiles = mapTiles;
            this.width = width;
            this.height = height;
            this.visibleTiles = new NativeList<int2>(width * height / 2, allocator);
        }



        public void Clear()
        {
            visibleTiles.Clear();
        }

        public void Dispose( )
        {
            visibleTiles.Dispose();
        }

        public void Dispose(JobHandle handle)
        {
            visibleTiles.Dispose(handle);
        }
    }
}