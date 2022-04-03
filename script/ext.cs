using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static float[] MinMax(this float[] values)
        {
            if (values.Length < 1) return new float[] {};
            var res = new float[2] { values[0], values[0] };
            for (int i = 1; i < values.Length; i++)
            {
                res[0] = Math.Min(res[0], values[i]);
                res[1] = Math.Max(res[1], values[i]);
            }
            return res;
        }

        public static async Task TimerAsync(this Node node, float WaitTime)
        {
            await node.ToSignal(node.GetTree().CreateTimer(WaitTime), "timeout");
        }

        public static Dir Diff(this Dir dir, Dir to)
        {
            return (Dir)(((int)to - (int)dir + 4) % 4);
        }
        public static Dir Turn(this Dir dir, Dir turn)
        {
            return (Dir)(((int)dir + (int)turn + 4) % 4);
        }
        public static Dir ToDir(this Vector2 pos)
        {
            if (pos == Vector2.Down)
                return Dir.Up;
            if (pos == Vector2.Left)
                return Dir.Right;
            if (pos == Vector2.Right)
                return Dir.Left;
            // if (pos == Vector2.Up)
            return Dir.Down;
        }

        public static Vector2 RotateSquare(this Vector2 vec, Dir dir)
        {
            switch (dir)
            {
                case Dir.Up:
                    return new Vector2(vec.x, -vec.y);
                case Dir.Right:
                    return new Vector2(vec.y, -vec.x);
                // case Dir.Down:
                //     return vec.Copy();
                case Dir.Left:
                    return new Vector2(-vec.y, vec.x);
                default:
                    return new Vector2(vec.x, vec.y);
            }
        }
    }
}
