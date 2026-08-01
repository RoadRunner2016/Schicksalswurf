using Godot;

namespace Schicksalswurf.UI
{
    using Dungeon;
    using Characters;

    /// <summary>
    /// Dialogue UI for NPC interactions.
    /// </summary>
    public partial class DialogueUI : Control
    {
        private NPC _npc;
        private string _currentNodeId;
        private Party _party;
        private bool _isActive = false;

        private PanelContainer _mainPanel;
        private Label _npcNameLabel;
        private Label _dialogueText;
        private VBoxContainer _optionsList;

        public bool ShopRequested { get; set; } = false;
        public bool HealRequested { get; set; } = false;
        public string QuestToStart { get; set; } = null;

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.95f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color SelectedColor = new(1.0f, 0.85f, 0.2f);
        private static readonly Color DimColor = new(0.55f, 0.52f, 0.45f);

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.Center);
            MouseFilter = MouseFilterEnum.Stop;
            Visible = false;
            BuildUI();
        }

        private void BuildUI()
        {
            _mainPanel = new PanelContainer();
            _mainPanel.CustomMinimumSize = new Vector2(700, 0);
            _mainPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = PanelBg,
                BorderWidthLeft = 3, BorderWidthRight = 3,
                BorderWidthTop = 3, BorderWidthBottom = 3,
                BorderColor = BorderColor,
                ContentMarginLeft = 24, ContentMarginRight = 24,
                ContentMarginTop = 20, ContentMarginBottom = 20
            });
            AddChild(_mainPanel);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(container);

            _npcNameLabel = new Label();
            _npcNameLabel.AddThemeFontSizeOverride("font_size", 20);
            _npcNameLabel.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(_npcNameLabel);

            _dialogueText = new Label();
            _dialogueText.AddThemeFontSizeOverride("font_size", 15);
            _dialogueText.AddThemeColorOverride("font_color", TextColor);
            _dialogueText.CustomMinimumSize = new Vector2(0, 60);
            _dialogueText.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            container.AddChild(_dialogueText);

            _optionsList = new VBoxContainer();
            _optionsList.AddThemeConstantOverride("separation", 5);
            container.AddChild(_optionsList);

            var hint = new Label { Text = "Esc: Gespraech beenden" };
            hint.AddThemeColorOverride("font_color", DimColor);
            hint.AddThemeFontSizeOverride("font_size", 12);
            container.AddChild(hint);
        }

        public void StartDialogue(NPC npc, Party party)
        {
            _npc = npc;
            _party = party;
            _currentNodeId = npc.StartNodeId;
            ShopRequested = false;
            HealRequested = false;
            QuestToStart = null;
            _isActive = true;
            Visible = true;
            RefreshUI();
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!_isActive) return;

            if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            {
                Close();
                GetViewport().SetInputAsHandled();
            }
        }

        private void RefreshUI()
        {
            if (_npc == null) return;

            _npcNameLabel.Text = _npc.Name;

            var node = _npc.Dialogue.TryGetValue(_currentNodeId, out var n) ? n : null;
            if (node == null)
            {
                Close();
                return;
            }

            _dialogueText.Text = node.Text;

            foreach (Node child in _optionsList.GetChildren())
                child.QueueFree();

            for (int i = 0; i < node.Options.Count; i++)
            {
                var option = node.Options[i];
                var btn = new Button { Text = $"{i + 1}. {option.Text}" };
                btn.AddThemeFontSizeOverride("font_size", 14);
                btn.AddThemeColorOverride("font_color", TextColor);
                btn.CustomMinimumSize = new Vector2(0, 32);

                var capturedOption = option;
                btn.Pressed += () => SelectOption(capturedOption);

                _optionsList.AddChild(btn);
            }
        }

        private void SelectOption(DialogueOption option)
        {
            // Handle special actions
            if (_npc.IsMerchant && option.Text == "Zeig mir deine Waren.")
                ShopRequested = true;
            else if (_npc.IsHealer && option.Text == "Heile die gesamte Gruppe.")
            {
                HealRequested = true;
                if (_party != null)
                {
                    int cost = _npc.HealCost * _party.Members.Count;
                    if (_party.Gold >= cost)
                    {
                        _party.Gold -= cost;
                        foreach (var m in _party.Members)
                            m.Rest();
                    }
                }
            }
            else if (_npc.IsQuestGiver && option.Text == "Ich werde helfen.")
            {
                QuestToStart = "kill_goblins";
            }

            if (option.OnSelect != null)
                option.OnSelect();

            if (string.IsNullOrEmpty(option.NextNodeId) || option.NextNodeId == "end")
            {
                Close();
                return;
            }

            _currentNodeId = option.NextNodeId;
            RefreshUI();
        }
    }
}
