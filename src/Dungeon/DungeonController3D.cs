using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Dungeon
{
    using Characters;
    using Combat;
    using Core;
    using UI;

    /// <summary>
    /// Main 3D dungeon controller. Handles grid-based movement with first-person
    /// camera, 3D rendering, and triggers encounters.
    /// </summary>
    public partial class DungeonController3D : Node3D
    {
        public DungeonMap Map { get; set; }
        public Vector2I GridPosition { get; set; }
        public Direction Facing { get; set; } = Direction.North;
        public Party Party { get; set; }

        private DungeonRenderer3D _renderer;
        private FirstPersonCamera _camera;

        // UI
        private CanvasLayer _uiLayer;
        private Label _positionLabel;
        private Label _messageLabel;
        private Label _partyLabel;
        private MinimapUI _minimap;

        // Combat
        private CombatManager _combat;
        private bool _inCombat = false;
        private CombatUI _combatUI;
        private CombatScene3D _combatScene;
        private bool _combatSceneActive = false;
        private float _enemyTurnDelay = 0;

        // Character sheet
        private CharacterSheetUI _charSheetUI;
        private InventoryUI _inventoryUI;
        private CharacterCreationUI _charCreationUI;
        private DialogueUI _dialogueUI;
        private HelpUI _helpUI;
        private MainMenuUI _mainMenuUI;
        private TownUI _townUI;
        private SettingsUI _settingsUI;
        private GameOverUI _gameOverUI;
        private LoadingScreenUI _loadingScreen;
        private AchievementsUI _achievementsUI;
        private CraftingUI _craftingUI;
        private bool _creationDone = false;
        private bool _gameStarted = false;

        // Core systems
        private SoundSystem _sound;
        private ParticleEffects _particles;
        private DayNightCycle _dayNight;
        private AmbientSoundSystem _ambientSound;
        private GameStats _stats;
        private SurvivalSystem.SurvivalState _survival;

        // Dungeon level
        private int _currentLevel = 1;
        private DungeonGenerator _generator;
        private DungeonTheme _currentTheme;

        // Trap tracking
        private Dictionary<Vector2I, TrapSystem.TrapInfo> _traps = new();

        // NPC tracking
        private Dictionary<Vector2I, NPC> _npcs = new();

        // Messages
        private float _messageTimer = 0;

        // Input cooldown to prevent spamming during animation
        private float _inputCooldown = 0;

        public override void _Ready()
        {
            // Setup renderer
            _renderer = new DungeonRenderer3D { Name = "DungeonRenderer" };
            AddChild(_renderer);

            // Setup camera
            _camera = new FirstPersonCamera { Name = "Camera" };
            AddChild(_camera);

            // Setup UI first (creation screen needs it)
            SetupUI();
            SetupCombatUI();
            SetupCharacterSheetUI();
            SetupInventoryUI();
            SetupCharacterCreationUI();
            SetupDialogueUI();
            SetupHelpUI();
            SetupMainMenuUI();
            SetupTownUI();
            SetupSettingsUI();
            SetupGameOverUI();
            SetupLoadingScreen();
            SetupAchievementsUI();
            SetupCraftingUI();

            // Core systems
            _sound = new SoundSystem { Name = "SoundSystem" };
            AddChild(_sound);
            _particles = new ParticleEffects { Name = "ParticleEffects" };
            AddChild(_particles);
            _dayNight = new DayNightCycle { Name = "DayNightCycle" };
            AddChild(_dayNight);
            _ambientSound = new AmbientSoundSystem { Name = "AmbientSound" };
            AddChild(_ambientSound);
            _stats = new GameStats();
            _stats.InitializeAchievements();
            _survival = new SurvivalSystem.SurvivalState();

            // Connect static references for combat UI
            CombatUI.Sound = _sound;
            CombatUI.Particles = _particles;

            // Initialize quests
            QuestRegistry.Initialize();

            // Controller support
            var controllerSystem = new ControllerSystem { Name = "ControllerSystem" };
            AddChild(controllerSystem);

            // Show main menu first
            _mainMenuUI.Visible = true;
        }

        private void SetupUI()
        {
            _uiLayer = new CanvasLayer();
            AddChild(_uiLayer);

            // Position label (top-left)
            _positionLabel = new Label();
            _positionLabel.Position = new Vector2(10, 10);
            _positionLabel.AddThemeFontSizeOverride("font_size", 16);
            _positionLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0.8f));
            _uiLayer.AddChild(_positionLabel);

            // Message label (center-bottom)
            _messageLabel = new Label();
            _messageLabel.AnchorRight = 1.0f;
            _messageLabel.AnchorLeft = 0.0f;
            _messageLabel.OffsetTop = 660;
            _messageLabel.OffsetBottom = 700;
            _messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _messageLabel.AddThemeFontSizeOverride("font_size", 18);
            _messageLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.6f));
            _uiLayer.AddChild(_messageLabel);

            // Party status label (bottom-left)
            _partyLabel = new Label();
            _partyLabel.OffsetLeft = 10;
            _partyLabel.OffsetTop = 660;
            _partyLabel.OffsetRight = 410;
            _partyLabel.OffsetBottom = 720;
            _partyLabel.AddThemeFontSizeOverride("font_size", 13);
            _partyLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _uiLayer.AddChild(_partyLabel);
            // Minimap (top-right)
            _minimap = new MinimapUI();
            _minimap.Position = new Vector2(1000, 10);
            _uiLayer.AddChild(_minimap);
        }

        private void SetupCombatUI()
        {
            _combatUI = new CombatUI { Name = "CombatUI" };
            _uiLayer.AddChild(_combatUI);
        }

        private void SetupCharacterSheetUI()
        {
            _charSheetUI = new CharacterSheetUI { Name = "CharacterSheetUI" };
            _uiLayer.AddChild(_charSheetUI);
        }

        private void SetupInventoryUI()
        {
            _inventoryUI = new InventoryUI { Name = "InventoryUI" };
            _uiLayer.AddChild(_inventoryUI);
        }

        private void SetupCharacterCreationUI()
        {
            _charCreationUI = new CharacterCreationUI { Name = "CharacterCreationUI" };
            _uiLayer.AddChild(_charCreationUI);
        }

        private void SetupDialogueUI()
        {
            _dialogueUI = new DialogueUI { Name = "DialogueUI" };
            _uiLayer.AddChild(_dialogueUI);
        }

        private void SetupHelpUI()
        {
            _helpUI = new HelpUI { Name = "HelpUI" };
            _uiLayer.AddChild(_helpUI);
        }

        private void SetupMainMenuUI()
        {
            _mainMenuUI = new MainMenuUI { Name = "MainMenuUI" };
            _uiLayer.AddChild(_mainMenuUI);
        }

        private void SetupTownUI()
        {
            _townUI = new TownUI { Name = "TownUI" };
            _uiLayer.AddChild(_townUI);
        }

        private void SetupSettingsUI()
        {
            _settingsUI = new SettingsUI { Name = "SettingsUI" };
            _uiLayer.AddChild(_settingsUI);
        }

        private void SetupGameOverUI()
        {
            _gameOverUI = new GameOverUI { Name = "GameOverUI" };
            _uiLayer.AddChild(_gameOverUI);
        }

        private void SetupLoadingScreen()
        {
            _loadingScreen = new LoadingScreenUI { Name = "LoadingScreen" };
            _uiLayer.AddChild(_loadingScreen);
        }

        private void SetupAchievementsUI()
        {
            _achievementsUI = new AchievementsUI { Name = "AchievementsUI" };
            _uiLayer.AddChild(_achievementsUI);
        }

        private void SetupCraftingUI()
        {
            _craftingUI = new CraftingUI { Name = "CraftingUI" };
            _uiLayer.AddChild(_craftingUI);
        }

        private void StartGame()
        {
            Party = _charCreationUI.CreatedParty;
            _creationDone = true;
            _gameStarted = true;
            _charCreationUI.QueueFree();
            _charCreationUI = null;

            // Start quests
            QuestRegistry.StartQuest("clear_level_1");
            QuestRegistry.StartQuest("kill_goblins");

            // Initialize systems
            CraftingSystem.Initialize();
            LoreSystem.Initialize();
            LoreSystem.Discover("world_intro");
            LoreSystem.DiscoverByLocation(1);

            // Start ambient sound
            _ambientSound?.PlayDungeonAmbient();

            GenerateLevel();

            ShowMessage("Willkommen im Dungeon! WASD = Bewegen, C = Charakter, I = Inventar, R = Rasten, H = Hilfe.");
            UpdateDisplay();
        }

        private void GenerateLevel()
        {
            _currentTheme = DungeonThemes.GetThemeByLevel(_currentLevel);
            _generator = new DungeonGenerator(24, 24);
            Map = _generator.Generate(_currentLevel);
            GridPosition = _generator.GetStartPosition();
            Facing = Direction.North;

            // Clear and regenerate traps
            _traps.Clear();
            _npcs.Clear();

            // Place traps on trap tiles
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    var tile = Map.GetTile(x, y);
                    if (tile.Type == TileType.Trap)
                        _traps[new Vector2I(x, y)] = TrapSystem.GenerateTrap(_currentLevel);
                }
            }

            // Place NPCs in some rooms
            PlaceNPCs();

            // Check quest progress
            QuestRegistry.OnLevelReached(_currentLevel);

            _renderer.Map = Map;
            _renderer.Theme = _currentTheme;
            _renderer.BuildGeometry();

            Vector3 camPos = _renderer.GridToWorldPos(GridPosition);
            float camRot = DungeonRenderer3D.DirectionToRotation(Facing);
            _camera.SnapToPosition(camPos, camRot);
            _renderer.UpdatePlayerLight(camPos);
        }

        private void PlaceNPCs()
        {
            // Place a merchant in a random room (not first or last)
            var rng = new RandomNumberGenerator();
            rng.Randomize();

            if (rng.Randf() < 0.4f && Map.Rooms.Count > 2)
            {
                int roomIdx = rng.RandiRange(1, Map.Rooms.Count - 2);
                var room = Map.Rooms[roomIdx];
                int nx = rng.RandiRange(room.Position.X, room.Position.X + room.Size.X - 1);
                int ny = rng.RandiRange(room.Position.Y, room.Position.Y + room.Size.Y - 1);
                if (Map.GetTile(nx, ny).Type == TileType.Floor)
                {
                    Map.SetTile(nx, ny, TileType.NPC);
                    _npcs[new Vector2I(nx, ny)] = NPCFactory.CreateMerchant();
                }
            }

            // Place a healer occasionally
            if (rng.Randf() < 0.25f && Map.Rooms.Count > 3)
            {
                int roomIdx = rng.RandiRange(1, Map.Rooms.Count - 2);
                var room = Map.Rooms[roomIdx];
                int nx = rng.RandiRange(room.Position.X, room.Position.X + room.Size.X - 1);
                int ny = rng.RandiRange(room.Position.Y, room.Position.Y + room.Size.Y - 1);
                if (Map.GetTile(nx, ny).Type == TileType.Floor)
                {
                    Map.SetTile(nx, ny, TileType.NPC);
                    _npcs[new Vector2I(nx, ny)] = NPCFactory.CreateHealer();
                }
            }

            // Place a quest giver on level 1
            if (_currentLevel == 1 && rng.Randf() < 0.5f && Map.Rooms.Count > 2)
            {
                int roomIdx = rng.RandiRange(1, Map.Rooms.Count - 2);
                var room = Map.Rooms[roomIdx];
                int nx = rng.RandiRange(room.Position.X, room.Position.X + room.Size.X - 1);
                int ny = rng.RandiRange(room.Position.Y, room.Position.Y + room.Size.Y - 1);
                if (Map.GetTile(nx, ny).Type == TileType.Floor)
                {
                    Map.SetTile(nx, ny, TileType.NPC);
                    _npcs[new Vector2I(nx, ny)] = NPCFactory.CreateQuestGiver();
                }
            }
        }

        public override void _Process(double delta)
        {
            // Check main menu
            if (!_gameStarted && _mainMenuUI != null)
            {
                if (_mainMenuUI.StartRequested)
                {
                    _mainMenuUI.QueueFree();
                    _mainMenuUI = null;
                    _charCreationUI.Visible = true;
                }
                else if (_mainMenuUI.LoadRequested && SaveSystem.HasSaveFile())
                {
                    LoadGame();
                }
                return;
            }

            // Check if character creation is done
            if (!_creationDone && _charCreationUI != null && _charCreationUI.IsDone)
            {
                StartGame();
            }

            if (!_creationDone) return;

            // Track play time
            if (_stats != null)
                _stats.AddTime((float)delta);

            // Check for game over
            if (_gameStarted && !_inCombat && Party != null)
            {
                bool allDead = true;
                foreach (var m in Party.Members)
                {
                    if (m.CombatStats.CurrentHealth > 0)
                    {
                        allDead = false;
                        break;
                    }
                }
                if (allDead && (_gameOverUI == null || !_gameOverUI.IsActive))
                {
                    _ambientSound?.StopAll();
                    _gameOverUI?.ShowGameOver(Party, _currentLevel, _stats);
                }
            }

            // Check game over restart
            if (_gameOverUI != null && !_gameOverUI.IsActive)
            {
                if (_gameOverUI.RestartRequested || _gameOverUI.MainMenuRequested)
                {
                    _gameOverUI.RestartRequested = false;
                    _gameOverUI.MainMenuRequested = false;
                    GetTree().ReloadCurrentScene();
                }
            }

            // Update minimap
            if (_minimap != null && Map != null)
                _minimap.Update(Map, GridPosition, Facing);

            // Update ambient light from day/night cycle
            if (_dayNight != null && _dayNight.IsEnabled)
            {
                var ambientColor = _dayNight.GetAmbientColor();
                // Could update WorldEnvironment here if available
            }

            // Check dialogue requests
            if (_dialogueUI != null && !_dialogueUI.IsActive)
            {
                if (_dialogueUI.ShopRequested)
                {
                    _dialogueUI.ShopRequested = false;
                    _inventoryUI.SetParty(Party);
                    _inventoryUI.Toggle(true);
                }
                if (_dialogueUI.QuestToStart != null)
                {
                    QuestRegistry.StartQuest(_dialogueUI.QuestToStart);
                    ShowMessage("Quest angenommen!");
                    _dialogueUI.QuestToStart = null;
                }
            }

            // Check town requests
            if (_townUI != null && !_townUI.IsActive)
            {
                if (_townUI.EnterDungeonRequested)
                {
                    _townUI.EnterDungeonRequested = false;
                    _currentLevel = 1;
                    GenerateLevel();
                    ShowMessage("Du betrittst erneut das Dungeon...");
                    UpdateDisplay();
                }
                if (_townUI.ShopRequested)
                {
                    _townUI.ShopRequested = false;
                    _inventoryUI.SetParty(Party);
                    _inventoryUI.Toggle(true);
                }
            }

            if (_messageTimer > 0)
            {
                _messageTimer -= (float)delta;
                if (_messageTimer <= 0)
                    _messageLabel.Text = "";
            }

            if (_inputCooldown > 0)
                _inputCooldown -= (float)delta;

            // Handle enemy turns with a small delay for readability
            if (_inCombat && _combat != null && _combat.Phase == CombatPhase.EnemyTurn)
            {
                _enemyTurnDelay -= (float)delta;
                if (_enemyTurnDelay <= 0)
                {
                    _enemyTurnDelay = 0.8f;
                    _combat.AdvanceToNextParticipant();
                    if (_combatUI != null)
                        _combatUI.StartCombat(_combat); // refresh
                    CheckCombatEnd();
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!_creationDone) return;

            if (_inCombat || _camera.IsAnimating || _inputCooldown > 0)
                return;

            if (_charSheetUI != null && _charSheetUI.IsActive)
                return;

            if (_inventoryUI != null && _inventoryUI.IsActive)
                return;

            if (_dialogueUI != null && _dialogueUI.IsActive)
                return;

            if (_helpUI != null && _helpUI.IsActive)
                return;

            if (_townUI != null && _townUI.IsActive)
                return;

            if (_settingsUI != null && _settingsUI.IsActive)
                return;

            if (_gameOverUI != null && _gameOverUI.IsActive)
                return;

            if (_achievementsUI != null && _achievementsUI.IsActive)
                return;

            if (_craftingUI != null && _craftingUI.IsActive)
                return;

            // H = help
            if (@event is InputEventKey hKey && hKey.Pressed && hKey.Keycode == Key.H)
            {
                _helpUI.Toggle();
                return;
            }

            // F2 = settings
            if (@event is InputEventKey setKey && setKey.Pressed && setKey.Keycode == Key.F2)
            {
                _settingsUI.Toggle();
                return;
            }

            // F3 = achievements
            if (@event is InputEventKey achKey && achKey.Pressed && achKey.Keycode == Key.F3)
            {
                _stats.CheckAchievements();
                _achievementsUI.Show(_stats);
                return;
            }

            // F4 = crafting
            if (@event is InputEventKey craftKey && craftKey.Pressed && craftKey.Keycode == Key.F4)
            {
                _craftingUI.Show(Party);
                return;
            }

            // F5 = save, F9 = load
            if (@event is InputEventKey sKey && sKey.Pressed && sKey.Keycode == Key.F5)
            {
                SaveGame();
                return;
            }
            if (@event is InputEventKey lKey && lKey.Pressed && lKey.Keycode == Key.F9)
            {
                if (SaveSystem.HasSaveFile())
                    LoadGame();
                else
                    ShowMessage("Kein Spielstand vorhanden.");
                return;
            }

            if (@event.IsActionPressed("move_forward"))
                TryMove(Facing);
            else if (@event.IsActionPressed("move_backward"))
                TryMove(Facing.Opposite());
            else if (@event.IsActionPressed("turn_left"))
                TurnLeft();
            else if (@event.IsActionPressed("turn_right"))
                TurnRight();
            else if (@event.IsActionPressed("strafe_left"))
                TryMove(Facing.RotateLeft(), false);
            else if (@event.IsActionPressed("strafe_right"))
                TryMove(Facing.RotateRight(), false);
            else if (@event.IsActionPressed("interact"))
                Interact();
            else if (@event.IsActionPressed("open_character"))
            {
                _charSheetUI.SetParty(Party);
                _charSheetUI.Toggle();
            }
            else if (@event.IsActionPressed("open_inventory"))
            {
                _inventoryUI.SetParty(Party);
                _inventoryUI.Toggle();
            }
            else if (@event is InputEventKey rKey && rKey.Pressed && rKey.Keycode == Key.R)
            {
                Rest();
            }
        }

        private void Rest()
        {
            foreach (var m in Party.Members)
                m.Rest();
            if (_stats != null) _stats.RestsTaken++;
            SurvivalSystem.OnRest(_survival);
            ShowMessage("Die Gruppe ruht sich aus und erholt sich.");
            UpdateDisplay();
        }

        private void TryMove(Direction dir, bool updateFacing = true)
        {
            Vector2I newPos = GridPosition + dir.ToVector();

            if (!Map.IsInBounds(newPos))
            {
                ShowMessage("Eine Wand versperrt den Weg.");
                _inputCooldown = 0.3f;
                return;
            }

            var tile = Map.GetTile(newPos);
            if (!tile.IsWalkable)
            {
                ShowMessage("Eine Wand versperrt den Weg.");
                _inputCooldown = 0.3f;
                return;
            }

            // Mark previous tile as explored
            Map.GetTile(GridPosition).IsExplored = true;

            GridPosition = newPos;
            if (updateFacing)
                Facing = dir;

            // Play footstep sound
            _sound?.PlayFootstep();

            // Survival and perishable tracking
            if (_survival != null)
            {
                SurvivalSystem.OnStep(_survival);
                SurvivalSystem.ApplyStarvationDamage(Party, _survival);
                PerishableSystem.OnStep();
            }

            // Animate camera
            Vector3 camPos = _renderer.GridToWorldPos(GridPosition);
            _camera.MoveTo(camPos);
            if (updateFacing)
                _camera.RotateTo(DungeonRenderer3D.DirectionToRotation(Facing));
            _renderer.UpdatePlayerLight(camPos);

            // Handle tile effects
            switch (tile.Type)
            {
                case TileType.Trap:
                    _sound?.PlayTrap();
                    if (_traps.TryGetValue(newPos, out var trapInfo))
                    {
                        if (!trapInfo.IsDisarmed)
                        {
                            var victim = Party.Members[0];
                            string trapMsg = TrapSystem.Trigger(victim, trapInfo);
                            ShowMessage(trapMsg);
                        }
                        tile.Type = TileType.Floor;
                        _renderer.RemoveTileObject(newPos);
                        _traps.Remove(newPos);
                    }
                    else
                    {
                        ShowMessage("Eine Falle! Du verlierst Lebenspunkte.");
                        Party.Members[0].TakeDamage(5);
                        tile.Type = TileType.Floor;
                        _renderer.RemoveTileObject(newPos);
                    }
                    break;
                case TileType.Chest:
                    _sound?.PlayChestOpen();
                    OpenChest(tile, newPos);
                    break;
                case TileType.StairsDown:
                    _sound?.PlayStairs();
                    DescendLevel();
                    return;
                case TileType.StairsUp:
                    if (_currentLevel > 1)
                    {
                        AscendLevel();
                        return;
                    }
                    // Exit dungeon to town
                    _sound?.PlayStairs();
                    _townUI.ShowTown(Party);
                    ShowMessage("Du verlässt das Dungeon und kehrst zur Stadt zurück.");
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
            _camera.RotateTo(DungeonRenderer3D.DirectionToRotation(Facing));
            UpdateDisplay();
        }

        private void TurnRight()
        {
            Facing = Facing.RotateRight();
            _camera.RotateTo(DungeonRenderer3D.DirectionToRotation(Facing));
            UpdateDisplay();
        }

        private void DescendLevel()
        {
            _currentLevel++;
            if (_stats != null) _stats.LevelsDescended++;
            LoreSystem.DiscoverByLocation(_currentLevel);
            _stats?.CheckAchievements();

            // Show loading screen briefly
            if (_loadingScreen != null)
            {
                _loadingScreen.Show($"Ebene {_currentLevel}");
                _loadingScreen.SetProgress(50);
            }

            ShowMessage($"Du steigst hinab in Ebene {_currentLevel}...");
            GenerateLevel();

            if (_loadingScreen != null)
            {
                _loadingScreen.SetProgress(100);
                _loadingScreen.Close();
            }

            UpdateDisplay();
        }

        private void AscendLevel()
        {
            _currentLevel--;
            ShowMessage($"Du steigst hinauf zu Ebene {_currentLevel}...");
            GenerateLevel();
            GridPosition = _generator.GetExitPosition();
            Vector3 camPos = _renderer.GridToWorldPos(GridPosition);
            _camera.SnapToPosition(camPos, DungeonRenderer3D.DirectionToRotation(Facing));
            _renderer.UpdatePlayerLight(camPos);
            UpdateDisplay();
        }

        private void Interact()
        {
            Vector2I targetPos = GridPosition + Facing.ToVector();
            var tile = Map.GetTile(targetPos);

            if (tile == null)
            {
                ShowMessage("Hier gibt es nichts zu tun.");
                return;
            }

            switch (tile.Type)
            {
                case TileType.Door:
                    _sound?.PlayDoorOpen();
                    tile.Type = TileType.Floor;
                    _renderer.RemoveTileObject(targetPos);
                    ShowMessage("Du oeffnest die Tuer.");
                    break;
                case TileType.Chest:
                    _sound?.PlayChestOpen();
                    OpenChest(tile, targetPos);
                    break;
                case TileType.NPC:
                    if (_npcs.TryGetValue(targetPos, out var npc))
                    {
                        _dialogueUI.StartDialogue(npc, Party);
                    }
                    break;
                case TileType.Trap:
                    if (_traps.TryGetValue(targetPos, out var trap))
                    {
                        if (!trap.IsDetected)
                        {
                            bool detected = TrapSystem.TryDetect(Party.Members[0], trap);
                            if (detected)
                                ShowMessage($"{TrapSystem.GetTrapName(trap.Type)} erkannt! F zum Entschärfen (braucht Dietrich).");
                            else
                                ShowMessage("Du siehst nichts Ungewoehnliches.");
                        }
                        else if (!trap.IsDisarmed)
                        {
                            var (success, msg, lostPick) = TrapSystem.TryDisarm(Party.Members[0], trap);
                            ShowMessage(msg);
                            if (success)
                            {
                                tile.Type = TileType.Floor;
                                _renderer.RemoveTileObject(targetPos);
                                _traps.Remove(targetPos);
                            }
                        }
                        else
                        {
                            ShowMessage("Diese Falle ist bereits entschärft.");
                        }
                    }
                    else
                    {
                        ShowMessage("Eine Falle!");
                    }
                    break;
                default:
                    // Check for adjacent trap detection
                    bool foundTrap = false;
                    foreach (var kv in _traps)
                    {
                        if (kv.Key == targetPos && !kv.Value.IsDetected)
                        {
                            if (TrapSystem.TryDetect(Party.Members[0], kv.Value))
                            {
                                ShowMessage($"{TrapSystem.GetTrapName(kv.Value.Type)} erkannt!");
                                foundTrap = true;
                            }
                        }
                    }
                    if (!foundTrap)
                        ShowMessage("Hier gibt es nichts zu tun.");
                    break;
            }

            UpdateDisplay();
        }

        private void OpenChest(Tile tile, Vector2I pos)
        {
            int roll = GD.RandRange(0, 100);
            string lootMsg;

            if (roll < 40)
            {
                int gold = GD.RandRange(10, 50) + _currentLevel * 5;
                Party.Gold += gold;
                lootMsg = $"Du oeffnest die Truhe: {gold} Gold!";
            }
            else if (roll < 65)
            {
                Party.Members[0].Inventory.Add(Item.HealthPotion);
                lootMsg = "Du oeffnest die Truhe: Ein Heiltrank!";
            }
            else if (roll < 80)
            {
                Party.Members[0].Inventory.Add(Item.ManaPotion);
                lootMsg = "Du oeffnest die Truhe: Ein Manatrank!";
            }
            else if (roll < 90)
            {
                Party.Members[0].Inventory.Add(Item.Torch);
                lootMsg = "Du oeffnest die Truhe: Eine Fackel.";
            }
            else if (roll < 96)
            {
                Party.Members[0].Inventory.Add(Item.Lockpick);
                lootMsg = "Du oeffnest die Truhe: Ein Dietrich.";
            }
            else
            {
                int gold = GD.RandRange(50, 100) + _currentLevel * 10;
                Party.Gold += gold;
                Party.Members[0].Inventory.Add(Item.HealthPotion);
                Party.Members[0].Inventory.Add(Item.ManaPotion);
                lootMsg = $"Du oeffnest die Truhe: {gold} Gold und zwei Traenke!";
            }

            // Add crafting ingredients occasionally
            if (GD.Randf() < 0.3f)
            {
                var ingredients = new[] { Item.Heilwurzel, Item.Manakristall, Item.Mohn, Item.Eisenbarren, Item.Giftbeutel };
                var ingredient = ingredients[GD.RandRange(0, ingredients.Length - 1)];
                Party.Members[0].Inventory.Add(ingredient);
                lootMsg += $" Zusätzlich: {ingredient.Name}!";
            }

            // Add food/water occasionally
            if (GD.Randf() < 0.2f)
            {
                Party.Members[0].Inventory.Add(Item.Proviant);
                PerishableSystem.TrackItem(Item.Proviant);
                lootMsg += " + Proviant!";
            }

            ShowMessage(lootMsg);
            if (_stats != null) _stats.ChestsFound++;
            tile.Type = TileType.Floor;
            _renderer.RemoveTileObject(pos);
        }

        private void StartCombatScene()
        {
            if (_combatScene == null)
            {
                _combatScene = new CombatScene3D { Name = "CombatScene3D" };
                AddChild(_combatScene);
            }

            _combatScene.Visible = true;
            _combatSceneActive = true;

            // Hide dungeon renderer during combat
            if (_renderer != null)
                _renderer.Visible = false;

            // Setup and render the combat grid
            _combatScene.SetupGrid(_combat.Grid);
            _combatScene.RenderUnits(_combat.Grid);

            // Focus camera on active unit
            if (_combat.ActiveUnit != null)
                _combatScene.FocusCameraOn(_combat.ActiveUnit.GridPosition);

            // Make combat camera current
            _combatScene.CombatCamera.Current = true;
        }

        private void EndCombatScene()
        {
            _combatSceneActive = false;
            if (_combatScene != null)
                _combatScene.Visible = false;

            // Show dungeon renderer again
            if (_renderer != null)
                _renderer.Visible = true;

            // Restore first-person camera
            if (_camera != null)
                _camera.Current = true;
        }

        private void UpdateCombatScene()
        {
            if (_combatScene == null || !_combatSceneActive) return;

            _combatScene.RenderUnits(_combat.Grid);

            if (_combat.ActiveUnit != null)
            {
                _combatScene.SelectUnit(_combat.ActiveUnit.GridPosition);
                _combatScene.FocusCameraOn(_combat.ActiveUnit.GridPosition);
            }
        }

        private void StartRandomEncounter()
        {
            var encounter = new Encounter();

            // Boss encounter on every 3rd level
            if (_currentLevel % 3 == 0 && GD.Randf() < 0.5f)
            {
                var bossTemplate = Enemy.Bosses[GD.RandRange(0, Enemy.Bosses.Count - 1)];
                var boss = Enemy.Clone(bossTemplate);
                ScaleEnemyToLevel(boss, _currentLevel);
                encounter.AddEnemy(boss);
                ShowMessage("Ein mächtiger Gegner erscheint!");
                CombatUI.Sound?.PlayBossRoar();
            }
            else
            {
                int enemyCount = GD.RandRange(1, 3);
                // Scale enemy selection with dungeon level
                int maxIdx = Mathf.Min(Enemy.Bestiary.Count - 1, 1 + _currentLevel / 2);

                for (int i = 0; i < enemyCount; i++)
                {
                    var template = Enemy.Bestiary[GD.RandRange(0, maxIdx)];
                    var enemy = Enemy.Clone(template);
                    ScaleEnemyToLevel(enemy, _currentLevel);
                    encounter.AddEnemy(enemy);
                }

                ShowMessage($"Kampf! {enemyCount} Gegner erscheinen!");
            }

            _combat = new CombatManager(Party, encounter);
            _inCombat = true;
            _enemyTurnDelay = 0.8f;
            _combat.StartCombat();

            // Switch to isometric combat scene
            StartCombatScene();

            _combatUI.StartCombat(_combat);
        }

        public void OnCombatActionExecuted()
        {
            if (_combat == null) return;
            _enemyTurnDelay = 0.8f;
            if (_combatUI != null)
                _combatUI.StartCombat(_combat); // refresh display
            UpdateCombatScene();
            CheckCombatEnd();
        }

        private void CheckCombatEnd()
        {
            if (_combat == null) return;
            if (_combat.Phase == CombatPhase.Victory || _combat.Phase == CombatPhase.Defeat)
            {
                _inCombat = false;
                _combatUI.EndCombat();
                EndCombatScene();

                if (_combat.Phase == CombatPhase.Victory)
                {
                    ShowMessage("Kampf gewonnen!");
                    if (_stats != null)
                    {
                        int goldEarned = 0;
                        foreach (var e in _combat.Encounter.Enemies)
                        {
                            _stats.EnemiesKilled++;
                            if (e.IsBoss)
                                _stats.BossesKilled++;
                            goldEarned += e.GoldReward;
                        }
                        _stats.GoldEarned += goldEarned;
                        _stats.CheckAchievements();
                    }
                    _sound?.PlayLevelUp();

                    // Show newly unlocked achievements
                    if (_stats != null)
                    {
                        var newAch = _stats.GetRecentlyUnlocked();
                        foreach (var a in newAch)
                            ShowMessage($"Erfolg freigeschaltet: {a.Title}!");
                        _stats.MarkAllSeen();
                    }
                }
                else
                    ShowMessage("Die Gruppe wurde besiegt...");

                _combat = null;
                UpdateDisplay();
            }
        }

        private void ShowMessage(string msg)
        {
            _messageLabel.Text = msg;
            _messageTimer = 4.0f;
            GD.Print($"[Dungeon] {msg}");
        }

        private void UpdateDisplay()
        {
            _positionLabel.Text = $"Ebene {_currentLevel}  Pos: ({GridPosition.X}, {GridPosition.Y})  Blick: {Facing.ToGerman()}  Gold: {Party.Gold}  Zeit: {(_stats != null ? _stats.PlayTimeStr : "00:00:00")}";

            // Party status
            var sb = new System.Text.StringBuilder();
            foreach (var m in Party.Members)
            {
                sb.Append($"{m.Name} [{m.CombatStats.CurrentHealth}/{m.CombatStats.MaxHealth}]  ");
            }

            // Show survival status
            if (_survival != null)
            {
                var hungerWarn = SurvivalSystem.GetHungerWarning(_survival);
                var thirstWarn = SurvivalSystem.GetThirstWarning(_survival);
                if (hungerWarn != null || thirstWarn != null)
                {
                    sb.AppendLine();
                    if (hungerWarn != null) sb.Append(hungerWarn + "  ");
                    if (thirstWarn != null) sb.Append(thirstWarn);
                }
            }

            // Show active quests
            var activeQuests = QuestRegistry.ActiveQuests;
            if (activeQuests.Count > 0)
            {
                sb.AppendLine();
                sb.Append("Quests: ");
                foreach (var q in activeQuests)
                {
                    if (q.TargetCount > 0)
                        sb.Append($"{q.Title} ({q.CurrentCount}/{q.TargetCount})  ");
                    else
                        sb.Append($"{q.Title}  ");
                }
            }

            _partyLabel.Text = sb.ToString().TrimEnd();
        }

        private void SaveGame()
        {
            SaveSystem.SaveGame(Party, _currentLevel, GridPosition, (int)Facing, Party.Gold);
            ShowMessage("Spielstand gespeichert.");
        }

        private void LoadGame()
        {
            var (party, level, pos, facing, gold) = SaveSystem.LoadGame();
            if (party == null)
            {
                ShowMessage("Laden fehlgeschlagen.");
                return;
            }

            Party = party;
            _currentLevel = level;

            if (_mainMenuUI != null)
            {
                _mainMenuUI.QueueFree();
                _mainMenuUI = null;
            }
            if (_charCreationUI != null)
            {
                _charCreationUI.QueueFree();
                _charCreationUI = null;
            }

            _creationDone = true;
            _gameStarted = true;
            GenerateLevel();
            GridPosition = pos;
            Facing = (Direction)facing;

            Vector3 camPos = _renderer.GridToWorldPos(GridPosition);
            _camera.SnapToPosition(camPos, DungeonRenderer3D.DirectionToRotation(Facing));
            _renderer.UpdatePlayerLight(camPos);

            ShowMessage("Spielstand geladen.");
            UpdateDisplay();
        }

        private static void ScaleEnemyToLevel(Enemy enemy, int dungeonLevel)
        {
            if (dungeonLevel <= 1) return;
            int scale = dungeonLevel - enemy.Level;
            if (scale <= 0) return;

            // Scale HP by 20% per level above enemy's base level
            int hpBonus = enemy.MaxHealth * scale / 5;
            enemy.MaxHealth += hpBonus;
            enemy.Health = enemy.MaxHealth;

            // Scale attack and damage by 10% per level
            enemy.Attack += scale;
            enemy.Damage += scale / 2;

            // Scale defense slightly
            enemy.Defense += scale / 3;

            // Scale rewards
            enemy.ExperienceReward += enemy.ExperienceReward * scale / 4;
            enemy.GoldReward += enemy.GoldReward * scale / 4;
        }
    }
}
