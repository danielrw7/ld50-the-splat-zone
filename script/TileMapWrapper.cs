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
    public List<List<Character?>> Characters;
    public Tiles TileGrid;

    [Export]
    private NodePath AttackPath;

    [Export]
    private Texture TextureRed;
    [Export]
    private Texture TextureBlue;
    [Export]
    private Texture TextureGreen;

    public void _on_TileMap_reset()
    {
        Reset();
    }

    public void Reset(bool hasSetup = true)
    {
        if (hasSetup)
        {
            foreach (var l in Characters)
            {
                foreach (var character in l)
                {
                    if (character is null)
                        continue;
                    character.El.QueueFree();
                }
            }
            foreach (var attack in new List<TileAttack>(TileGrid.Attacks))
            {
                attack.Done();
            }
            TileGrid.Wipe((TileMap)GetNode("Control/TileMap"));
        }

        spawnRate = 2;
        spawnCount = 2;
        attackRate = 0;
        attackCount = 0;

        Characters = new List<List<Character?>> {};
        TileGrid = new Tiles {};

        // var map = (TileMap)GetNode("Control/TileMap");
        // TileGrid.Load(map);

        alive = 100 * 3;
        alive = 999;
        SpawnCounts = new ColorCounts {
            red = alive / 3,
            blue = alive / 3,
            green = alive / 3,
        };
    }

    public Tile TileStatus(Vector2 pos)
    {
        var x = Mathf.FloorToInt(pos.x);
        var y = Mathf.FloorToInt(pos.y);
        Color _color = Color.Empty;
        try {
            _color = TileGrid.Grid[x, y];
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

    {
        foreach (var l in Characters)
        {
            foreach (var character in l)
            {
                if (character is null)
                    continue;
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
        TileGrid.Grid[x, y] = color;
    }

    public Character Spawn(Vector2 pos, Color? color = null, Dir? pointing = null)
    {
        Color _color = rand.NextDouble() < (2.0/3)
            ? (rand.NextDouble() < 0.5 ? Color.Red : Color.Green)
            : Color.Blue;
        if (color != null)
            _color = (Color)color;
        Dir _pointing = pointing is null
            ? (Dir)Math.Floor(rand.NextDouble()*4)
            : (Dir)pointing;

        Texture texture;
        NodePath template;
        if (_color == Color.Red)
        {
            texture = TextureRed;
            // template = "Chars/SpriteRed";
            SpawnCounts.red--;
        }
        else if (_color == Color.Green)
        {
            texture = TextureGreen;
            // template = "Chars/SpriteGreen";
            SpawnCounts.green--;
        }
        else
        {
            texture = TextureBlue;
            // template = "Chars/SpriteBlue";
            SpawnCounts.blue--;
        }

        var el = (Sprite)GetNode("Chars/Sprite").Duplicate();
        el.Visible = true;
        el.Texture = texture;
        GetNode("Chars").AddChild(el);

        var ID = CharacterCount();

        var character = CharacterSet(new Character {
            Wrapper = this,
            Location = new TileLocation {
                Pos = pos,
            },
            Pointing = _pointing,
            El = el,
            color = _color,
            ID = ID,
        });

        character.PositionEl();

        return character;
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
        Reset(false);

        ((TileMap)GetNode("Control/TileMap")).Connect("pause_play", this, "PausePlay");

        // Spawn(new Vector2(10, 20), Color.Red, Dir.Up);
        // Spawn(new Vector2(10, 19), Color.Red, Dir.Up);

        // StartTicking();
    }

    private bool paused = false;
    public void PausePlay(bool _paused)
    {
        paused = _paused;

        if (!paused && DoneTicking)
            StartTicking();
    }

    private bool DoneTicking = true;
    public async void StartTicking()
    {
        Visible = true;
        DoneTicking = false;
        while (!paused)
        // for (var i = 0; i < 7; i++)
        {
            Tick();
            await this.TimerAsync(0.001F);
            // await this.TimerAsync(0.6F);
            // if (CharacterCount() > 5)
            // {
            //     return;
            // }
        }
        DoneTicking = true;
    }

    public void Move(Character character)
    {
        var bestMove = character.BestMove;
        if (bestMove is null)
            return;
        CharacterMove(character, ((Tile)bestMove).Location);
    }

    public int spawnRate;
    public int spawnCount;

    public int attackRate;
    public int attackCount;

    public bool CanPlaceAttack(TileAttack attack)
    {
        foreach (var other in TileGrid.Attacks)
        {
            if (other.CollidesWith(attack))
                return false;
        }
        return true;
    }

    public int attackSize = 5;
    public Vector2 RandAttackPos()
    {
        var offsetTop = attackSize - 2;
        var offsetBottom = attackSize - 2;
        var ySize = 22 - attackSize - offsetTop - offsetBottom;
        return new Vector2((float)Math.Floor(1 + rand.NextDouble() * (22 - attackSize)), (float)Math.Floor(1 + offsetTop + rand.NextDouble() * ySize));
        // return new Vector2(9F, 10F);
    }

    public TileAttack RandAttack()
    {
        return new TileAttack {
            Location = new TileLocation {
                Pos = RandAttackPos(),
            },
            attackMap = (TileMap)GetNode(AttackPath),
            map = (TileMap)GetNode("Control/TileMap"),
            size = attackSize,
        };
    }

    public void AddAttack()
    {
        var existing = TileGrid.Attacks.Select(val => val.Location.Pos).ToList();
        var attack = RandAttack();
        var i = 0;
        while (!CanPlaceAttack(attack))
        {
            i++;
            if (i > 20)
                return;
            attack = RandAttack();
        }
        TileGrid.Attacks.Add(attack);
        attack.Start();
    }

    public struct ColorCounts
    {
        public int red;
        public int blue;
        public int green;
    }

    public int alive;
    public ColorCounts SpawnCounts;

    public Color? NextSpawnColor()
    {
        var choices = new List<Color> {};
        if (SpawnCounts.red > 0)
            choices.Add(Color.Red);
        if (SpawnCounts.blue > 0)
            choices.Add(Color.Blue);
        if (SpawnCounts.green > 0)
            choices.Add(Color.Green);
        if (choices.Count == 0)
            return null;
        return choices.OrderBy(val => rand.Next()).ToList()[0];
    }

    [Signal]
    delegate void score();
    [Signal]
    delegate void death();
    [Signal]
    delegate void game_over();

    Character? character;
    public void Tick()
    {
        if (attackRate == 0)
            // attackRate = 1;
            attackRate = 10 + attackSize * 2 + (int)Math.Floor((rand.NextDouble() - 0.5) * 5);

        foreach (var goal in TileGrid.Goals)
        {
            var character = CharacterAt(goal.Location.Pos - Vector2.Up);
            if (character is null || character.color != goal.color)
                continue;
            var x = Mathf.FloorToInt(character.Location.Pos.x);
            var y = Mathf.FloorToInt(character.Location.Pos.y);
            character.El.Call("score");
            EmitSignal("score");
            if (character.color == Color.Red)
                SpawnCounts.red++;
            if (character.color == Color.Blue)
                SpawnCounts.blue++;
            if (character.color == Color.Green)
                SpawnCounts.green++;
            Characters[y][x] = null;
        }
        var hasDeath = false;
        foreach (var attack in new List<TileAttack>(TileGrid.Attacks))
        {
            attack.TickDecr();
            if (attack.TicksLeft > 0)
                continue;
            var chars = attack.Positions.Select(val => CharacterAt(val - Vector2.One));
            foreach (var character in chars)
            {
                if (character is null)
                    continue;
                var x = Mathf.FloorToInt(character.Location.Pos.x);
                var y = Mathf.FloorToInt(character.Location.Pos.y);
                character.El.Call("splat", character.color);
                Characters[y][x] = null;
                alive -= 1;
                hasDeath = true;
            }
            attack.Done();
            TileGrid.Attacks.Remove(attack);
        }
        if (hasDeath)
            EmitSignal("death");

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


        if (alive == 0)
        {
            Reset();
            EmitSignal("game_over");
        }

        if (spawnCount < spawnRate)
        {
            spawnCount++;
        }
        else
        {
            spawnCount = 0;

            var spawnPos = new Vector2(10, 21);
            var spawnLoc = new TileLocation {
                Pos = spawnPos,
            };
            if (!TileStatus(spawnPos).empty)
            {
                spawnCount = spawnRate;
                return;
            }
            var color = NextSpawnColor();
            if (color != null)
                Spawn(spawnPos, color, Dir.Up);
        }

        if (attackCount < attackRate)
        {
            attackCount++;
        }
        else
        {
            attackRate = 0;
            attackCount = 0;
            AddAttack();
        }
    }

    [Export]
    public int CharCount = 0;
}
