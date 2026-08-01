using Godot;

namespace Schicksalswurf.Dungeon
{
    using Characters;
    using Combat;

    /// <summary>
    /// Main dungeon controller. Handles grid-based movement, rendering,
    /// and triggers encounters. Attach to a Node2D or Control node.
    /// </summary>
    public partial class DungeonController : Node2D
    {
        public DungeonMap Map { get; set; }
        public Vector2I GridPosition { get; set; }
        public Direction Facing { get; set; } = Direction.North;
        public Party Party { get; set; }

        // Rendering
        private TileMapLayer _floorLayer;
        private TileMapLayer _wallLayer;
        private Label _positionLabel;
        private Label _messageLabel;

        // Combat
        private CombatManager _combat;
        private bool _inCombat = false;

        // Messages
        private string _currentMessage = "";
        private float _messageTimer = 0;

        // Tile size in pixels
        private const int TileSize = 64;

        public override void _Ready()
        {
            // Create tile map layers for rendering
            _floorLayer = new TileMapLayer();
            _wallLayer = new TileMapLayer();
            AddChild(_floorLayer);
            AddChild(_wallLayer);

            // UI labels
            _positionLabel = new Label();
            _positionLabel.Position = new Vector2(10, 10);
            _positionLabel.AddThemeFontSizeOverride("font_size", 16);
            AddChild(_positionLabel);

            _messageLabel = new Label();
            _messageLabel.Position = new Vector2(10, 40);
            _messageLabel.AddThemeFontSizeOverride("font_size", 14);
            _messageLabel.Size = new Vector2(800, 30);
            AddChild(_messageLabel);

            // Initialize test dungeon
            Map = DungeonMap.CreateTestDungeon();
            GridPosition = new Vector2I(3, 3);
            Facing = Direction.North;

            // Create test party
            Party = new Party();
            Party.AddMember(new Character("Aldric", Archetype.Krieger));
            Party.AddMember(new Character("Lyra", Archetype.Schurke));
            Party.AddMember(new Character("Mavros", Archetype.Magier));
            Party.AddMember(new Character("Sera", Archetype.Heiler));

            ShowMessage("Willkommen im Dungeon! Bewege dich mit WASD.");
            UpdateDisplay();
        }

        public override void _Process(double delta)
        {
            if (_messageTimer > 0)
            {
                _messageTimer -= (float)delta;
                if (_messageTimer <= 0)
                {
                    _currentMessage = "";
                    _messageLabel.Text = "";
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_inCombat) return;

            if (@event.IsActionPressed("move_forward"))
                TryMove(Facing);
            else if (@event.IsActionPressed("move_backward"))
                TryMove(Facing.Opposite());
            else if (@event.IsActionPressed("turn_left"))
                TurnLeft();
            else if (@event.IsActionPressed("turn_right"))
                TurnRight();
            else if (@event.IsActionPressed("strafe_left"))
                TryMove(Facing.RotateLeft());
            else if (@event.IsActionPressed("strafe_right"))
                TryMove(Facing.RotateRight());
            else if (@event.IsActionPressed("interact"))
                Interact();
        }

        private void TryMove(Direction dir)
        {
            Vector2I newPos = GridPosition + dir.ToVector();

            if (!Map.IsInBounds(newPos))
            {
                ShowMessage("Eine Wand versperrt den Weg.");
                return;
            }

            var tile = Map.GetTile(newPos);
            if (!tile.IsWalkable)
            {
                ShowMessage("Eine Wand versperrt den Weg.");
                return;
            }

            // Mark previous tile as explored
            Map.GetTile(GridPosition).IsExplored = true;

            GridPosition = newPos;
            Facing = dir;

            // Handle tile effects
            switch (tile.Type)
            {
                case TileType.Trap:
                    ShowMessage("Eine Falle! Du verlierst Lebenspunkte.");
                    var victim = Party.Members[0];
                    victim.TakeDamage(5);
                    tile.Type = TileType.Floor;
                    break;
                case TileType.Chest:
                    ShowMessage("Du findest eine Truhe mit 20 Gold!");
                    Party.Gold += 20;
                    tile.Type = TileType.Floor;
                    break;
                case TileType.StairsDown:
                    ShowMessage("Treppe nach unten. (Noch nicht implementiert)");
                    break;
                case TileType.StairsUp:
                    ShowMessage("Treppe nach oben. (Noch nicht implementiert)");
                    break;
            }

            // Random encounter chance (15%)
            if (tile.Type == TileType.Floor && GD.Randf() < 0.15f)
            {
                StartRandomEncounter();
            }

            UpdateDisplay();
        }

        private void TurnLeft()
        {
            Facing = Facing.RotateLeft();
            ShowMessage($"Du blickst nach {Facing.ToGerman()}.");
            UpdateDisplay();
        }

        private void TurnRight()
        {
            Facing = Facing.RotateRight();
            ShowMessage($"Du blickst nach {Facing.ToGerman()}.");
            UpdateDisplay();
        }

        private void Interact()
        {
            Vector2I targetPos = GridPosition + Facing.ToVector();
            var tile = Map.GetTile(targetPos);

            if (tile == null) return;

            switch (tile.Type)
            {
                case TileType.Door:
                    tile.Type = TileType.Floor;
                    ShowMessage("Du oeffnest die Tuer.");
                    break;
                case TileType.Chest:
                    ShowMessage("Du oeffnest die Truhe: 20 Gold!");
                    Party.Gold += 20;
                    tile.Type = TileType.Floor;
                    break;
                default:
                    ShowMessage("Hier gibt es nichts zu tun.");
                    break;
            }

            UpdateDisplay();
        }

        private void StartRandomEncounter()
        {
            var encounter = new Encounter();
            int enemyCount = GD.RandRange(1, 3);

            for (int i = 0; i < enemyCount; i++)
            {
                var template = Enemy.Bestiary[GD.RandRange(0, 3)]; // weak enemies
                encounter.AddEnemy(template);
            }

            _combat = new CombatManager(Party, encounter);
            _inCombat = true;
            _combat.StartCombat();

            ShowMessage($"Kampf! {enemyCount} Gegner erscheinen!");

            // For now, auto-resolve combat (prototype)
            // In the full game, this would open a combat UI
            CallDeferred(nameof(AutoResolveCombat));
        }

        private void AutoResolveCombat()
        {
            // Simple auto-resolution for prototype
            while (_combat.Phase != CombatPhase.Victory && _combat.Phase != CombatPhase.Defeat)
            {
                var current = _combat.CurrentParticipant;
                if (current == null) break;

                if (current.IsPlayer)
                {
                    // Attack first alive enemy
                    var target = _combat.Encounter.AliveEnemies as System.Collections.Generic.IEnumerable<Enemy>;
                    var enemyList = new System.Collections.Generic.List<Enemy>(_combat.Encounter.AliveEnemies);
                    if (enemyList.Count > 0)
                        _combat.PlayerAttack(current.Character, enemyList[0]);
                }
                else
                {
                    // Enemy turn is handled automatically in AdvanceToNextParticipant
                    _combat.AdvanceToNextParticipant();
                }
            }

            _inCombat = false;

            if (_combat.Phase == CombatPhase.Victory)
            {
                ShowMessage("Kampf gewonnen!");
                foreach (var line in _combat.Log)
                    GD.Print($"[Combat] {line}");
            }
            else
            {
                ShowMessage("Die Gruppe wurde besiegt...");
            }

            _combat = null;
            UpdateDisplay();
        }

        private void ShowMessage(string msg)
        {
            _currentMessage = msg;
            _messageLabel.Text = msg;
            _messageTimer = 4.0f;
            GD.Print($"[Dungeon] {msg}");
        }

        private void UpdateDisplay()
        {
            // Update position label
            _positionLabel.Text = $"Pos: ({GridPosition.X}, {GridPosition.Y})  Blick: {Facing.ToGerman()}  Gold: {Party.Gold}";

            // Render the dungeon map (top-down view for prototype)
            RenderMap();
        }

        private void RenderMap()
        {
            _floorLayer.Clear();
            _wallLayer.Clear();

            // Simple colored rectangle rendering using _Draw override
            // For prototype, we just update the label
            // Full first-person rendering would use 3D or textured walls
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (Map == null) return;

            // Draw a top-down view of the dungeon
            var wallColor = new Color(0.2f, 0.2f, 0.3f);
            var floorColor = new Color(0.5f, 0.5f, 0.6f);
            var exploredColor = new Color(0.3f, 0.3f, 0.4f);
            var playerColor = new Color(1.0f, 0.8f, 0.2f);
            var doorColor = new Color(0.6f, 0.4f, 0.2f);
            var chestColor = new Color(1.0f, 0.8f, 0.0f);
            var stairsColor = new Color(0.2f, 0.8f, 0.2f);
            var trapColor = new Color(0.8f, 0.2f, 0.2f);

            int offsetX = 200;
            int offsetY = 80;

            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    var tile = Map.Tiles[x, y];
                    Color color;

                    if (!tile.IsExplored && !IsVisibleToPlayer(x, y))
                    {
                        color = new Color(0.05f, 0.05f, 0.08f);
                    }
                    else if (tile.Type == TileType.Wall)
                    {
                        color = tile.IsExplored ? wallColor : new Color(0.1f, 0.1f, 0.15f);
                    }
                    else
                    {
                        color = floorColor;

                        if (tile.Type == TileType.Door) color = doorColor;
                        else if (tile.Type == TileType.Chest) color = chestColor;
                        else if (tile.Type == TileType.StairsUp || tile.Type == TileType.StairsDown) color = stairsColor;
                        else if (tile.Type == TileType.Trap) color = trapColor;
                    }

                    var rect = new Rect2(
                        offsetX + x * TileSize,
                        offsetY + y * TileSize,
                        TileSize - 1,
                        TileSize - 1
                    );
                    DrawRect(rect, color);

                    // Mark explored tiles
                    if (IsVisibleToPlayer(x, y))
                        tile.IsExplored = true;
                }
            }

            // Draw player position
            var playerRect = new Rect2(
                offsetX + GridPosition.X * TileSize,
                offsetY + GridPosition.Y * TileSize,
                TileSize - 1,
                TileSize - 1
            );
            DrawRect(playerRect, playerColor);

            // Draw facing direction indicator
            var facingVec = Facing.ToVector();
            var arrowStart = new Vector2(
                offsetX + GridPosition.X * TileSize + TileSize / 2,
                offsetY + GridPosition.Y * TileSize + TileSize / 2
            );
            var arrowEnd = arrowStart + new Vector2(facingVec.X, facingVec.Y) * TileSize * 0.4f;
            DrawLine(arrowStart, arrowEnd, new Color(1, 0, 0), 3);
        }

        /// <summary>
        /// Simple line-of-sight check: visible if within 3 tiles and roughly in front.
        /// </summary>
        private bool IsVisibleToPlayer(int x, int y)
        {
            int dx = x - GridPosition.X;
            int dy = y - GridPosition.Y;
            int dist = Mathf.Abs(dx) + Mathf.Abs(dy);

            if (dist == 0) return true;
            if (dist > 4) return false;

            // Visible if in front arc (roughly)
            var facingVec = Facing.ToVector();
            int dot = dx * facingVec.X + dy * facingVec.Y;

            // Also see adjacent tiles
            if (dist <= 1) return true;

            return dot > 0;
        }
    }
}
