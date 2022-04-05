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
        var aboveMap = (TileMap)GetNode("Control/TileMapAbove");
        MusicPlayer.Seek(0);
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
            foreach (var goal in TileGrid.Goals)
            {
                goal.Done(aboveMap);
            }
        }

        numTicks = 0;
        spawnRate = 2;
        spawnCount = 2;
        attackRate = 0;
        attackCount = -20;

        var goals = new List<TileGoal> {};
        var goalColors = new List<Color> {
            Color.Red,
            Color.Blue,
            Color.Green,
        }.OrderBy(val => rand.Next()).ToList();
        foreach (var color in goalColors)
        {
            var pos = new Vector2((int)Math.Floor(1 + rand.NextDouble() * 20), -1);
            while (aboveMap.GetCellv(pos + Vector2.One) != -1)
                pos = new Vector2((int)Math.Floor(1 + rand.NextDouble() * 20), -1);
            var goal = new TileGoal {
                Location = new TileLocation {
                    Pos = pos,
                },
                color = color,
            };
            goal.Draw(aboveMap);
            goals.Add(goal);
        }

        Characters = new List<List<Character?>> {};
        TileGrid = new Tiles {
            Goals = goals,
        };

        // var map = (TileMap)GetNode("Control/TileMap");
        // TileGrid.Load(map);

        alive = 5 * 3;
        SpawnCounts = new ColorCounts {
            red = alive / 3,
            blue = alive / 3,
            green = alive / 3,
        };
        AliveCounts = new ColorCounts {
            red = alive / 3,
            blue = alive / 3,
            green = alive / 3,
        };

        EmitSignal("alive_count", AliveCounts.red, AliveCounts.blue, AliveCounts.green);
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
        var newX = Mathf.FloorToInt(character.Location.Pos.x);
        var newY = Mathf.FloorToInt(character.Location.Pos.y);
        TileGrid.Paths[newX, newY]++;
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
        el.Call("parent_speed", TimePerTick);
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
        MusicPlayer = (AudioStreamPlayer)GetNode(MusicPlayerPath);
        ScorePlayer = (AudioStreamPlayer2D)GetNode(ScorePlayerPath);
        MissPlayer = (AudioStreamPlayer2D)GetNode(MissPlayerPath);
        HitPlayer = (AudioStreamPlayer2D)GetNode(HitPlayerPath);
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

        if (paused && !MusicPlayer.StreamPaused)
            MusicPlayer.StreamPaused = true;

        if (!paused && DoneTicking)
            StartTicking();
    }

    [Export]
    private float TimePerTick;
    [Export]
    private NodePath MusicPlayerPath;
    [Export]
    private NodePath ScorePlayerPath;
    [Export]
    private NodePath MissPlayerPath;
    [Export]
    private NodePath HitPlayerPath;

    private AudioStreamPlayer MusicPlayer;
    private AudioStreamPlayer2D ScorePlayer;
    private AudioStreamPlayer2D MissPlayer;
    private AudioStreamPlayer2D HitPlayer;

    private bool DoneTicking = true;
    public async void StartTicking()
    {
        if (MusicPlayer.StreamPaused)
            MusicPlayer.StreamPaused = false;
        else
            MusicPlayer.Play();
        Visible = true;
        DoneTicking = false;
        float? lastSpeed = null;
        while (!paused)
        {
            var speedChanged = lastSpeed != TimePerTick;
            if (speedChanged)
                lastSpeed = TimePerTick;
            Tick(speedChanged);
            await this.TimerAsync(TimePerTick);
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
    public Vector2 RandAttackPos(TileMap map, bool reset = false)
    {
        var best = TileGrid.BestAttack(map, reset);
        var bestVec = new Vector2(best.Item1, best.Item2);
        bestVec -= new Vector2(2, 2);
        bestVec += new Vector2((float)Math.Floor(rand.NextDouble() * 2), (float)Math.Floor(rand.NextDouble() * 2));
        var offsetTop = 0; // attackSize - 2;
        var offsetBottom = 0; // attackSize - 2;
        var ySize = 22 - attackSize - offsetBottom - 1;
        bestVec.x = Mathf.Max(0, Mathf.Min(22 - attackSize - 1, bestVec.x));
        bestVec.y = Mathf.Max(offsetTop, Mathf.Min(ySize, bestVec.y));
        return bestVec + Vector2.One;
        // return new Vector2((float)Math.Floor(1 + rand.NextDouble() * (22 - attackSize)), (float)Math.Floor(1 + offsetTop + rand.NextDouble() * ySize));
        // return new Vector2(9F, 10F);
    }

    public TileAttack RandAttack(bool reset = false)
    {
        var map = (TileMap)GetNode("Control/TileMap");
        return new TileAttack {
            Location = new TileLocation {
                Pos = RandAttackPos(map, reset),
            },
            attackMap = (TileMap)GetNode(AttackPath),
            map = map,
            size = attackSize,
        };
    }

    public void AddAttack()
    {
        var existing = TileGrid.Attacks.Select(val => val.Location.Pos).ToList();
        var attack = RandAttack(true);
        var i = 0;
        while (!CanPlaceAttack(attack))
        {
            i++;
            if (i > 10)
                return;
            attack = RandAttack(false);
        }
        TileGrid.Attacks.Add(attack);
        TileGrid.Paths = new int[21, 21];
        attack.Start();
    }

    public struct ColorCounts
    {
        public int red;
        public int blue;
        public int green;
    }

    public int alive;
    public int numTicks;
    public ColorCounts SpawnCounts;
    public ColorCounts AliveCounts;

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
    delegate void alive_count(int red, int blue, int green);
    [Signal]
    delegate void game_over();

    Character? character;
    public void Tick(bool speedChanged = false)
    {
        numTicks++;
        if (attackRate == 0)
        {
            // attackRate = 1;
            attackRate = 5 + (int)(Math.Floor(Math.Max(0, Math.Min(5, (600.0 - numTicks) / 100)))) + attackSize * 2 + (int)Math.Floor((rand.NextDouble() - 0.5) * 5);
        }

        var hasScore = false;
        foreach (var goal in TileGrid.Goals)
        {
            var character = CharacterAt(goal.Location.Pos + Vector2.Down);
            if (character is null || character.color != goal.color)
                continue;
            hasScore = true;
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

        if (hasScore)
        {
            ScorePlayer.Play(0);
        }

        var hasAttack = false;
        var hasDeath = false;
        foreach (var attack in new List<TileAttack>(TileGrid.Attacks))
        {
            attack.TickDecr();
            if (attack.TicksLeft > 0)
                continue;
            hasAttack = true;
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
                if (character.color == Color.Red)
                    AliveCounts.red--;
                if (character.color == Color.Blue)
                    AliveCounts.blue--;
                if (character.color == Color.Green)
                    AliveCounts.green--;
                hasDeath = true;
            }
            attack.Done((Control)GetNode("Control/Swatter"));
            TileGrid.Attacks.Remove(attack);
        }
        if (hasDeath)
        {
            EmitSignal("alive_count", AliveCounts.red, AliveCounts.blue, AliveCounts.green);
            HitPlayer.Play();
        }
        else if (hasAttack)
        {
            MissPlayer.Play();
        }

        var toMove = new List<Character> {};
        foreach (var row in Characters)
        {
            foreach (var character in row)
            {
                if (character is null) continue;
                character.HasMoved = false;
                if (speedChanged)
                    character.El.Call("parent_speed", TimePerTick);
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
