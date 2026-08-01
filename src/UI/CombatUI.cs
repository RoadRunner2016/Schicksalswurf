using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.UI
{
    using Core;
    using Characters;
    using Combat;

    /// <summary>
    /// Interactive combat UI overlay. Shows party status, enemy list,
    /// action buttons, and a combat log. Player selects actions per turn.
    /// </summary>
    public partial class CombatUI : Control
    {
        private CombatManager _combat;
        private bool _isActive = false;
        public static SoundSystem Sound { get; set; }
        public static ParticleEffects Particles { get; set; }

        // Layout
        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;

        // Top section: enemies
        private Label _enemiesTitle;
        private VBoxContainer _enemyList;

        // Middle: combat log
        private RichTextLabel _combatLog;

        // Bottom: party + actions
        private HBoxContainer _partyRow;
        private HBoxContainer _actionButtons;

        // State
        private int _selectedEnemyIndex = 0;
        private List<Enemy> _currentEnemies = new();
        private bool _spellSelectMode = false;
        private List<Spell> _availableSpells = new();
        private int _selectedSpellIndex = 0;
        private VBoxContainer _spellSelectList;

        // Style
        private static readonly Color PanelBg = new(0.08f, 0.08f, 0.12f, 0.92f);
        private static readonly Color BorderColor = new(0.5f, 0.4f, 0.2f, 0.8f);
        private static readonly Color TextColor = new(0.9f, 0.85f, 0.7f);
        private static readonly Color HealthColor = new(0.8f, 0.2f, 0.2f);
        private static readonly Color EnemyColor = new(0.9f, 0.3f, 0.3f);
        private static readonly Color SelectedColor = new(1.0f, 0.85f, 0.2f);

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            Visible = false;

            BuildUI();
        }

        private void BuildUI()
        {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
            AddChild(_mainPanel);

            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 8);
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainPanel.AddChild(_mainContainer);

            // Title
            var titleLabel = new Label { Text = "⚔ KAMPF ⚔" };
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleLabel.AddThemeColorOverride("font_color", SelectedColor);
            _mainContainer.AddChild(titleLabel);

            // Enemies section
            _enemiesTitle = new Label { Text = "Gegner:" };
            _enemiesTitle.AddThemeFontSizeOverride("font_size", 16);
            _enemiesTitle.AddThemeColorOverride("font_color", EnemyColor);
            _mainContainer.AddChild(_enemiesTitle);

            _enemyList = new VBoxContainer();
            _enemyList.AddThemeConstantOverride("separation", 4);
            _mainContainer.AddChild(_enemyList);

            // Combat log
            var logTitle = new Label { Text = "Kampflog:" };
            logTitle.AddThemeFontSizeOverride("font_size", 14);
            logTitle.AddThemeColorOverride("font_color", TextColor);
            _mainContainer.AddChild(logTitle);

            _combatLog = new RichTextLabel();
            _combatLog.CustomMinimumSize = new Vector2(0, 120);
            _combatLog.BbcodeEnabled = true;
            _combatLog.AddThemeColorOverride("default_color", TextColor);
            _combatLog.AddThemeFontSizeOverride("normal_font_size", 13);
            _combatLog.ScrollFollowing = true;
            _mainContainer.AddChild(_combatLog);

            // Party row
            _partyRow = new HBoxContainer();
            _partyRow.AddThemeConstantOverride("separation", 12);
            _mainContainer.AddChild(_partyRow);

            // Action buttons
            _actionButtons = new HBoxContainer();
            _actionButtons.AddThemeConstantOverride("separation", 8);
            _actionButtons.Alignment = BoxContainer.AlignmentMode.Center;
            _mainContainer.AddChild(_actionButtons);

            // Spell selection list (hidden by default)
            _spellSelectList = new VBoxContainer();
            _spellSelectList.AddThemeConstantOverride("separation", 3);
            _spellSelectList.Visible = false;
            _mainContainer.AddChild(_spellSelectList);
        }

        private StyleBoxFlat CreatePanelStyle()
        {
            return new StyleBoxFlat
            {
                BgColor = PanelBg,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderColor = BorderColor,
                ContentMarginLeft = 16,
                ContentMarginRight = 16,
                ContentMarginTop = 12,
                ContentMarginBottom = 12
            };
        }

        public void StartCombat(CombatManager combat)
        {
            _combat = combat;
            _isActive = true;
            Visible = true;
            _combatLog.Clear();
            _selectedEnemyIndex = 0;
            RefreshUI();
        }

        public void EndCombat()
        {
            _isActive = false;
            Visible = false;
            _combat = null;
        }

        public bool IsActive => _isActive;

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!_isActive) return;

            if (_combat.Phase == CombatPhase.PlayerTurn)
            {
                var current = _combat.CurrentParticipant;
                if (current?.IsPlayer != true) return;

                // In spell selection mode: number keys select spell, F confirms, Esc cancels
                if (_spellSelectMode)
                {
                    if (@event is InputEventKey key && key.Pressed)
                    {
                        int num = (int)key.Keycode - 49; // '1' = 0
                        if (num >= 0 && num < _availableSpells.Count)
                        {
                            _selectedSpellIndex = num;
                            RefreshSpellSelect();
                        }
                        else if (key.Keycode == Key.F || key.Keycode == Key.Enter)
                        {
                            ConfirmSpellCast();
                        }
                        else if (key.Keycode == Key.Escape || key.Keycode == Key.E)
                        {
                            CancelSpellSelect();
                        }
                    }
                    return;
                }

                // Number keys 1-4 select enemy
                if (@event is InputEventKey key2 && key2.Pressed)
                {
                    int num = (int)key2.Keycode - 49; // '1' = 0
                    if (num >= 0 && num < _currentEnemies.Count)
                    {
                        _selectedEnemyIndex = num;
                        RefreshUI();
                    }
                }

                // A = attack, D = defend, F = flee, E = cast spell
                if (@event.IsActionPressed("interact"))
                    ExecuteAttack();
                else if (@event.IsActionPressed("turn_left"))
                    ExecuteDefend();
                else if (@event.IsActionPressed("move_backward"))
                    ExecuteFlee();
                else if (@event.IsActionPressed("strafe_right"))
                    EnterSpellSelect();
            }
        }

        private void ExecuteAttack()
        {
            var current = _combat.CurrentParticipant;
            if (current?.IsPlayer != true) return;
            if (_selectedEnemyIndex >= _currentEnemies.Count) return;

            var target = _currentEnemies[_selectedEnemyIndex];
            if (!target.IsAlive) return;

            var result = _combat.PlayerAttack(current.Character, target);
            if (result.Hit)
                Sound?.PlayCombatHit();
            else
                Sound?.PlayCombatMiss();
            AfterAction();
        }

        private void ExecuteDefend()
        {
            var current = _combat.CurrentParticipant;
            if (current?.IsPlayer != true) return;

            _combat.PlayerDefend(current.Character);
            AfterAction();
        }

        private void EnterSpellSelect()
        {
            var current = _combat.CurrentParticipant;
            if (current?.IsPlayer != true) return;

            var caster = current.Character;
            _availableSpells.Clear();
            foreach (var spellId in caster.KnownSpells)
            {
                var spell = SpellRegistry.Get(spellId);
                if (spell != null && caster.CombatStats.CurrentMana >= spell.ManaCost)
                    _availableSpells.Add(spell);
            }

            if (_availableSpells.Count == 0)
            {
                AppendLog("[color=gray]Keine Zauber verfuegbar.[/color]");
                return;
            }

            _spellSelectMode = true;
            _selectedSpellIndex = 0;
            _actionButtons.Visible = false;
            RefreshSpellSelect();
        }

        private void RefreshSpellSelect()
        {
            foreach (Node child in _spellSelectList.GetChildren())
                child.QueueFree();

            _spellSelectList.Visible = true;

            var title = new Label { Text = "Zauber waehlen (1-9 = auswaehlen, F = wirken, Esc = abbrechen):" };
            title.AddThemeColorOverride("font_color", SelectedColor);
            title.AddThemeFontSizeOverride("font_size", 14);
            _spellSelectList.AddChild(title);

            for (int i = 0; i < _availableSpells.Count; i++)
            {
                var spell = _availableSpells[i];
                bool selected = i == _selectedSpellIndex;

                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 8);

                var lbl = new Label();
                lbl.Text = $"{(selected ? "▶" : "  ")} [{i + 1}] {spell.Name} ({spell.ManaCost} MP) - {spell.Description}";
                lbl.AddThemeColorOverride("font_color", selected ? SelectedColor : TextColor);
                lbl.AddThemeFontSizeOverride("font_size", 13);
                row.AddChild(lbl);

                _spellSelectList.AddChild(row);
            }
        }

        private void ConfirmSpellCast()
        {
            if (_selectedSpellIndex >= _availableSpells.Count) return;

            var current = _combat.CurrentParticipant;
            var caster = current.Character;
            var spellToCast = _availableSpells[_selectedSpellIndex];

            object target = null;
            if (spellToCast.TargetType == SpellTarget.Enemy && _selectedEnemyIndex < _currentEnemies.Count)
                target = _currentEnemies[_selectedEnemyIndex];
            else if (spellToCast.TargetType == SpellTarget.Ally || spellToCast.TargetType == SpellTarget.Self ||
                     spellToCast.TargetType == SpellTarget.AllAllies)
                target = caster;
            else if (spellToCast.TargetType == SpellTarget.AllEnemies)
                target = _currentEnemies.Count > 0 ? _currentEnemies[0] : null;

            CancelSpellSelect();

            if (target == null)
            {
                AppendLog("[color=gray]Kein gueltiges Ziel.[/color]");
                return;
            }

            var result = SpellSystem.CastSpell(caster, spellToCast, target);
            _combat.Log.Add(result.Message);

            Sound?.PlaySpellCast();

            // Apply status effects from certain spells
            if (result.Success && target is Enemy enemy)
            {
                if (spellToCast.Id == "giftwolke")
                    StatusSystem.ApplyStatus(enemy, StatusEffectType.Poison, 4, 3, caster.Name);
                else if (spellToCast.Id == "saeurestrahl")
                    StatusSystem.ApplyStatus(enemy, StatusEffectType.Burn, 3, 2, caster.Name);
            }

            AfterAction();
        }

        private void CancelSpellSelect()
        {
            _spellSelectMode = false;
            _spellSelectList.Visible = false;
            _actionButtons.Visible = true;
        }

        private void ExecuteFlee()
        {
            _combat.PlayerFlee();
            AfterAction();
        }

        private void AfterAction()
        {
            RefreshUI();
            // Notify the dungeon controller that an action was executed
            var controller = FindParentOfType<Dungeon.DungeonController3D>();
            controller?.OnCombatActionExecuted();
        }

        private T FindParentOfType<T>() where T : Node
        {
            Node p = GetParent();
            while (p != null)
            {
                if (p is T typed) return typed;
                p = p.GetParent();
            }
            return null;
        }

        private void RefreshUI()
        {
            if (_combat == null) return;

            // Check combat end
            if (_combat.Phase == CombatPhase.Victory ||
                _combat.Phase == CombatPhase.Defeat)
            {
                AppendLog(_combat.Phase == CombatPhase.Victory
                    ? "[color=green]Sieg! Alle Gegner wurden besiegt.[/color]"
                    : "[color=red]Niederlage! Die Gruppe wurde besiegt.[/color]");

                // Auto-close after delay
                CallDeferred(nameof(DelayedEndCombat));
                return;
            }

            // Update enemy list
            UpdateEnemyList();

            // Update party row
            UpdatePartyRow();

            // Update action buttons
            UpdateActionButtons();

            // Update log
            UpdateLog();
        }

        private void DelayedEndCombat()
        {
            var timer = new Timer { WaitTime = 2.0f, OneShot = true };
            AddChild(timer);
            timer.Timeout += () => { EndCombat(); timer.QueueFree(); };
            timer.Start();
        }

        private void UpdateEnemyList()
        {
            foreach (Node child in _enemyList.GetChildren())
                child.QueueFree();

            _currentEnemies = new List<Enemy>(_combat.Encounter.AliveEnemies);

            for (int i = 0; i < _currentEnemies.Count; i++)
            {
                var enemy = _currentEnemies[i];
                bool selected = i == _selectedEnemyIndex;

                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 8);

                var label = new Label();
                string prefix = selected ? "▶ " : "  ";
                label.Text = $"{prefix}[{i + 1}] {enemy.Name} (Lvl {enemy.Level})";
                label.AddThemeColorOverride("font_color",
                    selected ? SelectedColor : EnemyColor);
                label.AddThemeFontSizeOverride("font_size", 15);
                row.AddChild(label);

                // Health bar
                var hpLabel = new Label();
                float hpPercent = (float)enemy.Health / enemy.MaxHealth;
                hpLabel.Text = $"  HP: {enemy.Health}/{enemy.MaxHealth}";
                hpLabel.AddThemeColorOverride("font_color",
                    hpPercent > 0.5f ? new Color(0.3f, 0.8f, 0.3f) :
                    hpPercent > 0.25f ? new Color(0.9f, 0.7f, 0.2f) :
                    HealthColor);
                hpLabel.AddThemeFontSizeOverride("font_size", 13);
                row.AddChild(hpLabel);

                _enemyList.AddChild(row);
            }

            if (_currentEnemies.Count == 0)
            {
                var empty = new Label { Text = "Keine Gegner mehr." };
                _enemyList.AddChild(empty);
            }
        }

        private void UpdatePartyRow()
        {
            foreach (Node child in _partyRow.GetChildren())
                child.QueueFree();

            var current = _combat.CurrentParticipant;

            foreach (var member in _combat.Party.Members)
            {
                var panel = new VBoxContainer();
                panel.AddThemeConstantOverride("separation", 2);

                bool isCurrent = current?.Character == member;

                var nameLbl = new Label();
                nameLbl.Text = (isCurrent ? "▶ " : "  ") + member.Name;
                nameLbl.AddThemeColorOverride("font_color",
                    isCurrent ? SelectedColor : TextColor);
                nameLbl.AddThemeFontSizeOverride("font_size", 14);
                panel.AddChild(nameLbl);

                var hpLbl = new Label();
                hpLbl.Text = $"HP: {member.CombatStats.CurrentHealth}/{member.CombatStats.MaxHealth}";
                hpLbl.AddThemeColorOverride("font_color",
                    member.IsAlive ? new Color(0.7f, 0.5f, 0.3f) : HealthColor);
                hpLbl.AddThemeFontSizeOverride("font_size", 12);
                panel.AddChild(hpLbl);

                var mpLbl = new Label();
                mpLbl.Text = $"MP: {member.CombatStats.CurrentMana}/{member.CombatStats.MaxMana}";
                mpLbl.AddThemeColorOverride("font_color", new Color(0.3f, 0.5f, 0.8f));
                mpLbl.AddThemeFontSizeOverride("font_size", 12);
                panel.AddChild(mpLbl);

                _partyRow.AddChild(panel);
            }
        }

        private void UpdateActionButtons()
        {
            foreach (Node child in _actionButtons.GetChildren())
                child.QueueFree();

            var current = _combat.CurrentParticipant;
            bool isPlayerTurn = current?.IsPlayer == true && _combat.Phase == CombatPhase.PlayerTurn;

            if (isPlayerTurn)
            {
                AddActionButton("F: Angreifen", ExecuteAttack);
                AddActionButton("E: Zauber", EnterSpellSelect);
                AddActionButton("A: Verteidigen", ExecuteDefend);
                AddActionButton("S: Fliehen", ExecuteFlee);
            }
            else
            {
                var waitLbl = new Label { Text = "Gegner am Zug..." };
                waitLbl.AddThemeColorOverride("font_color", EnemyColor);
                waitLbl.AddThemeFontSizeOverride("font_size", 15);
                _actionButtons.AddChild(waitLbl);
            }
        }

        private void AddActionButton(string text, System.Action action)
        {
            var btn = new Button { Text = text };
            btn.AddThemeFontSizeOverride("font_size", 14);
            btn.AddThemeColorOverride("font_color", TextColor);
            btn.CustomMinimumSize = new Vector2(160, 36);
            btn.Pressed += action;
            _actionButtons.AddChild(btn);
        }

        private void AppendLog(string text)
        {
            _combatLog.AppendText(text + "\n");
        }

        private void UpdateLog()
        {
            _combatLog.Clear();
            foreach (var line in _combat.Log)
            {
                string colored = line;
                if (line.Contains("kritisch"))
                    colored = $"[color=yellow]{line}[/color]";
                else if (line.Contains("verfehlt"))
                    colored = $"[color=gray]{line}[/color]";
                else if (line.Contains("trifft"))
                    colored = $"[color=orange]{line}[/color]";
                else if (line.Contains("Sieg") || line.Contains("gewonnen"))
                    colored = $"[color=green]{line}[/color]";
                else if (line.Contains("Niederlage") || line.Contains("besiegt"))
                    colored = $"[color=red]{line}[/color]";

                _combatLog.AppendText(colored + "\n");
            }
        }
    }
}
