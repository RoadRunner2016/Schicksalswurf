using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Combat
{
    using Characters;
    using Core;

    /// <summary>
    /// 3D isometric combat scene renderer.
    /// Renders a tactical grid with party members and enemies as 3D figures.
    /// </summary>
    public partial class CombatScene3D : Node3D
    {
        private CombatGrid _grid;
        private Dictionary<Vector2I, Node3D> _unitNodes = new();
        private Dictionary<Vector2I, MeshInstance3D> _tileNodes = new();

        private Node3D _gridRoot;
        private Node3D _unitsRoot;
        private OmniLight3D _combatLight;

        private const float TileSize = 2.0f;
        private const float TileHeight = 0.1f;

        // Isometric camera
        public Camera3D CombatCamera { get; private set; }

        private static readonly Color PartyColor = new(0.2f, 0.5f, 0.9f);
        private static readonly Color EnemyColor = new(0.9f, 0.2f, 0.15f);
        private static readonly Color TileColor = new(0.15f, 0.14f, 0.12f);
        private static readonly Color TileBorderColor = new(0.3f, 0.28f, 0.25f);
        private static readonly Color HighlightMoveColor = new(0.15f, 0.5f, 0.2f, 0.5f);
        private static readonly Color HighlightAttackColor = new(0.7f, 0.15f, 0.1f, 0.5f);
        private static readonly Color BossColor = new(0.8f, 0.6f, 0.1f);

        public override void _Ready()
        {
            _gridRoot = new Node3D { Name = "GridRoot" };
            AddChild(_gridRoot);

            _unitsRoot = new Node3D { Name = "UnitsRoot" };
            AddChild(_unitsRoot);

            // Isometric camera
            CombatCamera = new Camera3D
            {
                Fov = 50.0f,
                Near = 0.1f,
                Far = 200.0f,
                Position = new Vector3(18, 22, 18),
                Rotation = new Vector3(Mathf.DegToRad(-55), Mathf.DegToRad(45), 0)
            };
            AddChild(CombatCamera);

            // Combat lighting
            _combatLight = new OmniLight3D
            {
                Position = new Vector3(12, 10, 10),
                LightColor = new Color(0.9f, 0.85f, 0.7f),
                LightEnergy = 2.0f,
                OmniRange = 30.0f,
                OmniAttenuation = 1.2f,
                ShadowEnabled = true
            };
            AddChild(_combatLight);

            // Ambient fill light
            var fillLight = new DirectionalLight3D
            {
                Position = new Vector3(0, 20, 0),
                Rotation = new Vector3(Mathf.DegToRad(-60), 0, 0),
                LightColor = new Color(0.3f, 0.3f, 0.4f),
                LightEnergy = 0.5f,
                ShadowEnabled = false
            };
            AddChild(fillLight);

            // World environment
            var env = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = new Color(0.03f, 0.03f, 0.05f),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color(0.2f, 0.18f, 0.25f),
                AmbientLightEnergy = 0.5f,
                FogEnabled = true,
                FogLightColor = new Color(0.05f, 0.04f, 0.08f),
                FogDensity = 0.02f
            };
            AddChild(new WorldEnvironment { Environment = env });
        }

        public void SetupGrid(CombatGrid grid)
        {
            _grid = grid;
            ClearGrid();

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var pos = new Vector2I(x, y);
                    var worldPos = GridToWorld(pos);
                    var isBorder = x == 0 || y == 0 || x == grid.Width - 1 || y == grid.Height - 1;

                    var tileMat = new StandardMaterial3D
                    {
                        AlbedoColor = isBorder ? TileBorderColor : TileColor,
                        Roughness = 0.9f
                    };

                    var mesh = new BoxMesh { Size = new Vector3(TileSize * 0.95f, TileHeight, TileSize * 0.95f) };
                    var mi = new MeshInstance3D
                    {
                        Mesh = mesh,
                        MaterialOverride = tileMat,
                        Position = worldPos
                    };
                    _gridRoot.AddChild(mi);
                    _tileNodes[pos] = mi;
                }
            }
        }

        public void RenderUnits(CombatGrid grid)
        {
            // Clear existing units
            foreach (var node in _unitNodes.Values)
                node.QueueFree();
            _unitNodes.Clear();

            foreach (var kv in grid.Units)
            {
                var unit = kv.Value;
                if (!unit.IsAlive) continue;

                var node = CreateUnitNode(unit);
                node.Position = GridToWorld(kv.Key);
                _unitsRoot.AddChild(node);
                _unitNodes[kv.Key] = node;
            }
        }

        private Node3D CreateUnitNode(CombatUnit unit)
        {
            var root = new Node3D();

            var color = unit.IsPlayer ? PartyColor : (unit.Enemy?.IsBoss == true ? BossColor : EnemyColor);
            var skinColor = new Color(color.R * 1.3f, color.G * 1.3f, color.B * 1.3f);
            bool isBoss = unit.Enemy?.IsBoss == true;
            float scale = isBoss ? 1.4f : 1.0f;

            var bodyMat = new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 0.6f
            };
            var skinMat = new StandardMaterial3D
            {
                AlbedoColor = skinColor,
                Roughness = 0.5f
            };
            var darkMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(color.R * 0.6f, color.G * 0.6f, color.B * 0.6f),
                Roughness = 0.7f
            };

            // Torso (capsule-like: tapered cylinder)
            var bodyMesh = new CylinderMesh { TopRadius = 0.35f * scale, BottomRadius = 0.45f * scale, Height = 0.8f * scale };
            var bodyMi = new MeshInstance3D
            {
                Mesh = bodyMesh,
                MaterialOverride = bodyMat,
                Position = new Vector3(0, 0.8f * scale, 0)
            };
            root.AddChild(bodyMi);

            // Lower body / legs (box)
            var legMesh = new BoxMesh { Size = new Vector3(0.5f * scale, 0.5f * scale, 0.4f * scale) };
            var legMi = new MeshInstance3D
            {
                Mesh = legMesh,
                MaterialOverride = darkMat,
                Position = new Vector3(0, 0.25f * scale, 0)
            };
            root.AddChild(legMi);

            // Head (sphere)
            var headMesh = new SphereMesh { Radius = 0.25f * scale, Height = 0.5f * scale };
            var headMi = new MeshInstance3D
            {
                Mesh = headMesh,
                MaterialOverride = skinMat,
                Position = new Vector3(0, 1.45f * scale, 0)
            };
            root.AddChild(headMi);

            // Left arm (cylinder)
            var leftArmMesh = new CapsuleMesh { Radius = 0.12f * scale, Height = 0.6f * scale };
            var leftArmMi = new MeshInstance3D
            {
                Mesh = leftArmMesh,
                MaterialOverride = skinMat,
                Position = new Vector3(-0.45f * scale, 0.85f * scale, 0),
                Rotation = new Vector3(0, 0, Mathf.DegToRad(15))
            };
            root.AddChild(leftArmMi);

            // Right arm (cylinder)
            var rightArmMesh = new CapsuleMesh { Radius = 0.12f * scale, Height = 0.6f * scale };
            var rightArmMi = new MeshInstance3D
            {
                Mesh = rightArmMesh,
                MaterialOverride = skinMat,
                Position = new Vector3(0.45f * scale, 0.85f * scale, 0),
                Rotation = new Vector3(0, 0, Mathf.DegToRad(-15))
            };
            root.AddChild(rightArmMi);

            // Weapon (box for sword/axe) in right hand
            var weaponMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.7f, 0.7f, 0.75f),
                Metallic = 0.8f,
                Roughness = 0.3f
            };
            var weaponMesh = new BoxMesh { Size = new Vector3(0.08f * scale, 0.7f * scale, 0.02f * scale) };
            var weaponMi = new MeshInstance3D
            {
                Mesh = weaponMesh,
                MaterialOverride = weaponMat,
                Position = new Vector3(0.55f * scale, 1.1f * scale, 0.1f * scale),
                Rotation = new Vector3(Mathf.DegToRad(-20), 0, Mathf.DegToRad(-10)),
                Name = "Weapon"
            };
            root.AddChild(weaponMi);

            // Shield on left arm for players
            if (unit.IsPlayer)
            {
                var shieldMat = new StandardMaterial3D
                {
                    AlbedoColor = new Color(color.R * 0.8f, color.G * 0.8f, color.B * 1.2f),
                    Metallic = 0.3f,
                    Roughness = 0.5f
                };
                var shieldMesh = new BoxMesh { Size = new Vector3(0.05f, 0.5f, 0.35f) };
                var shieldMi = new MeshInstance3D
                {
                    Mesh = shieldMesh,
                    MaterialOverride = shieldMat,
                    Position = new Vector3(-0.55f, 0.85f, 0.05f),
                    Rotation = new Vector3(0, Mathf.DegToRad(10), 0),
                    Name = "Shield"
                };
                root.AddChild(shieldMi);
            }

            // Boss: add horns and cape
            if (isBoss)
            {
                var hornMat = new StandardMaterial3D
                {
                    AlbedoColor = new Color(0.1f, 0.05f, 0.03f),
                    Roughness = 0.4f
                };
                // Left horn (cone via cylinder with top radius 0)
                var leftHornMesh = new CylinderMesh { TopRadius = 0.0f, BottomRadius = 0.08f, Height = 0.25f };
                var leftHornMi = new MeshInstance3D
                {
                    Mesh = leftHornMesh,
                    MaterialOverride = hornMat,
                    Position = new Vector3(-0.15f, 1.7f, 0),
                    Rotation = new Vector3(0, 0, Mathf.DegToRad(-30))
                };
                root.AddChild(leftHornMi);
                // Right horn (cone via cylinder with top radius 0)
                var rightHornMesh = new CylinderMesh { TopRadius = 0.0f, BottomRadius = 0.08f, Height = 0.25f };
                var rightHornMi = new MeshInstance3D
                {
                    Mesh = rightHornMesh,
                    MaterialOverride = hornMat,
                    Position = new Vector3(0.15f, 1.7f, 0),
                    Rotation = new Vector3(0, 0, Mathf.DegToRad(30))
                };
                root.AddChild(rightHornMi);

                // Cape (box behind body)
                var capeMat = new StandardMaterial3D
                {
                    AlbedoColor = new Color(0.3f, 0.05f, 0.05f),
                    Roughness = 0.8f
                };
                var capeMesh = new BoxMesh { Size = new Vector3(0.6f, 1.0f, 0.05f) };
                var capeMi = new MeshInstance3D
                {
                    Mesh = capeMesh,
                    MaterialOverride = capeMat,
                    Position = new Vector3(0, 0.8f, -0.25f)
                };
                root.AddChild(capeMi);
            }

            // Health bar above unit (using 3D meshes)
            float healthPct = (float)unit.CurrentHealth / unit.MaxHealth;
            var hpBgMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                CullMode = BaseMaterial3D.CullModeEnum.Disabled
            };
            var hpBgMesh = new BoxMesh { Size = new Vector3(1.2f, 0.12f, 0.02f) };
            var hpBgMi = new MeshInstance3D
            {
                Mesh = hpBgMesh,
                MaterialOverride = hpBgMat,
                Position = new Vector3(0, 2.0f * scale, 0),
                Name = "HealthBarBg"
            };
            root.AddChild(hpBgMi);

            var hpFillMat = new StandardMaterial3D
            {
                AlbedoColor = unit.IsPlayer ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.1f),
                Emission = unit.IsPlayer ? new Color(0.1f, 0.4f, 0.1f) : new Color(0.4f, 0.1f, 0.05f),
                EmissionEnergyMultiplier = 0.5f,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled
            };
            var hpFillMesh = new BoxMesh { Size = new Vector3(Mathf.Max(0.01f, 1.2f * healthPct), 0.12f, 0.03f) };
            var hpFillMi = new MeshInstance3D
            {
                Mesh = hpFillMesh,
                MaterialOverride = hpFillMat,
                Position = new Vector3(-0.6f + 0.6f * healthPct, 2.0f * scale, 0.01f),
                Name = "HealthBarFill"
            };
            root.AddChild(hpFillMi);

            // Name label via Label3D
            var label = new Label3D
            {
                Text = unit.Name,
                Position = new Vector3(0, 2.4f * scale, 0),
                FontSize = 24,
                Modulate = unit.IsPlayer ? new Color(0.6f, 0.8f, 1.0f) : new Color(1.0f, 0.5f, 0.4f)
            };
            root.AddChild(label);

            // Selection indicator (ring on the ground)
            var ringMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(1.0f, 0.9f, 0.2f, 0.6f),
                Emission = new Color(1.0f, 0.9f, 0.2f),
                EmissionEnergyMultiplier = 1.0f
            };
            var ringMesh = new CylinderMesh { TopRadius = 0.7f * scale, BottomRadius = 0.7f * scale, Height = 0.05f };
            var ringMi = new MeshInstance3D
            {
                Mesh = ringMesh,
                MaterialOverride = ringMat,
                Position = new Vector3(0, 0.03f, 0),
                Name = "SelectionRing"
            };
            ringMi.Visible = false;
            root.AddChild(ringMi);

            return root;
        }

        public void HighlightTiles(List<Vector2I> tiles, bool attackMode)
        {
            ClearHighlights();
            var color = attackMode ? HighlightAttackColor : HighlightMoveColor;

            foreach (var pos in tiles)
            {
                if (_tileNodes.TryGetValue(pos, out var tile))
                {
                    var mat = new StandardMaterial3D
                    {
                        AlbedoColor = color,
                        Emission = color,
                        EmissionEnergyMultiplier = 0.5f,
                        Roughness = 0.8f
                    };
                    tile.MaterialOverride = mat;
                }
            }
        }

        public void ClearHighlights()
        {
            if (_grid == null) return;
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    var pos = new Vector2I(x, y);
                    if (_tileNodes.TryGetValue(pos, out var tile))
                    {
                        var isBorder = x == 0 || y == 0 || x == _grid.Width - 1 || y == _grid.Height - 1;
                        tile.MaterialOverride = new StandardMaterial3D
                        {
                            AlbedoColor = isBorder ? TileBorderColor : TileColor,
                            Roughness = 0.9f
                        };
                    }
                }
            }
        }

        public void SelectUnit(Vector2I pos)
        {
            // Clear previous selection
            foreach (var node in _unitNodes.Values)
            {
                var ring = node.GetNodeOrNull<MeshInstance3D>("SelectionRing");
                if (ring != null) ring.Visible = false;
            }

            if (_unitNodes.TryGetValue(pos, out var unit))
            {
                var ring = unit.GetNodeOrNull<MeshInstance3D>("SelectionRing");
                if (ring != null) ring.Visible = true;
            }
        }

        public void UpdateUnitPosition(Vector2I oldPos, Vector2I newPos)
        {
            if (_unitNodes.TryGetValue(oldPos, out var node))
            {
                _unitNodes.Remove(oldPos);
                node.Position = GridToWorld(newPos);
                _unitNodes[newPos] = node;
            }
        }

        public void RemoveUnit(Vector2I pos)
        {
            if (_unitNodes.TryGetValue(pos, out var node))
            {
                // Fade out animation
                var tween = node.CreateTween();
                tween.TweenProperty(node, "position:y", node.Position.Y - 1.5f, 0.5f)
                    .SetTrans(Tween.TransitionType.Quad);
                tween.TweenCallback(Callable.From(() => node.QueueFree()));
                _unitNodes.Remove(pos);
            }
        }

        public void UpdateHealthBars(CombatGrid grid)
        {
            foreach (var kv in grid.Units)
            {
                if (_unitNodes.TryGetValue(kv.Key, out var node))
                {
                    float pct = (float)kv.Value.CurrentHealth / kv.Value.MaxHealth;
                    bool isBoss = kv.Value.Enemy?.IsBoss == true;
                    float scale = isBoss ? 1.4f : 1.0f;

                    // Update health bar fill mesh
                    var fillMi = node.GetNodeOrNull<MeshInstance3D>("HealthBarFill");
                    if (fillMi != null && fillMi.Mesh is BoxMesh box)
                    {
                        box.Size = new Vector3(Mathf.Max(0.01f, 1.2f * pct), 0.12f, 0.03f);
                        fillMi.Position = new Vector3(-0.6f + 0.6f * pct, 2.0f * scale, 0.01f);
                    }

                    // Update label color based on health
                    var label = node.GetNodeOrNull<Label3D>("Label3D");
                    if (label != null)
                    {
                        if (pct < 0.3f)
                            label.Modulate = new Color(1.0f, 0.3f, 0.3f);
                        else if (pct < 0.6f)
                            label.Modulate = new Color(1.0f, 0.7f, 0.3f);
                    }
                }
            }
        }

        public void AnimateAttack(Vector2I attackerPos, Vector2I targetPos)
        {
            if (!_unitNodes.TryGetValue(attackerPos, out var attacker)) return;

            var originalPos = attacker.Position;
            var targetWorldPos = GridToWorld(targetPos);
            var direction = (targetWorldPos - originalPos).Normalized();
            var lungePos = originalPos + direction * 0.8f;

            var tween = attacker.CreateTween();
            tween.TweenProperty(attacker, "position", lungePos, 0.15f)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(attacker, "position", originalPos, 0.2f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
        }

        public void AnimateHit(Vector2I targetPos)
        {
            if (!_unitNodes.TryGetValue(targetPos, out var target)) return;

            var originalPos = target.Position;
            var tween = target.CreateTween();
            tween.TweenProperty(target, "position", originalPos + new Vector3(0, 0.2f, 0), 0.1f);
            tween.TweenProperty(target, "position", originalPos, 0.15f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
        }

        public void AnimateSpellCast(Vector2I casterPos)
        {
            if (!_unitNodes.TryGetValue(casterPos, out var caster)) return;

            var originalScale = caster.Scale;
            var tween = caster.CreateTween();
            tween.TweenProperty(caster, "scale", originalScale * 1.3f, 0.2f)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(caster, "scale", originalScale, 0.3f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
        }

        public void FocusCameraOn(Vector2I gridPos)
        {
            var worldPos = GridToWorld(gridPos);
            var camTarget = worldPos + new Vector3(12, 16, 12);
            var tween = CombatCamera.CreateTween();
            tween.TweenProperty(CombatCamera, "global_position", camTarget, 0.4f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }

        private void ClearGrid()
        {
            foreach (var tile in _tileNodes.Values)
                tile.QueueFree();
            _tileNodes.Clear();
            foreach (var unit in _unitNodes.Values)
                unit.QueueFree();
            _unitNodes.Clear();
        }

        public Vector3 GridToWorld(Vector2I gridPos)
        {
            return new Vector3(
                gridPos.X * TileSize,
                0,
                gridPos.Y * TileSize
            );
        }

        public Vector2I WorldToGrid(Vector3 worldPos)
        {
            return new Vector2I(
                Mathf.RoundToInt(worldPos.X / TileSize),
                Mathf.RoundToInt(worldPos.Z / TileSize)
            );
        }
    }
}
