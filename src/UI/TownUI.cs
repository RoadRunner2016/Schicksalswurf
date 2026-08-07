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
        private PanelContainer _questPanel;
        private Label _questLabel;

        public bool EnterDungeonRequested { get; set; } = false;
        public bool ShopRequested { get; set; } = false;

        private static readonly Color PanelBg = new(0.04f, 0.04f, 0.08f, 0.98f);
        private static readonly Color BorderColor = new(0.45f, 0.35f, 0.12f, 0.9f);
        private static readonly Color HeaderColor = new(0.95f, 0.78f, 0.28f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.5f, 0.48f, 0.4f);
        private static readonly Color AccentColor = new(0.6f, 0.45f, 0.15f);
        private static readonly Color SectionBg = new(0.08f, 0.07f, 0.05f, 0.85f);
        private static readonly Color GoldColor = new(0.85f, 0.7f, 0.2f);

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
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 60, ContentMarginRight = 60,
                ContentMarginTop = 40, ContentMarginBottom = 40
            });
            AddChild(_mainPanel);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 14);
            _mainPanel.AddChild(container);

            _titleLabel = new Label { Text = "STADT" };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 34);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(_titleLabel);

            var decorLine = new HSeparator();
            decorLine.AddThemeStyleboxOverride("separator", new StyleBoxFlat
            {
                BgColor = AccentColor,
                ContentMarginTop = 2, ContentMarginBottom = 6
            });
            container.AddChild(decorLine);

            // Info panel (gold + party)
            var infoPanel = new PanelContainer();
            infoPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = SectionBg,
                BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2,
                BorderColor = AccentColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 16, ContentMarginRight = 16,
                ContentMarginTop = 12, ContentMarginBottom = 12
            });
            container.AddChild(infoPanel);

            _infoLabel = new Label();
            _infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _infoLabel.AddThemeFontSizeOverride("font_size", 15);
            _infoLabel.AddThemeColorOverride("font_color", TextColor);
            _infoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            infoPanel.AddChild(_infoLabel);

            // Quest panel (hidden unless quests completed)
            _questPanel = new PanelContainer();
            _questPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.08f, 0.03f, 0.85f),
                BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2,
                BorderColor = GoldColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 16, ContentMarginRight = 16,
                ContentMarginTop = 12, ContentMarginBottom = 12
            });
            _questPanel.Visible = false;
            container.AddChild(_questPanel);

            _questLabel = new Label();
            _questLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _questLabel.AddThemeFontSizeOverride("font_size", 14);
            _questLabel.AddThemeColorOverride("font_color", GoldColor);
            _questPanel.AddChild(_questLabel);

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

        public bool IsActive => Visible;

        private void RefreshUI()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Gold: {_party.Gold}");
            sb.AppendLine();
            sb.AppendLine("Helden:");
            foreach (var m in _party.Members)
            {
                string status = m.CombatStats.CurrentHealth > 0 ? "OK" : "Bewusstlos";
                sb.AppendLine($"  {m.Name} - {m.Archetype} (Lvl {m.Level}) - HP: {m.CombatStats.CurrentHealth}/{m.CombatStats.MaxHealth} [{status}]");
            }

            _infoLabel.Text = sb.ToString();

            // Show completed quests
            var completed = QuestRegistry.GetCompletedQuests();
            if (completed.Count > 0)
            {
                var questSb = new System.Text.StringBuilder();
                questSb.AppendLine("Abgeschlossene Quests (Belohnung abholen):");
                foreach (var q in completed)
                    questSb.AppendLine($"  {q.Title} - {q.GoldReward}G + {q.ExpReward}EP");
                _questLabel.Text = questSb.ToString();
                _questPanel.Visible = true;
            }
            else
            {
                _questPanel.Visible = false;
            }

            foreach (Node child in _buttonList.GetChildren())
                child.QueueFree();

            AddButton("Rasten (volle Heilung)", OnRest);
            AddButton("Haendler besuchen", OnShop);
            if (completed.Count > 0)
                AddButton("Quest-Belohnungen abholen", OnClaimQuests);
            AddButton("Dungeon betreten", OnEnterDungeon);
        }

        private void AddButton(string text, System.Action action)
        {
            var btn = new Button { Text = text };
            btn.AddThemeFontSizeOverride("font_size", 16);
            btn.CustomMinimumSize = new Vector2(320, 42);
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
