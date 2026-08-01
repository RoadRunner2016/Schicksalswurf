using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Dungeon
{
    /// <summary>
    /// Generates and manages 3D meshes for the dungeon.
    /// Walls, floor, and ceiling are built from the DungeonMap data.
    /// Tiles are 3x3 units in size. Walls are 3 units tall.
    /// </summary>
    public partial class DungeonRenderer3D : Node3D
    {
        public DungeonMap Map { get; set; }
        public DungeonTheme Theme { get; set; }

        private const float TileSize = 3.0f;
        private const float WallHeight = 3.0f;
        private const float WallThickness = 0.2f;

        private StandardMaterial3D _wallMaterial;
        private StandardMaterial3D _floorMaterial;
        private StandardMaterial3D _ceilingMaterial;
        private StandardMaterial3D _doorMaterial;
        private StandardMaterial3D _chestMaterial;
        private StandardMaterial3D _stairsMaterial;
        private StandardMaterial3D _npcMaterial;
        private StandardMaterial3D _trapMaterial;

        // Container nodes
        private Node3D _staticGeometry;
        private Node3D _dynamicObjects;
        private OmniLight3D _playerLight;

        // Store dynamic object references for interaction
        private Dictionary<Vector2I, Node3D> _tileObjects = new();

        public override void _Ready()
        {
            // Generate procedural textures
            var wallTex = TextureGenerator.CreateStoneWallTexture();
            var floorTex = TextureGenerator.CreateStoneFloorTexture();
            var ceilingTex = TextureGenerator.CreateCeilingTexture();
            var doorTex = TextureGenerator.CreateDoorTexture();
            var chestTex = TextureGenerator.CreateChestTexture();
            var stairsTex = TextureGenerator.CreateStairsTexture();

            // Create materials with textures
            _wallMaterial = new StandardMaterial3D
            {
                AlbedoTexture = wallTex,
                Roughness = 0.9f,
                Metallic = 0.0f,
                Uv1Scale = new Vector3(1, 1, 1)
            };

            _floorMaterial = new StandardMaterial3D
            {
                AlbedoTexture = floorTex,
                Roughness = 1.0f
            };

            _ceilingMaterial = new StandardMaterial3D
            {
                AlbedoTexture = ceilingTex,
                Roughness = 1.0f
            };

            _doorMaterial = new StandardMaterial3D
            {
                AlbedoTexture = doorTex,
                Roughness = 0.8f
            };

            _chestMaterial = new StandardMaterial3D
            {
                AlbedoTexture = chestTex,
                Roughness = 0.7f
            };

            _stairsMaterial = new StandardMaterial3D
            {
                AlbedoTexture = stairsTex,
                Roughness = 0.9f
            };

            _npcMaterial = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.6f, 0.5f, 0.8f),
                Emission = new Color(0.4f, 0.3f, 0.6f),
                EmissionEnergyMultiplier = 0.5f,
                Roughness = 0.7f
            };

            _trapMaterial = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.6f, 0.1f, 0.1f),
                EmissionEnergyMultiplier = 0.5f,
                Emission = new Color(0.8f, 0.2f, 0.2f)
            };

            _staticGeometry = new Node3D { Name = "StaticGeometry" };
            AddChild(_staticGeometry);

            _dynamicObjects = new Node3D { Name = "DynamicObjects" };
            AddChild(_dynamicObjects);

            // Player torch light
            _playerLight = new OmniLight3D
            {
                LightColor = new Color(1.0f, 0.85f, 0.6f),
                LightEnergy = 2.5f,
                OmniRange = 12.0f,
                OmniAttenuation = 1.5f,
                ShadowEnabled = true
            };
            AddChild(_playerLight);
        }

        /// <summary>
        /// Rebuilds all dungeon geometry from the map.
        /// </summary>
        public void BuildGeometry()
        {
            if (Map == null) return;

            // Clear existing geometry
            foreach (Node child in _staticGeometry.GetChildren())
            {
                child.QueueFree();
            }
            foreach (Node child in _dynamicObjects.GetChildren())
            {
                child.QueueFree();
            }
            _tileObjects.Clear();

            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    var tile = Map.Tiles[x, y];
                    Vector3 worldPos = GridToWorld(x, y);

                    if (tile.Type == TileType.Wall)
                    {
                        BuildWall(x, y);
                    }
                    else
                    {
                        // Floor and ceiling for walkable tiles
                        BuildFloor(worldPos);
                        BuildCeiling(worldPos);

                        // Build walls around this tile where neighbors are walls
                        BuildBorderWalls(x, y);

                        // Dynamic objects
                        if (tile.Type == TileType.Door)
                            BuildDoor(worldPos);
                        else if (tile.Type == TileType.Chest)
                            BuildChest(worldPos);
                        else if (tile.Type == TileType.StairsUp)
                            BuildStairs(worldPos, true);
                        else if (tile.Type == TileType.StairsDown)
                            BuildStairs(worldPos, false);
                        else if (tile.Type == TileType.Trap)
                            BuildTrapMarker(worldPos);
                        else if (tile.Type == TileType.NPC)
                            BuildNPC(worldPos);
                    }
                }
            }

            // Apply theme colors if available
            if (Theme != null)
            {
                _wallMaterial.AlbedoColor = Theme.WallColor;
                _floorMaterial.AlbedoColor = Theme.FloorColor;
                _ceilingMaterial.AlbedoColor = Theme.CeilingColor;
            }
        }

        private Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(x * TileSize, 0, y * TileSize);
        }

        private void BuildWall(int x, int y)
        {
            Vector3 pos = GridToWorld(x, y);

            // Main wall block
            var mesh = new BoxMesh
            {
                Size = new Vector3(TileSize, WallHeight, TileSize)
            };

            var mi = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = _wallMaterial,
                Position = pos + new Vector3(0, WallHeight / 2, 0)
            };

            _staticGeometry.AddChild(mi);
        }

        private void BuildFloor(Vector3 pos)
        {
            var mesh = new PlaneMesh
            {
                Size = new Vector2(TileSize, TileSize)
            };

            var mi = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = _floorMaterial,
                Position = pos
            };

            _staticGeometry.AddChild(mi);
        }

        private void BuildCeiling(Vector3 pos)
        {
            var mesh = new PlaneMesh
            {
                Size = new Vector2(TileSize, TileSize)
            };

            var mi = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = _ceilingMaterial,
                Position = pos + new Vector3(0, WallHeight, 0),
                Rotation = new Vector3(Mathf.Pi, 0, 0)
            };

            _staticGeometry.AddChild(mi);
        }

        /// <summary>
        /// Builds thin wall segments between a walkable tile and adjacent wall tiles.
        /// </summary>
        private void BuildBorderWalls(int x, int y)
        {
            Vector3 pos = GridToWorld(x, y);

            // Check each direction
            BuildBorderWallIfNeeded(pos, x, y, 0, -1, Direction.North); // North
            BuildBorderWallIfNeeded(pos, x, y, 1, 0, Direction.East);   // East
            BuildBorderWallIfNeeded(pos, x, y, 0, 1, Direction.South);  // South
            BuildBorderWallIfNeeded(pos, x, y, -1, 0, Direction.West);  // West
        }

        private void BuildBorderWallIfNeeded(Vector3 tilePos, int x, int y, int dx, int dy, Direction dir)
        {
            var neighbor = Map.GetTile(x + dx, y + dy);
            if (neighbor == null || neighbor.Type == TileType.Wall)
            {
                // Build a thin wall on this side
                var dirVec = dir.ToVector();

                // Place torches on some walls
                if ((x * 7 + y * 13) % 5 == 0 && (dx == 0 || dy == 0))
                {
                    BuildTorch(tilePos, dirVec);
                }
                Vector3 offset = new Vector3(dirVec.X, 0, dirVec.Y) * (TileSize / 2 - WallThickness / 2);
                bool horizontal = (dir == Direction.North || dir == Direction.South);

                var mesh = new BoxMesh
                {
                    Size = new Vector3(
                        horizontal ? TileSize : WallThickness,
                        WallHeight,
                        horizontal ? WallThickness : TileSize
                    )
                };

                var mi = new MeshInstance3D
                {
                    Mesh = mesh,
                    MaterialOverride = _wallMaterial,
                    Position = tilePos + offset + new Vector3(0, WallHeight / 2, 0)
                };

                _staticGeometry.AddChild(mi);
            }
        }

        private void BuildTorch(Vector3 tilePos, Vector2I dirVec)
        {
            Vector3 offset = new Vector3(dirVec.X, 0, dirVec.Y) * (TileSize / 2 - 0.2f);
            Vector3 torchPos = tilePos + offset + new Vector3(0, WallHeight * 0.6f, 0);

            // Torch bracket
            var bracketMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.3f, 0.2f, 0.1f),
                Roughness = 0.9f
            };
            var bracketMesh = new BoxMesh { Size = new Vector3(0.15f, 0.3f, 0.1f) };
            var bracketMi = new MeshInstance3D
            {
                Mesh = bracketMesh,
                MaterialOverride = bracketMat,
                Position = torchPos
            };
            _staticGeometry.AddChild(bracketMi);

            // Flame (small sphere)
            var flameMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(1.0f, 0.6f, 0.1f),
                Emission = new Color(1.0f, 0.5f, 0.1f),
                EmissionEnergyMultiplier = 2.0f
            };
            var flameMesh = new SphereMesh { Radius = 0.12f, Height = 0.24f };
            var flameMi = new MeshInstance3D
            {
                Mesh = flameMesh,
                MaterialOverride = flameMat,
                Position = torchPos + new Vector3(0, 0.2f, 0)
            };
            _staticGeometry.AddChild(flameMi);

            // Torch light
            var light = new OmniLight3D
            {
                Position = torchPos + new Vector3(0, 0.2f, 0),
                LightColor = new Color(1.0f, 0.7f, 0.3f),
                LightEnergy = 1.5f,
                OmniRange = 6.0f,
                OmniAttenuation = 1.5f,
                ShadowEnabled = false
            };
            _staticGeometry.AddChild(light);
        }

        private void BuildDoor(Vector3 pos)
        {
            // Door frame - two side posts and a top
            var doorMat = _doorMaterial;

            // Door as a box
            var mesh = new BoxMesh
            {
                Size = new Vector3(TileSize * 0.8f, WallHeight * 0.85f, WallThickness)
            };

            var mi = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = doorMat,
                Position = pos + new Vector3(0, WallHeight * 0.425f, 0)
            };

            _dynamicObjects.AddChild(mi);
        }

        private void BuildChest(Vector3 pos)
        {
            var mesh = new BoxMesh
            {
                Size = new Vector3(1.0f, 0.6f, 0.8f)
            };

            var mi = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = _chestMaterial,
                Position = pos + new Vector3(0, 0.3f, 0)
            };

            _dynamicObjects.AddChild(mi);

            // Gold accent on top
            var goldMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(1.0f, 0.8f, 0.2f),
                Metallic = 0.8f,
                Roughness = 0.3f
            };

            var lockMesh = new BoxMesh { Size = new Vector3(0.2f, 0.15f, 0.1f) };
            var lockMi = new MeshInstance3D
            {
                Mesh = lockMesh,
                MaterialOverride = goldMat,
                Position = pos + new Vector3(0, 0.65f, 0.35f)
            };
            _dynamicObjects.AddChild(lockMi);
        }

        private void BuildStairs(Vector3 pos, bool up)
        {
            int numSteps = 5;
            float stepHeight = WallHeight / numSteps;
            float stepDepth = TileSize / numSteps;

            for (int i = 0; i < numSteps; i++)
            {
                float y = up ? i * stepHeight : (numSteps - 1 - i) * stepHeight;
                float z = -TileSize / 2 + i * stepDepth + stepDepth / 2;

                var mesh = new BoxMesh
                {
                    Size = new Vector3(TileSize * 0.8f, stepHeight, stepDepth)
                };

                var mi = new MeshInstance3D
                {
                    Mesh = mesh,
                    MaterialOverride = _stairsMaterial,
                    Position = pos + new Vector3(0, y + stepHeight / 2, z)
                };

                _dynamicObjects.AddChild(mi);
            }
        }

        private void BuildTrapMarker(Vector3 pos)
        {
            var mesh = new PlaneMesh { Size = new Vector2(TileSize * 0.7f, TileSize * 0.7f) };
            var mi = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = _trapMaterial,
                Position = pos + new Vector3(0, 0.02f, 0)
            };

            _dynamicObjects.AddChild(mi);
        }

        private void BuildNPC(Vector3 pos)
        {
            // NPC body (cylinder)
            var bodyMesh = new CylinderMesh { TopRadius = 0.3f, BottomRadius = 0.5f, Height = 1.6f };
            var bodyMi = new MeshInstance3D
            {
                Mesh = bodyMesh,
                MaterialOverride = _npcMaterial,
                Position = pos + new Vector3(0, 0.8f, 0)
            };
            _dynamicObjects.AddChild(bodyMi);

            // NPC head (sphere)
            var headMesh = new SphereMesh { Radius = 0.25f, Height = 0.5f };
            var headMi = new MeshInstance3D
            {
                Mesh = headMesh,
                MaterialOverride = _npcMaterial,
                Position = pos + new Vector3(0, 1.8f, 0)
            };
            _dynamicObjects.AddChild(headMi);

            // Indicator light
            var light = new OmniLight3D
            {
                Position = pos + new Vector3(0, 2.0f, 0),
                LightColor = new Color(0.6f, 0.5f, 0.8f),
                LightEnergy = 1.0f,
                OmniRange = 4.0f,
                OmniAttenuation = 1.0f
            };
            _dynamicObjects.AddChild(light);
        }

        /// <summary>
        /// Updates the player light position to follow the camera.
        /// </summary>
        public void UpdatePlayerLight(Vector3 worldPos)
        {
            _playerLight.Position = worldPos + new Vector3(0, 1.5f, 0);
        }

        /// <summary>
        /// Removes a dynamic object for a specific tile (e.g. opened chest, triggered trap).
        /// </summary>
        public void RemoveTileObject(Vector2I gridPos)
        {
            if (_tileObjects.TryGetValue(gridPos, out var obj))
            {
                obj.QueueFree();
                _tileObjects.Remove(gridPos);
            }
        }

        /// <summary>
        /// Converts a grid position to a world position for the camera.
        /// </summary>
        public Vector3 GridToWorldPos(Vector2I grid)
        {
            return new Vector3(grid.X * TileSize, WallHeight * 0.5f, grid.Y * TileSize);
        }

        /// <summary>
        /// Converts a facing direction to a Y-axis rotation in radians.
        /// </summary>
        public static float DirectionToRotation(Direction dir)
        {
            return dir switch
            {
                Direction.North => 0.0f,
                Direction.East => -Mathf.Pi / 2,
                Direction.South => Mathf.Pi,
                Direction.West => Mathf.Pi / 2,
                _ => 0.0f
            };
        }
    }
}
