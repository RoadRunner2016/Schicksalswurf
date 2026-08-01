using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.UI
{
    using Core;

    /// <summary>
    /// Achievements UI showing unlocked and locked achievements.
    /// </summary>
    public partial class AchievementsUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _container;
        private ScrollContainer _scroll;
        private bool _isActive = false;

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.98f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.5f, 0.48f, 0.42f);
        private static readonly Color GoldColor = new(1.0f, 0.85f, 0.2f);

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
                ContentMarginTop = 30, ContentMarginBottom = 30
            });
            AddChild(_mainPanel);

            _container = new VBoxContainer();
            _container.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_container);

            var title = new Label { Text = "🏆 ERFOLGE 🏆" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 28);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _container.AddChild(title);

            _scroll = new ScrollContainer();
            _scroll.CustomMinimumSize = new Vector2(0, 400);
            _scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
            _container.AddChild(_scroll);

            var list = new VBoxContainer { Name = "AchievementList" };
            list.AddThemeConstantOverride("separation", 6);
            _scroll.AddChild(list);

            var closeBtn = new Button { Text = "Schliessen (Esc)" };
            closeBtn.CustomMinimumSize = new Vector2(0, 40);
            closeBtn.Pressed += Close;
            _container.AddChild(closeBtn);
        }

        public void Show(GameStats stats)
        {
            _isActive = true;
            Visible = true;
            RefreshUI(stats);
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;

        private void RefreshUI(GameStats stats)
        {
            var list = _scroll.GetNode<VBoxContainer>("AchievementList");
            foreach (Node child in list.GetChildren())
                child.QueueFree();

            foreach (var ach in stats.Achievements)
            {
                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 10);

                var icon = new Label { Text = ach.Unlocked ? "✓" : "?" };
                icon.AddThemeFontSizeOverride("font_size", 20);
                icon.AddThemeColorOverride("font_color", ach.Unlocked ? GoldColor : DimColor);
                icon.CustomMinimumSize = new Vector2(30, 0);
                row.AddChild(icon);

                var info = new VBoxContainer();
                info.SizeFlagsHorizontal = SizeFlags.ExpandFill;

                var name = new Label { Text = ach.Unlocked ? ach.Title : "???" };
                name.AddThemeFontSizeOverride("font_size", 16);
                name.AddThemeColorOverride("font_color", ach.Unlocked ? TextColor : DimColor);
                info.AddChild(name);

                var desc = new Label { Text = ach.Unlocked ? ach.Description : "Noch nicht freigeschaltet" };
                desc.AddThemeFontSizeOverride("font_size", 13);
                desc.AddThemeColorOverride("font_color", DimColor);
                info.AddChild(desc);

                row.AddChild(info);

                var reward = new Label { Text = $"+{ach.ExpReward} EP" };
                reward.AddThemeFontSizeOverride("font_size", 14);
                reward.AddThemeColorOverride("font_color", ach.Unlocked ? GoldColor : DimColor);
                row.AddChild(reward);

                list.AddChild(row);
            }
        }
    }
}
