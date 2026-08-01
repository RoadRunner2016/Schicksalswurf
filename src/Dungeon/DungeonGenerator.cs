using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Dungeon
{
    /// <summary>
    /// Procedural dungeon generator. Creates random layouts with rooms,
    /// corridors, doors, chests, traps, and stairs.
    /// </summary>
    public class DungeonGenerator
    {
        private int _width;
        private int _height;
        private DungeonMap _map;
        private List<Rect2I> _rooms;
        private int _seed;
        private RandomNumberGenerator _rng;

        // Generation parameters
        public int MinRoomSize { get; set; } = 3;
        public int MaxRoomSize { get; set; } = 7;
        public int MaxRooms { get; set; } = 12;
        public float TrapChance { get; set; } = 0.06f;
        public float ChestChance { get; set; } = 0.12f;
        public float DoorChance { get; set; } = 0.5f;

        public DungeonGenerator(int width = 24, int height = 24, int seed = -1)
        {
            _width = width;
            _height = height;
            _rng = new RandomNumberGenerator();
            if (seed >= 0)
            {
                _seed = seed;
                _rng.Seed = (ulong)seed;
            }
            else
            {
                _rng.Randomize();
                _seed = (int)_rng.Seed;
            }
        }

        /// <summary>
        /// Generates a complete dungeon level.
        /// </summary>
        public DungeonMap Generate(int level = 1)
        {
            _map = new DungeonMap(_width, _height);
            _rooms = new List<Rect2I>();

            // Place rooms
            for (int i = 0; i < MaxRooms; i++)
            {
                int roomW = _rng.RandiRange(MinRoomSize, MaxRoomSize);
                int roomH = _rng.RandiRange(MinRoomSize, MaxRoomSize);
                int roomX = _rng.RandiRange(1, _width - roomW - 2);
                int roomY = _rng.RandiRange(1, _height - roomH - 2);

                var newRoom = new Rect2I(roomX, roomY, roomW, roomH);

                // Check overlap with existing rooms (with 1-tile padding)
                bool overlaps = false;
                foreach (var room in _rooms)
                {
                    if (newRoom.Intersects(new Rect2I(
                        room.Position.X - 1, room.Position.Y - 1,
                        room.Size.X + 2, room.Size.Y + 2)))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    CarveRoom(newRoom);

                    // Connect to previous room
                    if (_rooms.Count > 0)
                    {
                        var prevRoom = _rooms[_rooms.Count - 1];
                        ConnectRooms(prevRoom, newRoom);
                    }

                    _rooms.Add(newRoom);
                }
            }

            // Place features
            PlaceFeatures(level);

            // Copy rooms to map
            _map.Rooms = new List<Rect2I>(_rooms);

            // Set start position (center of first room)
            _map.Name = $"Dungeon Level {level} (Seed: {_seed})";

            return _map;
        }

        /// <summary>
        /// Returns the start position (center of first room).
        /// </summary>
        public Vector2I GetStartPosition()
        {
            if (_rooms.Count == 0) return new Vector2I(1, 1);
            var r = _rooms[0];
            return new Vector2I(r.Position.X + r.Size.X / 2, r.Position.Y + r.Size.Y / 2);
        }

        /// <summary>
        /// Returns the exit position (center of last room).
        /// </summary>
        public Vector2I GetExitPosition()
        {
            if (_rooms.Count == 0) return new Vector2I(_width - 2, _height - 2);
            var r = _rooms[_rooms.Count - 1];
            return new Vector2I(r.Position.X + r.Size.X / 2, r.Position.Y + r.Size.Y / 2);
        }

        private void CarveRoom(Rect2I room)
        {
            for (int x = room.Position.X; x < room.Position.X + room.Size.X; x++)
            {
                for (int y = room.Position.Y; y < room.Position.Y + room.Size.Y; y++)
                {
                    _map.SetTile(x, y, TileType.Floor);
                }
            }
        }

        private void ConnectRooms(Rect2I roomA, Rect2I roomB)
        {
            // Center points
            int ax = roomA.Position.X + roomA.Size.X / 2;
            int ay = roomA.Position.Y + roomA.Size.Y / 2;
            int bx = roomB.Position.X + roomB.Size.X / 2;
            int by = roomB.Position.Y + roomB.Size.Y / 2;

            // Randomly choose L-shaped or straight corridor
            bool horizontalFirst = _rng.Randf() < 0.5f;

            if (horizontalFirst)
            {
                CarveHorizontalTunnel(ax, bx, ay);
                CarveVerticalTunnel(ay, by, bx);
            }
            else
            {
                CarveVerticalTunnel(ay, by, ax);
                CarveHorizontalTunnel(ax, bx, by);
            }
        }

        private void CarveHorizontalTunnel(int x1, int x2, int y)
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);

            for (int x = minX; x <= maxX; x++)
            {
                if (_map.IsInBounds(x, y))
                {
                    // Don't overwrite existing floor with doors here
                    if (_map.GetTile(x, y).Type == TileType.Wall)
                        _map.SetTile(x, y, TileType.Floor);
                }
            }
        }

        private void CarveVerticalTunnel(int y1, int y2, int x)
        {
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);

            for (int y = minY; y <= maxY; y++)
            {
                if (_map.IsInBounds(x, y))
                {
                    if (_map.GetTile(x, y).Type == TileType.Wall)
                        _map.SetTile(x, y, TileType.Floor);
                }
            }
        }

        private void PlaceFeatures(int level)
        {
            if (_rooms.Count == 0) return;

            // First room: stairs up (entrance)
            var firstRoom = _rooms[0];
            var startPos = new Vector2I(
                firstRoom.Position.X + firstRoom.Size.X / 2,
                firstRoom.Position.Y + firstRoom.Size.Y / 2
            );
            _map.SetTile(startPos.X, startPos.Y, TileType.StairsUp);

            // Last room: stairs down (exit)
            var lastRoom = _rooms[_rooms.Count - 1];
            var exitPos = new Vector2I(
                lastRoom.Position.X + lastRoom.Size.X / 2,
                lastRoom.Position.Y + lastRoom.Size.Y / 2
            );
            _map.SetTile(exitPos.X, exitPos.Y, TileType.StairsDown);

            // Place doors at room entrances (where corridors meet rooms)
            PlaceDoors();

            // Place chests and traps in rooms (not first or last)
            for (int i = 1; i < _rooms.Count - 1; i++)
            {
                var room = _rooms[i];

                // Chest
                if (_rng.Randf() < ChestChance)
                {
                    int cx = _rng.RandiRange(room.Position.X, room.Position.X + room.Size.X - 1);
                    int cy = _rng.RandiRange(room.Position.Y, room.Position.Y + room.Size.Y - 1);
                    if (_map.GetTile(cx, cy).Type == TileType.Floor)
                        _map.SetTile(cx, cy, TileType.Chest);
                }

                // Traps
                for (int x = room.Position.X; x < room.Position.X + room.Size.X; x++)
                {
                    for (int y = room.Position.Y; y < room.Position.Y + room.Size.Y; y++)
                    {
                        if (_map.GetTile(x, y).Type == TileType.Floor && _rng.Randf() < TrapChance)
                            _map.SetTile(x, y, TileType.Trap);
                    }
                }
            }
        }

        private void PlaceDoors()
        {
            // Find floor tiles adjacent to walls on two opposite sides (door candidates)
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    var tile = _map.GetTile(x, y);
                    if (tile.Type != TileType.Floor) continue;

                    // Check if this is a corridor entry point (walls on 2 opposite sides, floor on other 2)
                    bool wallN = _map.GetTile(x, y - 1).Type == TileType.Wall;
                    bool wallS = _map.GetTile(x, y + 1).Type == TileType.Wall;
                    bool wallE = _map.GetTile(x + 1, y).Type == TileType.Wall;
                    bool wallW = _map.GetTile(x - 1, y).Type == TileType.Wall;

                    bool isHorizontalCorridor = wallN && wallS && !wallE && !wallW;
                    bool isVerticalCorridor = wallE && wallW && !wallN && !wallS;

                    if ((isHorizontalCorridor || isVerticalCorridor) && _rng.Randf() < DoorChance)
                    {
                        // Check that it's at a room boundary (one side leads to a room)
                        bool leadsToRoom = false;
                        if (isHorizontalCorridor)
                        {
                            leadsToRoom = IsPartOfRoom(x - 1, y) || IsPartOfRoom(x + 1, y);
                        }
                        else
                        {
                            leadsToRoom = IsPartOfRoom(x, y - 1) || IsPartOfRoom(x, y + 1);
                        }

                        if (leadsToRoom)
                            _map.SetTile(x, y, TileType.Door);
                    }
                }
            }
        }

        private bool IsPartOfRoom(int x, int y)
        {
            foreach (var room in _rooms)
            {
                if (x >= room.Position.X && x < room.Position.X + room.Size.X &&
                    y >= room.Position.Y && y < room.Position.Y + room.Size.Y)
                    return true;
            }
            return false;
        }
    }
}
