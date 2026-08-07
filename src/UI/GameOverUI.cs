using Godot;

namespace Schicksalswurf.UI
{
    using Characters;
    using Core;

    /// <summary>
    /// Death/GameOver screen with statistics and restart options.
    /// </summary>
    public partial class GameOverUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _container;
        private bool _isActive = false;

        public bool RestartRequested { get; set; } = false;
        public bool MainMenuRequested { get; set; } = false;

        private static readonly Color PanelBg = new(0.06f, 0.02f, 0.02f, 0.98f);
        private static readonly Color BorderColor = new(0.6f, 0.1f, 0.1f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.2f, 0.15f);
        private static readonly Color TextColor = new(0.85f, 0.75f, 0.7f);
        private static readonly Color DimColor = new(0.5f, 0.4f, 0.38f);
        private static readonly Color AccentColor = new(0.5f, 0.15f, 0.1f);
        private static readonly Color StatBg = new(0.1f, 0.05f, 0.04f, 0.85f);

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

            _container = new VBoxContainer();
            _container.AddThemeConstantOverride("separation", 15);
            _mainPanel.AddChild(_container);

            var title = new Label { Text = "GAME OVER" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 48);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _container.AddChild(title);

            var decorLine = new HSeparator();
            decorLine.AddThemeStyleboxOverride("separator", new StyleBoxFlat
            {
                BgColor = AccentColor,
                ContentMarginTop = 2, ContentMarginBottom = 6
            });
            _container.AddChild(decorLine);

            var subtitle = new Label { Text = "Eure Gruppe ist gefallen..." };
            subtitle.HorizontalAlignment = HorizontalAlignment.Center;
            subtitle.AddThemeFontSizeOverride("font_size", 18);
            subtitle.AddThemeColorOverride("font_color", TextColor);
            _container.AddChild(subtitle);

            // Stats panel
            var statsPanel = new PanelContainer();
            statsPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = StatBg,
                BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2,
                BorderColor = AccentColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 20, ContentMarginRight = 20,
                ContentMarginTop = 14, ContentMarginBottom = 14
            });
            _container.AddChild(statsPanel);

            var statsLabel = new Label();
            statsLabel.Name = "StatsLabel";
            statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statsLabel.AddThemeFontSizeOverride("font_size", 15);
            statsLabel.AddThemeColorOverride("font_color", TextColor);
            statsPanel.AddChild(statsLabel);

            // Spacer
            _container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

            // Buttons
            var restartBtn = new Button { Text = "Neues Spiel" };
            restartBtn.AddThemeFontSizeOverride("font_size", 18);
            restartBtn.CustomMinimumSize = new Vector2(300, 50);
            restartBtn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            restartBtn.Pressed += () => { RestartRequested = true; Close(); };
            _container.AddChild(restartBtn);

            var menuBtn = new Button { Text = "Hauptmenue" };
            menuBtn.AddThemeFontSizeOverride("font_size", 18);
            menuBtn.CustomMinimumSize = new Vector2(300, 50);
            menuBtn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            menuBtn.Pressed += () => { MainMenuRequested = true; Close(); };
            _container.AddChild(menuBtn);

            var quitBtn = new Button { Text = "Beenden" };
            quitBtn.AddThemeFontSizeOverride("font_size", 18);
            quitBtn.CustomMinimumSize = new Vector2(300, 50);
            quitBtn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            quitBtn.Pressed += () => GetTree().Quit();
            _container.AddChild(quitBtn);
        }

        public void ShowGameOver(Party party, int dungeonLevel, GameStats stats)
        {
            _isActive = true;
            Visible = true;
            RestartRequested = false;
            MainMenuRequested = false;

            var statsLabel = _container.GetNode<Label>("StatsLabel");
            statsLabel.Text = $"Erreichte Ebene:  {dungeonLevel}\n" +
                              $"Besiegte Gegner:  {stats.EnemiesKilled}\n" +
                              $"Gefundene Truhen: {stats.ChestsFound}\n" +
                              $"Ausgegebenes Gold: {stats.GoldSpent}\n" +
                              $"Gespielte Zeit:   {stats.PlayTimeStr}";
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => Visible;
    }
}
