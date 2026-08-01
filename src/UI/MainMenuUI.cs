using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Main menu with start, load, and quit options.
    /// </summary>
    public partial class MainMenuUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        private Button _newGameButton;
        private Button _loadButton;
        private Button _quitButton;
        private Label _titleLabel;
        private Label _subtitleLabel;

        public bool StartRequested { get; private set; } = false;
        public bool LoadRequested { get; private set; } = false;

        private static readonly Color PanelBg = new(0.04f, 0.04f, 0.08f, 0.98f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.55f, 0.52f, 0.45f);

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
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
                BorderColor = BorderColor
            });
            AddChild(_mainPanel);

            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 20);
            _mainContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
            _mainPanel.AddChild(_mainContainer);

            // Spacer
            var spacer = new Control { CustomMinimumSize = new Vector2(0, 80) };
            _mainContainer.AddChild(spacer);

            // Title
            _titleLabel = new Label { Text = "SCHICKSALSWURF" };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 42);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            _mainContainer.AddChild(_titleLabel);

            _subtitleLabel = new Label { Text = "Ein Dungeon-Crawler im Geiste Das Schwarze Auge" };
            _subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _subtitleLabel.AddThemeFontSizeOverride("font_size", 16);
            _subtitleLabel.AddThemeColorOverride("font_color", DimColor);
            _mainContainer.AddChild(_subtitleLabel);

            // Spacer
            var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 40) };
            _mainContainer.AddChild(spacer2);

            // Buttons
            _newGameButton = new Button { Text = "Neues Spiel" };
            _newGameButton.AddThemeFontSizeOverride("font_size", 20);
            _newGameButton.CustomMinimumSize = new Vector2(300, 50);
            _newGameButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _newGameButton.Pressed += OnNewGame;
            _mainContainer.AddChild(_newGameButton);

            _loadButton = new Button { Text = "Spiel laden" };
            _loadButton.AddThemeFontSizeOverride("font_size", 20);
            _loadButton.CustomMinimumSize = new Vector2(300, 50);
            _loadButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _loadButton.Pressed += OnLoad;
            _mainContainer.AddChild(_loadButton);

            _quitButton = new Button { Text = "Beenden" };
            _quitButton.AddThemeFontSizeOverride("font_size", 20);
            _quitButton.CustomMinimumSize = new Vector2(300, 50);
            _quitButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _quitButton.Pressed += OnQuit;
            _mainContainer.AddChild(_quitButton);
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

        private void OnQuit()
        {
            GetTree().Quit();
        }
    }
}
