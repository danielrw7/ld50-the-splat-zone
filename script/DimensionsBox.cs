using Godot;
using System;
using System.Collections.Generic;
using ExtensionMethods;

public struct DimensionsBox
{
    public float[] x { get; set; }
    public float[] y { get; set; }

    public bool Intersects(DimensionsBox box)
    {
        if (x[1] < box.x[0])
            return false;
        if (x[0] > box.x[1])
            return false;
        if (y[1] < box.y[0])
            return false;
        if (y[0] > box.y[1])
            return false;
        return true;
    }
    public bool IntersectsMany(params DimensionsBox[] boxes)
    {
        foreach (var box in boxes)
        {
            if (Intersects(box)) return true;
        }
        return false;
    }

    public DimensionsBox Extend(Vector2 vec)
    {
        var existing = ToTuple();
        return DimensionsBox.FromList(new List<(Vector2, Vector2)> {
            existing,
            (
                existing.Item1 + vec,
                existing.Item2 + vec
            ),
        });
    }

    public DimensionsBox Copy()
    {
        return DimensionsBox.FromTuple(ToTuple());
    }

    public (Vector2, Vector2) ToTuple()
    {
        return (
            new Vector2(x[0], y[0]),
            new Vector2(x[1], y[1])
        );
    }

    public override string ToString()
    {
        return $"{base.ToString()} {{ x = [{String.Join(", ", x)}], y = [{String.Join(", ", y)}] }}";
    }

    public static DimensionsBox FromTuple((Vector2, Vector2) pair)
    {
        return new DimensionsBox {
            x = new float[2] { pair.Item1.x, pair.Item2.x }.MinMax(),
            y = new float[2] { pair.Item1.y, pair.Item2.y }.MinMax()
        };
    }

    public static DimensionsBox FromList(List<DimensionsBox> boxes)
    {
        var bounds = new float[2, 2] {
            { boxes[0].x[0], boxes[0].x[1] },
            { boxes[0].y[0], boxes[0].y[1] },
        };
        foreach (var box in boxes)
        {
            if (box.x[0] < bounds[0, 0])
                bounds[0, 0] = box.x[0];
            if (box.x[1] > bounds[0, 1])
                bounds[0, 1] = box.x[1];
            if (box.y[0] < bounds[1, 0])
                bounds[1, 0] = box.y[0];
            if (box.y[1] > bounds[1, 1])
                bounds[1, 1] = box.y[1];
        }
        return new DimensionsBox {
            x = new float[2] { bounds[0, 0], bounds[0, 1] },
            y = new float[2] { bounds[1, 0], bounds[1, 1] },
        };
    }
    public static DimensionsBox FromList(List<(Vector2, Vector2)> boxes)
    {
        var all = new List<float>[2] {
            new List<float> {},
            new List<float> {},
        };
        int i = 0;
        foreach ((Vector2, Vector2) box in boxes)
        {
            all[0].Add(box.Item1.x);
            all[0].Add(box.Item2.x);
            all[1].Add(box.Item1.y);
            all[1].Add(box.Item2.y);
            i++;
        }
        return new DimensionsBox {
            x = all[0].ToArray().MinMax(),
            y = all[1].ToArray().MinMax(),
        };
    }

    public static DimensionsBox operator +(DimensionsBox box, Vector2 vec)
    {
        return new DimensionsBox {
            x = new float[2] {
                box.x[0] + vec.x,
                box.x[1] + vec.x,
            },
            y = new float[2] {
                box.y[0] + vec.y,
                box.y[1] + vec.y,
            },
        };
    }
    public static DimensionsBox operator -(DimensionsBox box, Vector2 vec)
    {
        return box + (-1 * vec);
    }

    public static List<DimensionsBox> ListIntersectsWithList(List<DimensionsBox> boxes, List<DimensionsBox> theirBoxes, DimensionsBox? _ourRoughBox = null, DimensionsBox? theirRoughBox = null)
    {
        var res = new List<DimensionsBox> {};

        DimensionsBox ourRoughBox = _ourRoughBox is null
            ? DimensionsBox.FromList(boxes)
            : (DimensionsBox)_ourRoughBox;

        if (theirRoughBox != null)
        {
            if (!ourRoughBox.Intersects((DimensionsBox)theirRoughBox))
                return res;
        }

        int i = 0;
        for (; i < theirBoxes.Count; i++)
        {
            var checkBox = theirBoxes[i];
            if (checkBox.x[0] <= ourRoughBox.x[0] && checkBox.y[0] <= ourRoughBox.y[0])
                break;
        }
        if (i == theirBoxes.Count)
            return res;

        for (; i < theirBoxes.Count; i++)
        {
            var checkBox = theirBoxes[i];
            if (ourRoughBox.x[1] < checkBox.x[0] && ourRoughBox.y[1] < checkBox.y[0])
                break;

            foreach (var box in boxes)
            {
                if (box.Intersects(checkBox))
                    res.Add(theirBoxes[i]);
            }
        }

        return res;
    }
}
