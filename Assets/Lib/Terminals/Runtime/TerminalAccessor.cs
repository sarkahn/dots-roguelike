using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

using Sark.Common.GridUtil;

using Color = UnityEngine.Color;
using static Sark.Terminals.CodePage437;

namespace Sark.Terminals
{
    public struct TerminalAccessor
    {
        GridData2D<TerminalTile> tiles;

        public int Width => tiles.Width;
        public int Height => tiles.Height;
        public int Length => tiles.Length;
        public int2 Size => tiles.Size;

        public TerminalTile this[int i]
        {
            get => tiles[i];
            set => tiles[i] = value;
        }

        public TerminalTile this[int x, int y]
        {
            get => tiles[x, y];
            set => tiles[x, y] = value;
        }

        public TerminalTile this[int2 p]
        {
            get => tiles[p];
            set => tiles[p] = value;
        }

        public bool InBounds(int i) =>
            tiles.InBounds(i);
        public bool InBounds(int2 p) =>
            tiles.InBounds(p);

        public TerminalAccessor(
            DynamicBuffer<TerminalTilesBuffer> tiles, int2 size) :
            this(tiles.Reinterpret<TerminalTile>().AsNativeArray(), size)
        { }

        public TerminalAccessor(NativeArray<TerminalTile> tiles, int2 size)
        {
            this.tiles = new GridData2D<TerminalTile>(tiles, size);
        }

        public int PosToIndex(int x, int y) => tiles.PosToIndex(x, y);
        public int PosToindex(int2 p) => tiles.PosToIndex(p);
        public int2 IndexToPos(int i) => tiles.IndexToPos(i);

        public void SetByte(int x, int y, byte b)
        {
            int i = Grid2D.PosToIndex(x, y, Width);
            var t = tiles[i];
            t.Glyph = b;
            tiles[i] = t;
        }

        public void SetByteFGColor(int x, int y, byte b, Color fg)
        {
            int i = tiles.PosToIndex(x, y);
            var t = tiles[i];
            t.Glyph = b;
            t.FGColor = fg;
            tiles[i] = t;
        }

        public void SetByteBGColor(int x, int y, byte b, Color bg)
        {
            int i = tiles.PosToIndex(x, y);
            var t = tiles[i];
            t.Glyph = b;
            t.BGColor = bg;
            tiles[i] = t;
        }

        public void SetByteFGColorBGColor(int x, int y, byte b, Color fg, Color bg)
        {
            int i = tiles.PosToIndex(x, y);
            var t = tiles[i];
            t.Glyph = b;
            t.FGColor = fg;
            t.BGColor = bg;
            tiles[i] = t;
        }

        public void SetFGColor(int x, int y, Color fg)
        {
            int i = tiles.PosToIndex(x, y);
            var t = tiles[i];
            t.FGColor = fg;
            tiles[i] = t;
        }

        public void SetBGColor(int x, int y, Color bg)
        {
            int i = tiles.PosToIndex(x, y);
            var t = tiles[i];
            t.BGColor = bg;
            tiles[i] = t;
        }

        public void SetChar(int x, int y, char c)
        {
            SetByte(x, y, ToCP437(c));
        }

        public void ClearTile(int x, int y)
        {
            this[x, y] = TerminalTile.Default;
        }

        public void SetCharFGColor(int x, int y, char c, Color fg)
        {
            SetByteFGColor(x, y, ToCP437(c), fg);
        }

        public void SetCharBGColor(int x, int y, char c, Color bg)
        {
            SetByteBGColor(x, y, ToCP437(c), bg);
        }

        public void SetCharFGColorBGColor(int x, int y, char c, Color fg, Color bg)
        {
            SetByteFGColorBGColor(x, y, ToCP437(c), fg, bg);
        }

        public TerminalTile GetTile(int x, int y)
            => tiles[x, y];

        public void SetTile(int x, int y, TerminalTile t)
            => tiles[x, y] = t;

        public NativeArray<TerminalTile> ReadTiles(int x, int y,
            int len, Allocator allocator)
        {
            var buff = new NativeArray<TerminalTile>(len, allocator);
            len = math.min(len, tiles.Length - x);
            int i = Grid2D.PosToIndex(x, y, Width);
            NativeArray<TerminalTile>.Copy(tiles.Array, i, buff, 0, len);

            return buff;
        }

        public void ClearScreen()
        {
            for (int i = 0; i < tiles.Length; ++i)
                tiles[i] = TerminalTile.Default;
        }

        public void Print(int2 xy, FixedString128 str) => Print(xy.x, xy.y, str);
        public void Print(int x, int y, FixedString128 str)
        {
            //UnityEngine.Debug.Log($"Printing string of len {str.Length}");
            for (int i = 0; i < str.Length; ++i)
            {
                int index = tiles.PosToIndex(x, y);
                if (tiles.InBounds(index))
                {
                    var t = tiles[index];
                    t.Glyph = str[i];
                    tiles[index] = t;
                }
                else
                    return;

                ++x;

                if (x >= Width)
                {
                    x = 0;
                    y--;
                }
            }
        }

        public void PrintFGColor(int2 xy, FixedString128 str, Color fgColor) =>
            PrintFGColor(xy.x, xy.y, str, fgColor);
        public void PrintFGColor(int x, int y, FixedString128 str, Color fgColor)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                int index = tiles.PosToIndex(x, y);
                if (tiles.InBounds(index))
                {
                    var t = tiles[index];
                    t.Glyph = str[i];
                    t.FGColor = fgColor;
                    tiles[index] = t;
                }
                else
                    return;

                ++x;

                if (x >= Width)
                {
                    x = 0;
                    y--;
                }
            }
        }

        public void PrintFGColorBGColor(int x, int y, FixedString128 str, Color fgColor, Color bgColor)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                int index = tiles.PosToIndex(x, y);
                if (tiles.InBounds(index))
                {
                    var t = tiles[index];
                    t.Glyph = str[i];
                    t.FGColor = fgColor;
                    t.BGColor = bgColor;
                    tiles[index] = t;
                }
                else
                    return;

                ++x;

                if (x >= Width)
                {
                    x = 0;
                    y--;
                }
            }
        }

        public void DrawBorder()
        {
            DrawBox(0, 0, Width, Height);
        }

        public void DrawFilledBox(int xOrigin, int yOrigin, int width, int height)
        {
            for(int x = xOrigin; x < xOrigin + width; ++x)
            {
                for(int y = yOrigin; y < yOrigin + height; ++y)
                {
                    tiles[x, y] = TerminalTile.Default;
                }
            }
            DrawBox(xOrigin, yOrigin, width, height);
        }
        public void DrawBox(int xOrigin, int yOrigin, int width, int height)
        {
            int l = xOrigin;
            int r = xOrigin + width - 1;
            int b = yOrigin;
            int t = yOrigin + height - 1;

            for (int y = b + 1; y < t; ++y)
            {
                int x = l;
                SetChar(x, y, '│');
                x = r;
                SetChar(x, y, '│');
            }

            for (int x = l + 1; x < r; ++x)
            {
                int y = b;
                SetChar(x, y, '─');
                y = t;
                SetChar(x, y, '─');
            }

            SetChar(l, b, '└');
            SetChar(l, t, '┌');
            SetChar(r, t, '┐');
            SetChar(r, b, '┘');
        }

        public void DrawBoxFGColor(int xOrigin, int yOrigin, int width, int height, Color fgColor)
        {
            int l = xOrigin;
            int r = xOrigin + width - 1;
            int b = yOrigin;
            int t = yOrigin + height - 1;

            for (int y = b + 1; y < t; ++y)
            {
                int x = l;
                SetCharFGColor(x, y, '│', fgColor);
                x = r;
                SetCharFGColor(x, y, '│', fgColor);
            }

            for (int x = l + 1; x < r; ++x)
            {
                int y = b;
                SetCharFGColor(x, y, '─', fgColor);
                y = t;
                SetCharFGColor(x, y, '─', fgColor);
            }

            SetCharFGColor(l, b, '└', fgColor);
            SetCharFGColor(l, t, '┌', fgColor);
            SetCharFGColor(r, t, '┐', fgColor);
            SetCharFGColor(r, b, '┘', fgColor);

            for (int x = l + 1; x < r; ++x)
            {
                for (int y = b + 1; y < t; ++y)
                {
                    SetCharFGColor(x, y, ' ', fgColor);
                }
            }
        }

        public void DrawHorizontalBar(int x, int y, int width, int value, int max)
        {
            DrawHorizontalBar(x, y, width, value, max, Color.white, Color.grey);
        }

        public void DrawHorizontalBar(int x, int y, int width, int value, int max,
            Color filledColor, Color emptyColor)
        {
            float normalized = max == 0 ? 0 : (float)value / max;
            int v = (int)math.ceil(normalized * width);

            if (v != 0)
            {
                for (int i = 0; i < v; ++i)
                {
                    SetCharFGColor(x + i, y, '▓', filledColor);
                }
            }
            for (int i = v; i < width; ++i)
                SetCharFGColor(x + i, y, '░', emptyColor);
        }
    }
}
