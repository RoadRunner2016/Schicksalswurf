using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.UI
{
    using Dungeon;

    /// <summary>
    /// Minimap showing explored dungeon tiles.
    /// </summary>
    public partial class MinimapUI : Control
    {
        private DungeonMap _map;
        private Vector2I _playerPos;
        private Direction _facing;
        private int _tileSize = 6;
        private int _margin = 2;

        private static readonly Color WallColor = new(0.3f, 0.25f, 0.15f);
        private static readonly Color FloorColor = new(0.15f, 0.15f, 0.2f);
        private static readonly Color ExploredColor = new(0.25f, 0.22f, 0.18f);
        private static readonly Color PlayerColor = new(1.0f, 0.85f, 0.2f);
        private static readonly Color DoorColor = new(0.6f, 0.4f, 0.2f);
        private static readonly Color ChestColor = new(0.9f, 0.7f, 0.2f);
        private static readonly Color StairsDownColor = new(0.2f, 0.8f, 0.3f);
        private static readonly Color StairsUpColor = new(0.3f, 0.5f, 0.9f);
        private static readonly Color TrapColor = new(0.8f, 0.2f, 0.2f);
        private static readonly Color NpcColor = new(0.7f, 0.6f, 0.9f);
        private static readonly Color UnexploredColor = new(0.05f, 0.05f, 0.08f);

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            ZIndex = 10;
        }

        public void Update(DungeonMap map, Vector2I playerPos, Direction facing)
        {
            _map = map;
            _playerPos = playerPos;
            _facing = facing;
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (_map == null) return;

            int mapW = _map.Width;
            int mapH = _map.Height;
            int totalW = mapW * _tileSize + _margin * 2;
            int totalH = mapH * _tileSize + _margin * 2;

            // Background
            DrawRect(new Rect2(0, 0, totalW, totalH), new Color(0.02f, 0.02f, 0.05f, 0.8f), true);

            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    var tile = _map.GetTile(x, y);
                    var rect = new Rect2(
                        _margin + x * _tileSize,
                        _margin + y * _tileSize,
                        _tileSize, _tileSize
                    );

                    Color color;
                    if (!tile.IsExplored)
                    {
                        color = UnexploredColor;
                    }
                    else
                    {
                        color = tile.Type switch
                        {
                            TileType.Wall => WallColor,
                            TileType.Floor => ExploredColor,
                            TileType.Door => DoorColor,
                            TileType.Chest => ChestColor,
                            TileType.StairsDown => StairsDownColor,
                            TileType.StairsUp => StairsUpColor,
                            TileType.Trap => TrapColor,
                            TileType.NPC => NpcColor,
                            _ => FloorColor
                        };
                    }

                    DrawRect(rect, color, true);
                }
            }

            // Draw player position
            var playerRect = new Rect2(
                _margin + _playerPos.X * _tileSize,
                _margin + _playerPos.Y * _tileSize,
                _tileSize, _tileSize
            );
            DrawRect(playerRect, PlayerColor, true);

            // Draw facing direction indicator
            var dirVec = _facing.ToVector();
            var center = playerRect.Position + new Vector2(_tileSize / 2f, _tileSize / 2f);
            var end = center + new Vector2(dirVec.X, dirVec.Y) * _tileSize;
            DrawLine(center, end, PlayerColor, 2f);
        }
    }
}
