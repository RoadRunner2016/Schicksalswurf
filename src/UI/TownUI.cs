using Godot;

namespace Schicksalswurf.UI
{
    using Characters;
    using Dungeon;

    /// <summary>
    /// Town/Hub UI shown between dungeon runs. Allows healing, shopping, quest management,
    /// and re-entering the dungeon.
    /// </summary>
    public partial class TownUI : Control
    {
        private Party _party;
        private bool _isActive = false;

        private PanelContainer _mainPanel;
        private Label _titleLabel;
        private Label _infoLabel;
        private VBoxContainer _buttonList;

        public bool EnterDungeonRequested { get; set; } = false;
        public bool ShopRequested { get; set; } = false;

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.98f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
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
            _mainPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = PanelBg,
                BorderWidthLeft = 3, BorderWidthRight = 3,
                BorderWidthTop = 3, BorderWidthBottom = 3,
                BorderColor = BorderColor,
                ContentMarginLeft = 60, ContentMarginRight = 60,
                ContentMarginTop = 40, ContentMarginBottom = 40
            });
            AddChild(_mainPanel);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 15);
            _mainPanel.AddChild(container);

            _titleLabel = new Label { Text = "🏰 STADT 🏰" };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 32);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(_titleLabel);

            _infoLabel = new Label();
            _infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _infoLabel.AddThemeFontSizeOverride("font_size", 15);
            _infoLabel.AddThemeColorOverride("font_color", TextColor);
            _infoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            container.AddChild(_infoLabel);

            _buttonList = new VBoxContainer();
            _buttonList.AddThemeConstantOverride("separation", 10);
            _buttonList.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            container.AddChild(_buttonList);
        }

        public void ShowTown(Party party)
        {
            _party = party;
            _isActive = true;
            Visible = true;
            EnterDungeonRequested = false;
            RefreshUI();
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;

        private void RefreshUI()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Gold: {_party.Gold}");
            sb.AppendLine();
            sb.AppendLine("Helden:");
            foreach (var m in _party.Members)
            {
                sb.AppendLine($"  {m.Name} - {m.Archetype} (Lvl {m.Level}) - HP: {m.CombatStats.CurrentHealth}/{m.CombatStats.MaxHealth}");
            }

            // Show completed quests
            var completed = QuestRegistry.GetCompletedQuests();
            if (completed.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Abgeschlossene Quests (Belohnung abholen):");
                foreach (var q in completed)
                    sb.AppendLine($"  • {q.Title} - {q.GoldReward}G + {q.ExpReward}EP");
            }

            _infoLabel.Text = sb.ToString();

            foreach (Node child in _buttonList.GetChildren())
                child.QueueFree();

            AddButton("Rasten (volle Heilung)", OnRest);
            AddButton("Haendler besuchen", OnShop);
            AddButton("Quest-Belohnungen abholen", OnClaimQuests);
            AddButton("Dungeon betreten", OnEnterDungeon);
        }

        private void AddButton(string text, System.Action action)
        {
            var btn = new Button { Text = text };
            btn.AddThemeFontSizeOverride("font_size", 16);
            btn.CustomMinimumSize = new Vector2(300, 40);
            btn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            btn.Pressed += action;
            _buttonList.AddChild(btn);
        }

        private void OnRest()
        {
            foreach (var m in _party.Members)
                m.Rest();
            RefreshUI();
        }

        private void OnShop()
        {
            ShopRequested = true;
        }

        private void OnClaimQuests()
        {
            var completed = QuestRegistry.GetCompletedQuests();
            foreach (var q in completed)
                QuestRegistry.ClaimReward(q, _party);
            RefreshUI();
        }

        private void OnEnterDungeon()
        {
            EnterDungeonRequested = true;
            Close();
        }
    }
}
