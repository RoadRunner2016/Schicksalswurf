using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.UI
{
    using Core;
    using Characters;

    /// <summary>
    /// Character sheet UI showing attributes, skills, equipment, and inventory.
    /// Toggle with 'C' key. Cycles through party members with Tab.
    /// </summary>
    public partial class CharacterSheetUI : Control
    {
        private Party _party;
        private int _selectedMember = 0;
        private bool _isActive = false;

        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        private HBoxContainer _memberTabs;
        private Label _nameLabel;
        private GridContainer _attrGrid;
        private VBoxContainer _skillList;
        private VBoxContainer _equipmentBox;
        private VBoxContainer _inventoryBox;
        private Label _combatStatsLabel;

        private static readonly Color PanelBg = new(0.08f, 0.08f, 0.12f, 0.95f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color SelectedColor = new(1.0f, 0.85f, 0.2f);
        private static readonly Color DimColor = new(0.55f, 0.52f, 0.45f);

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            Visible = false;
            BuildUI();
        }

        private void BuildUI()
        {
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
            AddChild(_mainPanel);

            // Scroll container for content
            var scroll = new ScrollContainer();
            scroll.CustomMinimumSize = new Vector2(0, 700);
            _mainPanel.AddChild(scroll);

            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 10);
            _mainContainer.CustomMinimumSize = new Vector2(900, 0);
            _mainContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            scroll.AddChild(_mainContainer);

            // Title
            var title = new Label { Text = "📜 CHARAKTERBOGEN 📜" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 22);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _mainContainer.AddChild(title);

            // Member tabs
            _memberTabs = new HBoxContainer();
            _memberTabs.AddThemeConstantOverride("separation", 12);
            _memberTabs.Alignment = BoxContainer.AlignmentMode.Center;
            _mainContainer.AddChild(_memberTabs);

            // Character name
            _nameLabel = new Label();
            _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _nameLabel.AddThemeFontSizeOverride("font_size", 18);
            _nameLabel.AddThemeColorOverride("font_color", SelectedColor);
            _mainContainer.AddChild(_nameLabel);

            // Two-column layout: left = attributes + combat stats, right = skills
            var columns = new HBoxContainer();
            columns.AddThemeConstantOverride("separation", 24);
            columns.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainContainer.AddChild(columns);

            // Left column
            var leftCol = new VBoxContainer();
            leftCol.AddThemeConstantOverride("separation", 8);
            leftCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            columns.AddChild(leftCol);

            // Attributes
            var attrTitle = new SectionLabel("Attribute");
            leftCol.AddChild(attrTitle);

            _attrGrid = new GridContainer();
            _attrGrid.Columns = 2;
            _attrGrid.AddThemeConstantOverride("h_separation", 20);
            _attrGrid.AddThemeConstantOverride("v_separation", 4);
            leftCol.AddChild(_attrGrid);

            // Combat stats
            var combatTitle = new SectionLabel("Kampfwerte");
            leftCol.AddChild(combatTitle);

            _combatStatsLabel = new Label();
            _combatStatsLabel.AddThemeColorOverride("font_color", TextColor);
            _combatStatsLabel.AddThemeFontSizeOverride("font_size", 13);
            leftCol.AddChild(_combatStatsLabel);

            // Equipment
            var equipTitle = new SectionLabel("Ausrüstung");
            leftCol.AddChild(equipTitle);

            _equipmentBox = new VBoxContainer();
            _equipmentBox.AddThemeConstantOverride("separation", 3);
            leftCol.AddChild(_equipmentBox);

            // Inventory
            var invTitle = new SectionLabel("Inventar");
            leftCol.AddChild(invTitle);

            _inventoryBox = new VBoxContainer();
            _inventoryBox.AddThemeConstantOverride("separation", 3);
            leftCol.AddChild(_inventoryBox);

            // Right column: skills
            var rightCol = new VBoxContainer();
            rightCol.AddThemeConstantOverride("separation", 8);
            rightCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            columns.AddChild(rightCol);

            var skillTitle = new SectionLabel("Talente");
            rightCol.AddChild(skillTitle);

            _skillList = new VBoxContainer();
            _skillList.AddThemeConstantOverride("separation", 3);
            rightCol.AddChild(_skillList);

            // Hint
            var hint = new Label { Text = "Tab: Charakter wechseln  |  C: Schließen" };
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            hint.AddThemeColorOverride("font_color", DimColor);
            hint.AddThemeFontSizeOverride("font_size", 12);
            _mainContainer.AddChild(hint);
        }

        private StyleBoxFlat CreatePanelStyle()
        {
            return new StyleBoxFlat
            {
                BgColor = PanelBg,
                BorderWidthLeft = 3,
                BorderWidthRight = 3,
                BorderWidthTop = 3,
                BorderWidthBottom = 3,
                BorderColor = BorderColor,
                ContentMarginLeft = 20,
                ContentMarginRight = 20,
                ContentMarginTop = 16,
                ContentMarginBottom = 16
            };
        }

        public void SetParty(Party party)
        {
            _party = party;
        }

        public void Toggle()
        {
            if (_party == null || _party.Members.Count == 0) return;
            _isActive = !_isActive;
            Visible = _isActive;
            if (_isActive)
            {
                _selectedMember = 0;
                RefreshUI();
            }
        }

        public bool IsActive => _isActive;

        public void NextMember()
        {
            if (_party == null) return;
            _selectedMember = (_selectedMember + 1) % _party.Members.Count;
            RefreshUI();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!_isActive) return;

            if (@event is InputEventKey key && key.Pressed)
            {
                if (key.Keycode == Key.Tab)
                {
                    NextMember();
                    GetViewport().SetInputAsHandled();
                }
                else if (key.Keycode == Key.C)
                {
                    Toggle();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        private void RefreshUI()
        {
            if (_party == null || _party.Members.Count == 0) return;

            var member = _party.Members[_selectedMember];

            // Member tabs
            foreach (Node child in _memberTabs.GetChildren())
                child.QueueFree();

            for (int i = 0; i < _party.Members.Count; i++)
            {
                var m = _party.Members[i];
                bool selected = i == _selectedMember;
                var lbl = new Label();
                lbl.Text = selected ? $"▶ {m.Name}" : $"  {m.Name}  ";
                lbl.AddThemeColorOverride("font_color", selected ? SelectedColor : DimColor);
                lbl.AddThemeFontSizeOverride("font_size", 15);
                _memberTabs.AddChild(lbl);
            }

            // Name + archetype
            _nameLabel.Text = $"{member.Name} - {member.Archetype} (Level {member.Level}, {member.Experience} EP)";
            if (member.UnspentAttributePoints > 0 || member.UnspentSkillPoints > 0)
                _nameLabel.Text += $"  [ATR:{member.UnspentAttributePoints} / TAL:{member.UnspentSkillPoints}]";

            // Attributes with + buttons if points available
            foreach (Node child in _attrGrid.GetChildren())
                child.QueueFree();

            foreach (Attribute attr in System.Enum.GetValues(typeof(Attribute)))
            {
                var nameLbl = new Label();
                nameLbl.Text = $"{AttributeInfo.GetName(attr)} ({AttributeInfo.GetShort(attr)}):";
                nameLbl.AddThemeColorOverride("font_color", TextColor);
                nameLbl.AddThemeFontSizeOverride("font_size", 14);
                _attrGrid.AddChild(nameLbl);

                var valRow = new HBoxContainer();
                valRow.AddThemeConstantOverride("separation", 4);

                var valLbl = new Label();
                valLbl.Text = member.Attributes[attr].ToString();
                valLbl.AddThemeColorOverride("font_color", SelectedColor);
                valLbl.AddThemeFontSizeOverride("font_size", 14);
                valRow.AddChild(valLbl);

                if (member.UnspentAttributePoints > 0)
                {
                    var plusBtn = new Button { Text = "+", CustomMinimumSize = new Vector2(24, 20) };
                    plusBtn.AddThemeFontSizeOverride("font_size", 11);
                    var capturedAttr = attr;
                    plusBtn.Pressed += () => { member.IncreaseAttribute(capturedAttr); RefreshUI(); };
                    valRow.AddChild(plusBtn);
                }

                _attrGrid.AddChild(valRow);
            }

            // Combat stats
            var cs = member.CombatStats;
            _combatStatsLabel.Text =
                $"Leben:     {cs.CurrentHealth}/{cs.MaxHealth}\n" +
                $"Ausdauer:  {cs.CurrentStamina}/{cs.MaxStamina}\n" +
                $"Mana:      {cs.CurrentMana}/{cs.MaxMana}\n" +
                $"Initiative: {cs.Initiative}  Angriff: {member.GetAttackValue()}  Verteidigung: {member.GetDefenseValue()}\n" +
                $"Schaden:   {member.GetDamage()}  Rüstung: {member.GetSoak()}  Tragkraft: {cs.CarryCapacity}";

            // Equipment
            foreach (Node child in _equipmentBox.GetChildren())
                child.QueueFree();

            AddInfoLine(_equipmentBox, "Waffe:", member.EquippedWeapon?.Name ?? "-");
            AddInfoLine(_equipmentBox, "Rüstung:", member.EquippedArmor?.Name ?? "-");

            // Inventory
            foreach (Node child in _inventoryBox.GetChildren())
                child.QueueFree();

            if (member.Inventory.Count == 0 && _party.SharedInventory.Count == 0)
            {
                var empty = new Label { Text = "  (leer)" };
                empty.AddThemeColorOverride("font_color", DimColor);
                empty.AddThemeFontSizeOverride("font_size", 12);
                _inventoryBox.AddChild(empty);
            }
            else
            {
                foreach (var item in member.Inventory)
                    AddInfoLine(_inventoryBox, "•", $"{item.Name} ({item.Value}G)");
                foreach (var item in _party.SharedInventory)
                    AddInfoLine(_inventoryBox, "•", $"{item.Name} ({item.Value}G) [Gruppe]");
            }

            AddInfoLine(_inventoryBox, "Gold:", $"{_party.Gold}G");

            // Skills
            foreach (Node child in _skillList.GetChildren())
                child.QueueFree();

            // Group skills by category
            foreach (SkillCategory cat in System.Enum.GetValues(typeof(SkillCategory)))
            {
                bool hasSkillsInCategory = false;
                var catLabel = new Label();
                catLabel.Text = $"  {cat}:";
                catLabel.AddThemeColorOverride("font_color", HeaderColor);
                catLabel.AddThemeFontSizeOverride("font_size", 13);
                _skillList.AddChild(catLabel);

                foreach (var skill in SkillRegistry.GetByCategory(cat))
                {
                    int value = member.Skills[skill.Id];
                    if (value > 0)
                    {
                        hasSkillsInCategory = true;
                        var skillRow = new HBoxContainer();
                        skillRow.AddThemeConstantOverride("separation", 4);

                        var lbl = new Label();
                        lbl.Text = $"    {skill.Name}: {value}";
                        lbl.AddThemeColorOverride("font_color", TextColor);
                        lbl.AddThemeFontSizeOverride("font_size", 13);
                        skillRow.AddChild(lbl);

                        if (member.UnspentSkillPoints > 0)
                        {
                            var plusBtn = new Button { Text = "+", CustomMinimumSize = new Vector2(24, 18) };
                            plusBtn.AddThemeFontSizeOverride("font_size", 10);
                            var capturedSkillId = skill.Id;
                            plusBtn.Pressed += () => { member.IncreaseSkill(capturedSkillId); RefreshUI(); };
                            skillRow.AddChild(plusBtn);
                        }

                        _skillList.AddChild(skillRow);
                    }
                }

                if (!hasSkillsInCategory)
                {
                    catLabel.Text = $"  {cat}: (keine)";
                    catLabel.AddThemeColorOverride("font_color", DimColor);
                }
            }
        }

        private void AddInfoLine(Node parent, string label, string value)
        {
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 6);

            var lbl = new Label { Text = label };
            lbl.AddThemeColorOverride("font_color", DimColor);
            lbl.AddThemeFontSizeOverride("font_size", 13);
            hbox.AddChild(lbl);

            var val = new Label { Text = value };
            val.AddThemeColorOverride("font_color", TextColor);
            val.AddThemeFontSizeOverride("font_size", 13);
            hbox.AddChild(val);

            parent.AddChild(hbox);
        }

        private partial class SectionLabel : Label
        {
            public SectionLabel(string text)
            {
                Text = text;
                AddThemeColorOverride("font_color", HeaderColor);
                AddThemeFontSizeOverride("font_size", 16);
            }
        }
    }
}
