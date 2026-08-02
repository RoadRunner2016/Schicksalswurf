using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Main menu with animated title, start, load, settings, and quit options.
    /// </summary>
    public partial class MainMenuUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        private Button _newGameButton;
        private Button _loadButton;
        private Button _settingsButton;
        private Button _quitButton;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _versionLabel;
        private SettingsUI _settingsUI;

        public bool StartRequested { get; private set; } = false;
        public bool LoadRequested { get; private set; } = false;

        private static readonly Color PanelBg = new(0.03f, 0.03f, 0.06f, 0.98f);
        private static readonly Color BorderColor = new(0.45f, 0.35f, 0.12f, 0.9f);
        private static readonly Color HeaderColor = new(0.95f, 0.78f, 0.28f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.5f, 0.48f, 0.4f);
        private static readonly Color AccentColor = new(0.6f, 0.45f, 0.15f);
        private static readonly Color ButtonBg = new(0.08f, 0.07f, 0.05f, 0.9f);
        private static readonly Color ButtonHover = new(0.15f, 0.12f, 0.06f, 0.95f);

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            BuildUI();
        }

        private StyleBoxFlat CreateButtonStyle(Color bg)
        {
            return new StyleBoxFlat
            {
                BgColor = bg,
                BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2,
                BorderColor = AccentColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 16, ContentMarginRight = 16,
                ContentMarginTop = 8, ContentMarginBottom = 8
            };
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
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
            });
            AddChild(_mainPanel);

            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 16);
            _mainContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
            _mainPanel.AddChild(_mainContainer);

            // Top spacer
            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 60) });

            // Title with glow effect
            var titleContainer = new VBoxContainer();
            titleContainer.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _mainContainer.AddChild(titleContainer);

            _titleLabel = new Label { Text = "SCHICKSALSWURF" };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 48);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            titleContainer.AddChild(_titleLabel);

            // Decorative line under title
            var decorLine = new HSeparator();
            decorLine.CustomMinimumSize = new Vector2(400, 2);
            decorLine.AddThemeStyleboxOverride("separator", new StyleBoxFlat
            {
                BgColor = AccentColor,
                ContentMarginTop = 2, ContentMarginBottom = 2
            });
            titleContainer.AddChild(decorLine);

            _subtitleLabel = new Label { Text = "Ein Dungeon-Crawler im Geiste Das Schwarze Auge" };
            _subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _subtitleLabel.AddThemeFontSizeOverride("font_size", 15);
            _subtitleLabel.AddThemeColorOverride("font_color", DimColor);
            _mainContainer.AddChild(_subtitleLabel);

            // Spacer
            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 30) });

            // Buttons with styled appearance
            _newGameButton = CreateMenuButton("⚔  Neues Spiel", true);
            _newGameButton.Pressed += OnNewGame;
            _mainContainer.AddChild(_newGameButton);

            _loadButton = CreateMenuButton("📜  Spiel laden", false);
            _loadButton.Pressed += OnLoad;
            _mainContainer.AddChild(_loadButton);

            _settingsButton = CreateMenuButton("⚙  Einstellungen", false);
            _settingsButton.Pressed += OnSettings;
            _mainContainer.AddChild(_settingsButton);

            _quitButton = CreateMenuButton("✕  Beenden", false);
            _quitButton.Pressed += OnQuit;
            _mainContainer.AddChild(_quitButton);

            // Bottom spacer
            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

            // Version label
            _versionLabel = new Label { Text = "v1.0  |  Godot 4.7  |  .NET 10" };
            _versionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _versionLabel.AddThemeFontSizeOverride("font_size", 12);
            _versionLabel.AddThemeColorOverride("font_color", DimColor);
            _versionLabel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _mainContainer.AddChild(_versionLabel);

            // Settings UI (hidden by default)
            _settingsUI = new SettingsUI();
            AddChild(_settingsUI);
        }

        private Button CreateMenuButton(string text, bool primary)
        {
            var btn = new Button { Text = text };
            btn.AddThemeFontSizeOverride("font_size", 20);
            btn.CustomMinimumSize = new Vector2(340, 52);
            btn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;

            var normalStyle = CreateButtonStyle(primary ? ButtonHover : ButtonBg);
            var hoverStyle = CreateButtonStyle(new Color(0.2f, 0.15f, 0.08f, 0.95f));

            btn.AddThemeStyleboxOverride("normal", normalStyle);
            btn.AddThemeStyleboxOverride("hover", hoverStyle);
            btn.AddThemeStyleboxOverride("pressed", hoverStyle);
            btn.AddThemeColorOverride("font_color", primary ? HeaderColor : TextColor);
            btn.AddThemeColorOverride("font_hover_color", HeaderColor);

            return btn;
        }

        private void OnNewGame()
        {
            StartRequested = true;
            Visible = false;
        }

        private void OnLoad()
        {
            LoadRequested = true;
        }

        private void OnSettings()
        {
            _settingsUI?.Toggle();
        }

        private void OnQuit()
        {
            GetTree().Quit();
        }
    }
}
