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

            // Body (cylinder)
            var bodyMat = new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 0.6f
            };
            var bodyMesh = new CylinderMesh { TopRadius = 0.4f, BottomRadius = 0.5f, Height = 1.2f };
            var bodyMi = new MeshInstance3D
            {
                Mesh = bodyMesh,
                MaterialOverride = bodyMat,
                Position = new Vector3(0, 0.6f, 0)
            };
            root.AddChild(bodyMi);

            // Head (sphere)
            var headMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(color.R * 1.3f, color.G * 1.3f, color.B * 1.3f),
                Roughness = 0.5f
            };
            var headMesh = new SphereMesh { Radius = 0.3f, Height = 0.6f };
            var headMi = new MeshInstance3D
            {
                Mesh = headMesh,
                MaterialOverride = headMat,
                Position = new Vector3(0, 1.5f, 0)
            };
            root.AddChild(headMi);

            // Health bar above unit
            var barBg = new ColorRect
            {
                Color = new Color(0.1f, 0.1f, 0.1f, 0.8f),
                Size = new Vector2(1.2f, 0.15f),
                Position = new Vector2(-0.6f, -0.1f)
            };
            var barFill = new ColorRect
            {
                Color = unit.IsPlayer ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.1f),
                Size = new Vector2(1.2f, 0.15f),
                Position = new Vector2(-0.6f, -0.1f)
            };
            var barRoot = new Control
            {
                Size = new Vector2(1.2f, 0.15f)
            };
            barRoot.AddChild(barBg);
            barRoot.AddChild(barFill);

            // Use a Sprite3D-like approach with a SubViewport would be complex;
            // instead, scale the fill based on health
            float healthPct = (float)unit.CurrentHealth / unit.MaxHealth;
            barFill.Size = new Vector2(1.2f * healthPct, 0.15f);

            // Name label via Label3D
            var label = new Label3D
            {
                Text = unit.Name,
                Position = new Vector3(0, 2.2f, 0),
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
            var ringMesh = new CylinderMesh { TopRadius = 0.7f, BottomRadius = 0.7f, Height = 0.05f };
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
                    // Update label color based on health
                    var label = node.GetNodeOrNull<Label3D>("Label3D");
                    if (label != null)
                    {
                        float pct = (float)kv.Value.CurrentHealth / kv.Value.MaxHealth;
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
