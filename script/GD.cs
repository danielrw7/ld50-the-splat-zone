using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

class GD
{
    public static void Print(params object[] what)
    {
        if (what.Length < 2) {
            Godot.GD.Print(what);
            return;
        }
        List<object> whatPadded = new List<object> { what[0] };

        var rest = new ArraySegment<object>(what, 1, what.Length - 1);
        foreach (object part in rest)
        {
            whatPadded.Add(" ");
            whatPadded.Add(part);
        }

        Godot.GD.Print(whatPadded.ToArray());
    }
}

static class GDE
{

}
