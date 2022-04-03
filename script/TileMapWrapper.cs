using Godot;
using System;
using ExtensionMethods;
using System.Collections.Generic;
using System.Linq;

public class TileMapWrapper : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public List<List<Character?>> Characters = new List<List<Character?>> {};
    public Tiles TileColors = new Tiles {};

    public Tile TileStatus(Vector2 pos)
    {
        var x = Mathf.FloorToInt(pos.x);
        var y = Mathf.FloorToInt(pos.y);
        Color _color = Color.Empty;
        try {
            _color = TileColors.Grid[x, y];
        }
        catch {}
        var character = CharacterAt(x, y);
        return new Tile {
            Location = new TileLocation {
                Pos = pos,
            },
            color = _color,
            empty = character is null,
            character = character,
        };
    }

    public Character CharacterSet(Character character)
    {
        var pos = character.Location.Pos;
        var x = Mathf.FloorToInt(pos.x);
        var y = Mathf.FloorToInt(pos.y);
        while (Characters.Count < y + 1)
        {
            Characters.Add(new List<Character?> {});
        }
        while (Characters[y].Count < pos.x + 1)
        {
            Characters[y].Add(null);
        }
        Characters[y][x] = character;
        // GD.Print("set char ", y, " ", x);
        character.PositionEl();
        return character;
    }

    public Character? CharacterAt(Vector2 pos) => CharacterAt(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
    public Character? CharacterAt(int x, int y)
    {
        if (y < 0 || Characters.Count < y + 1)
            return null;
        if (x < 0 || Characters[y].Count < x + 1)
            return null;
        return Characters[y][x];
    }

    public void CharacterMove(Character character, TileLocation loc)
    {
        var oldPos = character.Location.Pos;
        var oldAdjacent = character.Location.Adjacent;
        var x = Mathf.FloorToInt(oldPos.x);
        var y = Mathf.FloorToInt(oldPos.y);
        character.Location = loc;
        Characters[y][x] = null;
        CharacterSet(character);
        foreach (var pos in oldAdjacent)
        {
            var adjChar = CharacterAt(pos);
            if (adjChar is null || adjChar.HasMoved)
                continue;
            adjChar.SetPotentialMoves();
        }
        foreach (var pos in character.Location.Adjacent)
        {
            var adjChar = CharacterAt(pos);
            if (adjChar is null || adjChar.HasMoved)
                continue;
            adjChar.SetPotentialMoves();
        }
    }

    public void PrintCharacters()
    {
        foreach (var l in Characters)
        {
            foreach (var character in l)
            {
                if (character is null)
                    continue;
                GD.Print(character);
            }
        }
    }

    public int CharacterCount()
    {
        var res = 0;
        foreach (var l in Characters)
        {
            foreach (var character in l)
            {
                if (character is null)
                    continue;
                res++;
            }
        }
        return res;
    }

    public void PlaceTile(int x, int y, Color color)
    {
        TileColors.Grid[x, y] = color;
    }

    public Character Spawn(Vector2 pos, Color? color = null, Dir? pointing = null)
    {
        Color _color = rand.NextDouble() < 0.5
            ? Color.Red
            : Color.Blue;
        if (color != null)
            _color = (Color)color;
        Dir _pointing = pointing is null
            ? (Dir)Math.Floor(rand.NextDouble()*4)
            : (Dir)pointing;

        NodePath template;
        if (_color == Color.Red)
        {
            template = "Chars/SpriteRed";
        }
        else
        {
            template = "Chars/SpriteBlue";
        }

        var el = (Sprite)GetNode(template).Duplicate();
        el.Visible = true;
        GetNode("Chars").AddChild(el);

        return CharacterSet(new Character {
            Wrapper = this,
            Location = new TileLocation {
                Pos = pos,
            },
            Pointing = _pointing,
            El = el,
            color = _color,
            ID = CharacterCount(),
        });
    }

    public Random rand = new Random {};
    public void LoadRandom(double howManyAppx)
    {

        for (var y = 0; y < 21; y++)
        {
            for (var x = 0; x < 21; x++)
            {
                if (rand.NextDouble() > howManyAppx / (21*21))
                    continue;

                Spawn(new Vector2(x, y));
            }
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TileColors.Load((TileMap)GetNode("Control/TileMap"));

        // Spawn(new Vector2(10, 20), Color.Red, Dir.Up);
        // Spawn(new Vector2(10, 19), Color.Red, Dir.Up);

        StartTicking();
    }

    public async void StartTicking()
    {
        while (true)
        // for (var i = 0; i < 7; i++)
        {
            await this.TimerAsync(0.5F);
            Tick();
            // GD.Print("tick done ======================");
            // if (CharacterCount() > 5)
            // {
            //     return;
            // }
        }
    }

    public void Move(Character character)
    {
        var bestMove = character.BestMove;
        if (bestMove is null)
            return;
        CharacterMove(character, ((Tile)bestMove).Location);
    }

    public int spawnRate = 2;
    public int spawnCount = 2;

    Character? character;
    public void Tick()
    {
        foreach (var goal in TileColors.Goals)
        {
            var character = CharacterAt(goal.Location.Pos - Vector2.Up);
            if (character is null || character.color != goal.color)
                continue;
            var x = Mathf.FloorToInt(character.Location.Pos.x);
            var y = Mathf.FloorToInt(character.Location.Pos.y);
            character.El.QueueFree();
            Characters[y][x] = null;
        }

        var toMove = new List<Character> {};
        foreach (var row in Characters)
        {
            foreach (var character in row)
            {
                if (character is null) continue;
                character.HasMoved = false;
            }
        }
        foreach (var row in Characters)
        {
            foreach (var character in row)
            {
                if (character is null) continue;
                character.SetPotentialMoves();
                toMove.Add(character);
            }
        }

        List<Character> Now = new List<Character> {};
        List<Character> Later = new List<Character> {};

        foreach (var character in toMove)
        {
            if (character.IsIdealMove)
                Now.Add(character);
            else
                Later.Add(character);
        }

        // toMove.Sort((obj1, obj2) => obj1.ChoicePriority.CompareTo(obj2.ChoicePriority));

        // GD.Print(toMove[0].ChoicePriority, toMove[0].El.GetPath(), toMove[1].ChoicePriority, toMove[1].El.GetPath());

        while (Now.Count > 0 || Later.Count > 0)
        {
            Character character;
            if (Now.Count > 0)
                character = Now[0];
            else
                character = Later[0];
            Now.Remove(character);
            Later.Remove(character);
            var allAdjacent = character.Location.Adjacent;
            if (character.PotentialMoves.Count == 0)
                continue;
            Move(character);
            allAdjacent = allAdjacent.Concat(character.Location.Adjacent).ToList();
            foreach (var pos in allAdjacent)
            {
                var adjChar = CharacterAt(pos);
                if (adjChar is null || adjChar.HasMoved)
                    continue;
                var oldPriority = adjChar.ChoicePriority;
                if (adjChar.IsIdealMove && Now.IndexOf(adjChar) == -1)
                {
                    Now.Add(adjChar);
                    Later.Remove(adjChar);
                }
                else if (!adjChar.IsIdealMove && Now.IndexOf(adjChar) > -1)
                {
                    Now.Remove(adjChar);
                    Later.Add(adjChar);
                }
            }
        }

        // GD.Print("finished tick");

        if (spawnCount < spawnRate)
        {
            spawnCount++;
            return;
        }
        spawnCount = 0;

        var spawnPos = new Vector2(10, 20);
        var spawnLoc = new TileLocation {
            Pos = spawnPos,
        };
        if (!TileStatus(spawnPos).empty)
        {
            return;
        }
        Spawn(spawnPos, null, Dir.Up);
    }
}
