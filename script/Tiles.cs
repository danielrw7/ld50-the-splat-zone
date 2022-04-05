using Godot;
using System;
using ExtensionMethods;
using System.Collections.Generic;
using System.Linq;

public enum Dir {Right = 0, Down = 1, Left = 2, Up = 3}
public enum Color { Empty = 1, Blue = 2, Red = 4, Green = 9 }

public struct TileLocation
{
    private Vector2 _Pos;
    public Vector2 Pos { get; set; }

    public List<Vector2> Adjacent {
        get
        {
            return new List<Vector2> {
                Pos + Vector2.Up,
                Pos + Vector2.Down,
                Pos + Vector2.Left,
                Pos + Vector2.Right,
            };
        }
    }

    public override string ToString()
    {
        return $"TileLocation {{ Pos = {Pos.ToString()} }}";
    }
}

public struct Tile
{
    public TileLocation Location;
    public Color color;
    public bool empty;
    public Character? character;

    public bool Valid
    {
        get => Location.Pos.x >= 0
            && Location.Pos.y >= 0
            && Location.Pos.x <= 20
            && Location.Pos.y <= 20;
    }

    public bool Locked
    {
        get => !(character is null) && character.HasMoved;
    }
}

public struct TileGoal
{
    public TileLocation Location;
    public Color color;

    public void Draw(TileMap map)
    {
        if (color == Color.Red)
            map.SetCellv(Location.Pos + Vector2.One, 5);
        if (color == Color.Blue)
            map.SetCellv(Location.Pos + Vector2.One, 6);
        if (color == Color.Green)
            map.SetCellv(Location.Pos + Vector2.One, 10);
    }
    public void Done(TileMap map)
    {
        map.SetCellv(Location.Pos + Vector2.One, -1);
    }
}

public class TileAttack
{
    public TileLocation Location;
    public int TicksLeft = 15;
    public TileMap attackMap;
    public TileMap map;

    public int size = 4;

    public void Start()
    {
        attackMap.Call("attack_count_inc");
        Draw();
    }
    public void Done(Control? swatter = null)
    {
        attackMap.Call("attack_count_dec");
        Positions.ForEach(val => map.Call("place", val - Vector2.One, 1));
        Draw(-1);
        if (swatter != null)
            swatter.Call("swat", Location.Pos);
    }
    public void TickDecr()
    {
        TicksLeft--;
    }

    private List<Vector2>? _Positions;
    public List<Vector2> Positions
    {
        get
        {
            if (_Positions != null) return _Positions;
            _Positions = new List<Vector2> {};
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                _Positions.Add(Location.Pos + Vector2.Down * y + Vector2.Right * x);
            }
            return _Positions;
        }
    }

    private DimensionsBox? _CollisionBox;
    public DimensionsBox CollisionBox
    {
        get
        {
            _CollisionBox ??= DimensionsBox.FromTuple((Location.Pos, Location.Pos + new Vector2((float)size - 1, (float)size - 1)));
            return (DimensionsBox)_CollisionBox;
        }
    }

    public bool CollidesWith(TileAttack attack)
    {
        return CollisionBox.Intersects(attack.CollisionBox);
    }
    public void Draw(int tileID = 11)
    {
        attackMap.SetCellv(Location.Pos, tileID);
        // Positions.Skip(1).ToList().ForEach(val => attackMap.SetCellv(val, 8));
    }
}

public class Tiles
{
    public Color[,] Grid = new Color[21, 21];
    public int[,] Paths = new int[21, 21];
    public List<TileGoal> Goals = new List<TileGoal> {
        new TileGoal {
            Location = new TileLocation {
                Pos = new Vector2(6, -1),
            },
            color = Color.Red,
        },
        new TileGoal {
            Location = new TileLocation {
                Pos = new Vector2(15, -1),
            },
            color = Color.Blue,
        },
        new TileGoal {
            Location = new TileLocation {
                Pos = new Vector2(11, -1),
            },
            color = Color.Green,
        },
    };
    public List<TileAttack> Attacks = new List<TileAttack> {};
    public Vector2 Offset = Vector2.One;

    public Tiles()
    {
        // Grid[0, 0] = Color.Red;
    }

    public void Load(TileMap map)
    {
        for (var y = 0; y < 21; y++)
        {
            for (var x = 0; x < 21; x++)
            {
                Grid[x, y] = (Color)map.GetCellv(new Vector2(x, y) + Offset);
            }
        }
    }

    public void Wipe(TileMap map)
    {
        for (var y = 0; y < 21; y++)
        {
            for (var x = 0; x < 21; x++)
            {
                if (Grid[x, y] != Color.Empty)
                    map.SetCellv(new Vector2(x, y) + Offset, (int)Color.Empty);
            }
        }
    }

    private Random rand = new Random {};
    public (int, int)? _BestAttack = null;
    public (int, int) BestAttack(TileMap map, bool reset = false)
    {
        if (!reset && _BestAttack != null) return ((int, int))_BestAttack;

        var res = new List<(int, int, int)> {};

        for (var y = 0; y < 21; y++)
        {
            for (var x = 0; x < 21; x++)
            {
                var mapVal = map.GetCellv(new Vector2(x, y) + Offset) != (int)Color.Empty ? 2 : 0;
                var val = Paths[x, y] + 2 - (int)Math.Floor(rand.NextDouble() * 4);
                if (y > 12 && x >= 6 && x <= 14)
                    continue;
                res.Add((x, y, val));
            }
        }

        res.Sort((val1, val2) => val2.Item3.CompareTo(val1.Item3));
        var best = res.First();
        _BestAttack = (best.Item1, best.Item2);
        return ((int, int))_BestAttack;
    }
}

public class Character
{
    private TileLocation _Location;
    public TileLocation Location {
        get => _Location;
        set
        {
            Pointing = (Location.Pos - value.Pos).ToDir();
            _Location = value;
            _PotentialMoves = null;
            _Uncontested = null;
            _Wait = null;
            PositionEl();
            HasMoved = true;
        }
    }
    public Sprite El { get; set; }
    public bool HasMoved = false;
    public Dir Pointing = Dir.Down;
    public Color color;

    public int ID;

    public void PositionEl(float? speed = null)
    {
        if (!(El is Sprite))
            return;
        if (El.Position == Vector2.Zero)
            El.Position = _Location.Pos + Vector2.One * 1.5F;
        else
        {
            El.Call("set_next_pos", _Location.Pos + Vector2.One * 1.5F);
        }
        El.FlipH = false;
        El.FlipV = false;
        if (Pointing == Dir.Up)
            El.RotationDegrees = 180F;
        if (Pointing == Dir.Right)
        {
            El.RotationDegrees = -90F;
        }
        if (Pointing == Dir.Left)
        {
            El.FlipH = true;
            El.RotationDegrees = 90F;
        }
        if (Pointing == Dir.Down)
            El.RotationDegrees = 0F;
    }

    public TileMapWrapper Wrapper;

    private Random rand = new Random {};

    public List<Tile> SetPotentialMoves()
    {
        _Uncontested = null;
        _Wait = null;
        _IdealMove = null;
        _ChoicePriority = null;
        var pos = Location.Pos;
        _AllPotentialMoves = new List<Tile> {
            Wrapper.TileStatus(pos + Vector2.Down.RotateSquare(Pointing)),
            Wrapper.TileStatus(pos + Vector2.Left.RotateSquare(Pointing)),
            Wrapper.TileStatus(pos + Vector2.Right.RotateSquare(Pointing)),
            // Wrapper.TileStatus(pos + Vector2.Up.RotateSquare(Pointing)),
        };
        if (rand.NextDouble() >= 0.5)
        {
            var tmp = _AllPotentialMoves[1];
            _AllPotentialMoves[1] = _AllPotentialMoves[2];
            _AllPotentialMoves[2] = tmp;
        }
        _PotentialMoves = _AllPotentialMoves.Where(val => val.empty && val.Valid && !val.Locked).ToList();
        if (_PotentialMoves.Count == 0)
        {
            var pot = Wrapper.TileStatus(pos + Vector2.Up.RotateSquare(Pointing));
            _AllPotentialMoves.Add(pot);
            if (pot.empty && pot.Valid && !pot.Locked)
                _PotentialMoves.Add(pot);
        }
        return _PotentialMoves;
    }

    public bool IsIdealMove
    {
        get
        {
            if (BestMove is null || IdealMove is null)
                return true;
            return ((Tile)BestMove).Location.Pos == ((Tile)IdealMove).Location.Pos;
        }
    }

    public Tile? BestMove
    {
        get
        {
            if (PotentialMoves.Count == 0)
                return null;
            foreach (var move in PotentialMoves)
            {
                if (move.color == color)
                {
                    return move;
                }
            }
            return PotentialMoves[0];
        }
    }

    public Tile? _IdealMove = null;
    public Tile? IdealMove
    {
        get
        {
            if (!(_IdealMove is null))
                return _IdealMove;
            if (_AllPotentialMoves is null || _AllPotentialMoves.Count == 0)
                return _IdealMove;
            foreach (var move in _AllPotentialMoves)
            {
                if (move.Locked || !move.Valid)
                    continue;
                if (move.color == color)
                {
                    _IdealMove = move;
                    return _IdealMove;
                }
            }
            foreach (var move in _AllPotentialMoves)
            {
                if (move.Locked || !move.Valid)
                    continue;
                _IdealMove = move;
                break;
            }
            if (_IdealMove is null)
            {
                _IdealMove = _AllPotentialMoves[0];
            }
            return _IdealMove;
        }
    }

    public bool? _Uncontested = null;
    public bool Uncontested
    {
        get
        {
            if (!(_Uncontested is null)) return (bool)_Uncontested;
            if (BestMove is null || _AllPotentialMoves is null)
            {
                _Uncontested = false;
                // if (ID == 0)
                return (bool)_Uncontested;
            }
            var _best = (Tile)BestMove;
            var index = _AllPotentialMoves.IndexOf(_best);
            if (index != 0)
            {
                _Uncontested = false;
                // if (ID == 0)
                return (bool)_Uncontested;
            }
            var adj = new List<Tile> {
                Wrapper.TileStatus(_best.Location.Pos + Vector2.Down.RotateSquare(Pointing)),
                Wrapper.TileStatus(_best.Location.Pos + Vector2.Left.RotateSquare(Pointing)),
                Wrapper.TileStatus(_best.Location.Pos + Vector2.Right.RotateSquare(Pointing)),
                Wrapper.TileStatus(_best.Location.Pos + Vector2.Up.RotateSquare(Pointing)),
            };
            _Uncontested = adj.Where(val => !(val.character is null) && val.character != this && !val.character.HasMoved).Count() == 0;
            // if (ID == 0)
            return (bool)_Uncontested;
        }
    }

    public bool? _Wait = null;
    public bool Wait
    {
        get
        {
            if (!(_Wait is null)) return (bool)_Wait;
            _Wait = false;
            if (BestMove is null)
            {
                return (bool)_Wait;
            }
            var _best = (Tile)BestMove;
            if (_best.color == color)
            {
                _Wait = true;
            }
            else
            {
                if (!(_AllPotentialMoves is null))
                {
                    foreach (var tile in _AllPotentialMoves)
                    {
                        if (tile.color == color && !tile.Locked)
                        {
                            _Wait = true;
                            break;
                        }
                    }
                }
            }
            return (bool)_Wait;
        }
    }

    public List<Tile>? _AllPotentialMoves;

    public int? _ChoicePriority;
    public int ChoicePriority
    {
        get
        {
            if (!(_ChoicePriority is null))
                return (int)_ChoicePriority;
            if (BestMove is null || _AllPotentialMoves is null)
            {
                _ChoicePriority = int.MaxValue;
                return (int)_ChoicePriority;
            }
            var _best = (Tile)BestMove;
            var index = _AllPotentialMoves.IndexOf(_best);
            if (index == -1) index = 4;
            // return index;
            if (_best.color == color) index -= 4;
            if (!IsIdealMove) index += 32;
            else if (Uncontested)
            {
                index -= 8;
            }
            // else if (Wait) index += 16;
            // if (ID == 0)
            // if (ID == 1)
            // if (ID == 2)
            _ChoicePriority = index;
            return index;
        }
    }

    private List<Tile>? _PotentialMoves;
    public List<Tile> PotentialMoves {
        get
        {
            if (_PotentialMoves is null)
                _PotentialMoves = SetPotentialMoves();
            return _PotentialMoves;
        }
    }

    public override string ToString()
    {
        return $"Character {{ ID = {ID}, Location = {Location.ToString()} }}";
    }
}
