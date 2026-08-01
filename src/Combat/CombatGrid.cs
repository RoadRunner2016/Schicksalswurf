using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Combat
{
    using Characters;
    using Core;

    /// <summary>
    /// Tactical combat grid for isometric combat, DSA-style.
    /// Party on one side, enemies on the other, with movement and positioning.
    /// </summary>
    public class CombatGrid
    {
        public int Width { get; } = 12;
        public int Height { get; } = 10;

        public Dictionary<Vector2I, CombatUnit> Units { get; } = new();

        public void Clear() => Units.Clear();

        public Vector2I GetPartyPosition(int slot)
        {
            // Party starts on the left side, spread vertically
            int y = 2 + slot * 2;
            if (y >= Height) y = Height - 1;
            return new Vector2I(1, y);
        }

        public Vector2I GetEnemyPosition(int index, int total)
        {
            // Enemies start on the right side, spread vertically
            int y;
            if (total <= 1)
                y = Height / 2;
            else
                y = 1 + index * (Height - 2) / (total - 1);
            return new Vector2I(Width - 2, y);
        }

        public bool IsOccupied(Vector2I pos) => Units.ContainsKey(pos);

        public bool IsInBounds(Vector2I pos) =>
            pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;

        public int Distance(Vector2I a, Vector2I b) =>
            Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);

        public List<Vector2I> GetAdjacentTiles(Vector2I pos)
        {
            var result = new List<Vector2I>();
            var dirs = new[] { new Vector2I(0, -1), new Vector2I(1, 0), new Vector2I(0, 1), new Vector2I(-1, 0) };
            foreach (var d in dirs)
            {
                var p = pos + d;
                if (IsInBounds(p) && !IsOccupied(p))
                    result.Add(p);
            }
            return result;
        }

        public List<Vector2I> GetTilesInRange(Vector2I origin, int range)
        {
            var result = new List<Vector2I>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var p = new Vector2I(x, y);
                    if (Distance(origin, p) <= range && p != origin && !IsOccupied(p))
                        result.Add(p);
                }
            }
            return result;
        }

        public List<CombatUnit> GetEnemiesInRange(Vector2I origin, int range)
        {
            var result = new List<CombatUnit>();
            foreach (var kv in Units)
            {
                if (!kv.Value.IsPlayer && Distance(origin, kv.Key) <= range)
                    result.Add(kv.Value);
            }
            return result;
        }

        public List<CombatUnit> GetAlliesInRange(Vector2I origin, int range)
        {
            var result = new List<CombatUnit>();
            foreach (var kv in Units)
            {
                if (kv.Value.IsPlayer && Distance(origin, kv.Key) <= range)
                    result.Add(kv.Value);
            }
            return result;
        }

        public void MoveUnit(CombatUnit unit, Vector2I newPos)
        {
            // Remove old position
            var oldPos = unit.GridPosition;
            if (Units.TryGetValue(oldPos, out var u) && u == unit)
                Units.Remove(oldPos);
            unit.GridPosition = newPos;
            Units[newPos] = unit;
        }
    }

    public class CombatUnit
    {
        public Character Character { get; set; }
        public Enemy Enemy { get; set; }
        public Vector2I GridPosition { get; set; }
        public bool IsPlayer => Character != null;
        public bool IsAlive => IsPlayer ? Character.IsAlive : Enemy.IsAlive;
        public string Name => IsPlayer ? Character.Name : Enemy.Name;
        public int CurrentHealth => IsPlayer ? Character.CombatStats.CurrentHealth : Enemy.Health;
        public int MaxHealth => IsPlayer ? Character.CombatStats.MaxHealth : Enemy.MaxHealth;

        // Movement points per turn (DSA-style)
        public int MovementPoints { get; set; } = 3;
        public int MaxMovementPoints { get; set; } = 3;
        public bool HasActed { get; set; } = false;

        public void ResetTurn()
        {
            MovementPoints = MaxMovementPoints;
            HasActed = false;
        }
    }
}
