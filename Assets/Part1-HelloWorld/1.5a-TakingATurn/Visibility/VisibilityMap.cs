
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using RLTK.FieldOfView;
using System;
using Unity.Jobs;

namespace RLTKTutorial.Part1_5A
{
    public struct VisibilityMap : IVisibilityMap
    {
        public int width;
        public int height;
        
        NativeArray<TileType> tiles;

        NativeArray<bool> view;
        NativeArray<bool> memory;
        
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
            int i = p.y * width + p.x;
            view[i] = true;

            if( memory.IsCreated )
                memory[i] = true;
        }

        public VisibilityMap(int width, int height, 
            NativeArray<TileType> map, 
            NativeArray<bool> view,
            NativeArray<bool> memory = default)
        {
            this.tiles = map;
            this.width = width;
            this.height = height;
            this.view = view;
            this.memory = memory;
        }
    }
}