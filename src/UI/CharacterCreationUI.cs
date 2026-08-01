using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.UI
{
    using Characters;
    using Core;

    /// <summary>
    /// Character creation screen. Lets the player create up to 4 party members
    /// with name, archetype selection, and bonus attribute distribution.
    /// </summary>
    public partial class CharacterCreationUI : Control
    {
        private Party _party;
        private int _currentSlot = 0;
        private const int MaxMembers = 4;

        private string _enteredName = "";
        private Archetype _selectedArchetype = Archetype.Krieger;
        private int _bonusPoints = 5;
        private Dictionary<Attribute, int> _bonusAttrs = new();

        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private Label _slotLabel;
        private LineEdit _nameInput;
        private VBoxContainer _archetypeList;
        private GridContainer _attrGrid;
        private Label _bonusPointsLabel;
        private Button _confirmButton;
        private Button _startButton;
        private Label _summaryLabel;

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.97f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color SelectedColor = new(1.0f, 0.85f, 0.2f);
        private static readonly Color DimColor = new(0.55f, 0.52f, 0.45f);

        public bool IsDone { get; private set; } = false;
        public Party CreatedParty => _party;

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            _party = new Party();
            ResetBonusAttrs();
            BuildUI();
        }

        private void ResetBonusAttrs()
        {
            _bonusAttrs.Clear();
            foreach (Attribute attr in System.Enum.GetValues(typeof(Attribute)))
                _bonusAttrs[attr] = 0;
            _bonusPoints = 5;
        }

        private void BuildUI()
        {
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = PanelBg,
                BorderWidthLeft = 3, BorderWidthRight = 3,
                BorderWidthTop = 3, BorderWidthBottom = 3,
                BorderColor = BorderColor,
                ContentMarginLeft = 30, ContentMarginRight = 30,
                ContentMarginTop = 20, ContentMarginBottom = 20
            });
            AddChild(_mainPanel);

            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 12);
            _mainContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainPanel.AddChild(_mainContainer);

            // Title
            _titleLabel = new Label { Text = "⚔ CHARAKTERERSTELLUNG ⚔" };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            _mainContainer.AddChild(_titleLabel);

            _slotLabel = new Label();
            _slotLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _slotLabel.AddThemeFontSizeOverride("font_size", 16);
            _slotLabel.AddThemeColorOverride("font_color", SelectedColor);
            _mainContainer.AddChild(_slotLabel);

            // Two columns
            var columns = new HBoxContainer();
            columns.AddThemeConstantOverride("separation", 30);
            columns.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainContainer.AddChild(columns);

            // Left: name + archetype
            var leftCol = new VBoxContainer();
            leftCol.AddThemeConstantOverride("separation", 8);
            leftCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            columns.AddChild(leftCol);

            var nameLabel = new Label { Text = "Name:" };
            nameLabel.AddThemeColorOverride("font_color", HeaderColor);
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            leftCol.AddChild(nameLabel);

            _nameInput = new LineEdit();
            _nameInput.CustomMinimumSize = new Vector2(250, 0);
            _nameInput.AddThemeFontSizeOverride("font_size", 16);
            _nameInput.TextChanged += OnNameChanged;
            leftCol.AddChild(_nameInput);

            var archTitle = new Label { Text = "Archetyp:" };
            archTitle.AddThemeColorOverride("font_color", HeaderColor);
            archTitle.AddThemeFontSizeOverride("font_size", 16);
            leftCol.AddChild(archTitle);

            _archetypeList = new VBoxContainer();
            _archetypeList.AddThemeConstantOverride("separation", 4);
            leftCol.AddChild(_archetypeList);

            // Right: attributes
            var rightCol = new VBoxContainer();
            rightCol.AddThemeConstantOverride("separation", 8);
            rightCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            columns.AddChild(rightCol);

            var attrTitle = new Label { Text = "Attributspunkte verteilen:" };
            attrTitle.AddThemeColorOverride("font_color", HeaderColor);
            attrTitle.AddThemeFontSizeOverride("font_size", 16);
            rightCol.AddChild(attrTitle);

            _bonusPointsLabel = new Label();
            _bonusPointsLabel.AddThemeColorOverride("font_color", SelectedColor);
            _bonusPointsLabel.AddThemeFontSizeOverride("font_size", 14);
            rightCol.AddChild(_bonusPointsLabel);

            _attrGrid = new GridContainer();
            _attrGrid.Columns = 3;
            _attrGrid.AddThemeConstantOverride("h_separation", 12);
            _attrGrid.AddThemeConstantOverride("v_separation", 6);
            rightCol.AddChild(_attrGrid);

            // Summary
            _summaryLabel = new Label();
            _summaryLabel.AddThemeColorOverride("font_color", TextColor);
            _summaryLabel.AddThemeFontSizeOverride("font_size", 13);
            _summaryLabel.CustomMinimumSize = new Vector2(0, 60);
            _mainContainer.AddChild(_summaryLabel);

            // Buttons
            var buttonRow = new HBoxContainer();
            buttonRow.AddThemeConstantOverride("separation", 16);
            buttonRow.Alignment = BoxContainer.AlignmentMode.Center;
            _mainContainer.AddChild(buttonRow);

            _confirmButton = new Button { Text = "Charakter bestaetigen" };
            _confirmButton.AddThemeFontSizeOverride("font_size", 16);
            _confirmButton.CustomMinimumSize = new Vector2(220, 40);
            _confirmButton.Pressed += ConfirmCharacter;
            buttonRow.AddChild(_confirmButton);

            _startButton = new Button { Text = "Spiel starten (min. 1 Held)" };
            _startButton.AddThemeFontSizeOverride("font_size", 16);
            _startButton.CustomMinimumSize = new Vector2(220, 40);
            _startButton.Pressed += StartGame;
            buttonRow.AddChild(_startButton);

            RefreshUI();
        }

        private void OnNameChanged(string text)
        {
            _enteredName = text;
        }

        private void SelectArchetype(Archetype arch)
        {
            _selectedArchetype = arch;
            RefreshUI();
        }

        private void IncreaseAttr(Attribute attr)
        {
            if (_bonusPoints <= 0) return;
            _bonusAttrs[attr]++;
            _bonusPoints--;
            RefreshUI();
        }

        private void DecreaseAttr(Attribute attr)
        {
            if (_bonusAttrs[attr] <= 0) return;
            _bonusAttrs[attr]--;
            _bonusPoints++;
            RefreshUI();
        }

        private void ConfirmCharacter()
        {
            string name = string.IsNullOrWhiteSpace(_enteredName) ? $"Held {_currentSlot + 1}" : _enteredName;

            var character = new Character(name, _selectedArchetype);

            // Apply bonus attributes
            foreach (var kv in _bonusAttrs)
            {
                if (kv.Value > 0)
                    character.Attributes[kv.Key] = character.Attributes[kv.Key] + kv.Value;
            }

            // Recalculate combat stats with new attributes
            character.CombatStats = CombatStats.Calculate(character.Attributes, character.Level);

            // Give starting potions
            character.Inventory.Add(Item.HealthPotion);
            character.Inventory.Add(Item.HealthPotion);

            _party.AddMember(character);

            _currentSlot++;

            if (_currentSlot >= MaxMembers)
            {
                StartGame();
                return;
            }

            // Reset for next character
            _enteredName = "";
            _nameInput.Text = "";
            _selectedArchetype = Archetype.Krieger;
            ResetBonusAttrs();
            RefreshUI();
        }

        private void StartGame()
        {
            if (_party.Members.Count == 0)
            {
                // Create a default party if none was made
                _party.AddMember(new Character("Aldric", Archetype.Krieger));
                _party.AddMember(new Character("Lyra", Archetype.Schurke));
                _party.AddMember(new Character("Mavros", Archetype.Magier));
                _party.AddMember(new Character("Sera", Archetype.Heiler));
            }

            IsDone = true;
            Visible = false;
        }

        private void RefreshUI()
        {
            _slotLabel.Text = $"Charakter {_currentSlot + 1} / {MaxMembers}  ({_party.Members.Count} erstellt)";

            // Archetype list
            foreach (Node child in _archetypeList.GetChildren())
                child.QueueFree();

            foreach (Archetype arch in System.Enum.GetValues(typeof(Archetype)))
            {
                bool selected = arch == _selectedArchetype;
                var data = ArchetypeData.Data[arch];

                var hbox = new HBoxContainer();
                hbox.AddThemeConstantOverride("separation", 8);

                var nameLbl = new Label();
                nameLbl.Text = (selected ? "▶ " : "  ") + ArchetypeData.GetName(arch);
                nameLbl.AddThemeColorOverride("font_color", selected ? SelectedColor : TextColor);
                nameLbl.AddThemeFontSizeOverride("font_size", 15);
                hbox.AddChild(nameLbl);

                var descLbl = new Label();
                descLbl.Text = data.desc;
                descLbl.AddThemeColorOverride("font_color", DimColor);
                descLbl.AddThemeFontSizeOverride("font_size", 12);
                descLbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                hbox.AddChild(descLbl);

                var btn = new Button { Text = "Waehlen" };
                btn.AddThemeFontSizeOverride("font_size", 12);
                btn.Pressed += () => SelectArchetype(arch);
                hbox.AddChild(btn);

                _archetypeList.AddChild(hbox);
            }

            // Bonus points
            _bonusPointsLabel.Text = $"Verfuegbare Bonuspunkte: {_bonusPoints}";

            // Attribute grid
            foreach (Node child in _attrGrid.GetChildren())
                child.QueueFree();

            var baseAttrs = ArchetypeData.Data[_selectedArchetype].attrs;
            foreach (Attribute attr in System.Enum.GetValues(typeof(Attribute)))
            {
                var nameLbl = new Label();
                nameLbl.Text = AttributeInfo.GetShort(attr);
                nameLbl.AddThemeColorOverride("font_color", TextColor);
                nameLbl.AddThemeFontSizeOverride("font_size", 13);
                _attrGrid.AddChild(nameLbl);

                int baseVal = baseAttrs[attr];
                int bonus = _bonusAttrs[attr];
                var valLbl = new Label();
                valLbl.Text = $"{baseVal} + {bonus} = {baseVal + bonus}";
                valLbl.AddThemeColorOverride("font_color", bonus > 0 ? SelectedColor : TextColor);
                valLbl.AddThemeFontSizeOverride("font_size", 13);
                _attrGrid.AddChild(valLbl);

                var btnBox = new HBoxContainer();
                btnBox.AddThemeConstantOverride("separation", 4);

                var plusBtn = new Button { Text = "+", CustomMinimumSize = new Vector2(28, 24) };
                plusBtn.Pressed += () => IncreaseAttr(attr);
                btnBox.AddChild(plusBtn);

                var minusBtn = new Button { Text = "-", CustomMinimumSize = new Vector2(28, 24) };
                minusBtn.Pressed += () => DecreaseAttr(attr);
                btnBox.AddChild(minusBtn);

                _attrGrid.AddChild(btnBox);
            }

            // Summary
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Name: {(string.IsNullOrWhiteSpace(_enteredName) ? "(leer)" : _enteredName)}");
            sb.AppendLine($"Archetyp: {_selectedArchetype}");
            sb.AppendLine($"Startwerte: KP {baseAttrs[Attribute.Kraft]}, GE {baseAttrs[Attribute.Gewandtheit]}, KO {baseAttrs[Attribute.Konstitution]}");
            _summaryLabel.Text = sb.ToString();

            // Button states
            _startButton.Text = _party.Members.Count > 0
                ? $"Spiel starten ({_party.Members.Count} Held{(_party.Members.Count > 1 ? "en" : "")})"
                : "Spiel starten (min. 1 Held)";
        }
    }
}
