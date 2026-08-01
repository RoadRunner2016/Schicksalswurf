using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Dungeon
{
    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public static class DirectionExtensions
    {
        public static Direction RotateRight(this Direction dir) => (Direction)(((int)dir + 1) % 4);
        public static Direction RotateLeft(this Direction dir) => (Direction)(((int)dir + 3) % 4);
        public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);

        public static Vector2I ToVector(this Direction dir) => dir switch
        {
            Direction.North => new Vector2I(0, -1),
            Direction.East => new Vector2I(1, 0),
            Direction.South => new Vector2I(0, 1),
            Direction.West => new Vector2I(-1, 0),
            _ => new Vector2I(0, 0)
        };

        public static string ToGerman(this Direction dir) => dir switch
        {
            Direction.North => "Norden",
            Direction.East => "Osten",
            Direction.South => "Sueden",
            Direction.West => "Westen",
            _ => "Unbekannt"
        };
    }

    public enum TileType
    {
        Wall,
        Floor,
        Door,
        StairsUp,
        StairsDown,
        Chest,
        Trap,
        Exit,
        NPC
    }

    /// <summary>
    /// A single tile in the dungeon grid.
    /// </summary>
    public class Tile
    {
        public TileType Type { get; set; } = TileType.Wall;
        public bool IsExplored { get; set; } = false;
        public bool IsWalkable => Type != TileType.Wall;
        public string Note { get; set; } = "";
    }

    /// <summary>
    /// A grid-based dungeon map. Uses a 2D array of tiles.
    /// Coordinates: x = column (east-west), y = row (north-south, 0 = top/north).
    /// </summary>
    public class DungeonMap
    {
        public int Width { get; }
        public int Height { get; }
        public Tile[,] Tiles { get; }
        public string Name { get; set; } = "Dungeon";
        public List<Rect2I> Rooms { get; set; } = new();

        public DungeonMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Tiles[x, y] = new Tile();
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return Tiles[x, y];
        }

        public Tile GetTile(Vector2I pos) => GetTile(pos.X, pos.Y);

        public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsInBounds(Vector2I pos) => IsInBounds(pos.X, pos.Y);

        public bool IsWalkable(int x, int y)
        {
            var tile = GetTile(x, y);
            return tile?.IsWalkable ?? false;
        }

        public bool IsWalkable(Vector2I pos) => IsWalkable(pos.X, pos.Y);

        public void SetTile(int x, int y, TileType type)
        {
            if (IsInBounds(x, y))
                Tiles[x, y].Type = type;
        }

        /// <summary>
        /// Creates a simple test dungeon with rooms and corridors.
        /// </summary>
        public static DungeonMap CreateTestDungeon()
        {
            var map = new DungeonMap(16, 16);

            // Fill with walls (default), then carve rooms

            // Room 1 (start room) - top-left
            CarveRoom(map, 1, 1, 5, 5);
            map.SetTile(3, 1, TileType.StairsUp);
            map.SetTile(3, 3, TileType.Chest);

            // Corridor east
            CarveRoom(map, 6, 3, 3, 1);

            // Room 2 - center
            CarveRoom(map, 9, 1, 5, 5);
            map.SetTile(11, 3, TileType.Trap);

            // Corridor south from room 1
            CarveRoom(map, 3, 6, 1, 3);

            // Room 3 - below room 1
            CarveRoom(map, 1, 9, 5, 5);

            // Corridor east from room 3
            CarveRoom(map, 6, 11, 3, 1);

            // Room 4 - bottom-right
            CarveRoom(map, 9, 9, 5, 5);
            map.SetTile(13, 13, TileType.StairsDown);
            map.SetTile(11, 11, TileType.Chest);

            // Corridor south from room 2
            CarveRoom(map, 11, 6, 1, 3);

            // Doors at room entrances
            map.SetTile(6, 3, TileType.Door);
            map.SetTile(3, 6, TileType.Door);
            map.SetTile(6, 11, TileType.Door);
            map.SetTile(11, 6, TileType.Door);

            map.Name = "Testdungeon - Ebene 1";
            return map;
        }

        private static void CarveRoom(DungeonMap map, int x, int y, int w, int h)
        {
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    map.SetTile(x + dx, y + dy, TileType.Floor);
        }
    }
}
