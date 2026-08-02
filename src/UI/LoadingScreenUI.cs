using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Loading screen shown during level transitions with animated progress bar.
    /// </summary>
    public partial class LoadingScreenUI : Control
    {
        private PanelContainer _mainPanel;
        private Label _titleLabel;
        private Label _tipLabel;
        private ProgressBar _progressBar;
        private Label _progressLabel;
        private bool _isActive = false;

        private static readonly string[] Tips = {
            "Tipp: Raste regelmaessig, um dich zu heilen.",
            "Tipp: Fallen koennen mit Wahrnehmung erkannt werden.",
            "Tipp: Dietriche werden beim Entschärfen verbraucht.",
            "Tipp: Zauber kosten Mana - spare es fuer Kaempfe.",
            "Tipp: Tiefere Ebenen haben staerkere Gegner, aber bessere Beute.",
            "Tipp: Boss-Gegner erscheinen alle 3 Ebenen.",
            "Tipp: Haendler im Dungeon verkaufen nuetzliche Items.",
            "Tipp: Heiler koennen deine Gruppe fuer Gold heilen.",
            "Tipp: Attribute beeinflussen Kampfwerte direkt.",
            "Tipp: F5 speichert das Spiel, F9 laedt es.",
            "Tipp: H oeffnet die Hilfe mit allen Steuerungen.",
            "Tipp: Quests geben Gold und Erfahrung als Belohnung.",
            "Tipp: Kritische Treffer verdoppeln den Schaden!",
            "Tipp: Soak reduziert eingehenden Schaden - trage Ruestung!",
            "Tipp: Ab Ebene 15 warten neue Dungeon-Themes auf dich.",
            "Tipp: Im Kampf kannst du dich mit WASD auf dem Grid bewegen.",
            "Tipp: Schilde erhoehen die Verteidigung deiner Helden.",
            "Tipp: Vergiftete Gegner nehmen jeden Tick Schaden.",
            "Tipp: Hochstufige Zauber wie Apokalypse sind sehr maechtig.",
            "Tipp: Crafting-Zutaten koennen zu nuetzlichen Items verarbeitet werden."
        };

        private static readonly Color PanelBg = new(0.02f, 0.02f, 0.06f, 0.97f);
        private static readonly Color HeaderColor = new(0.95f, 0.78f, 0.28f);
        private static readonly Color TextColor = new(0.7f, 0.68f, 0.6f);
        private static readonly Color DimColor = new(0.45f, 0.43f, 0.38f);
        private static readonly Color AccentColor = new(0.6f, 0.45f, 0.15f);

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
                BorderColor = AccentColor,
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
            });
            AddChild(_mainPanel);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 20);
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            container.SizeFlagsVertical = SizeFlags.ExpandFill;
            _mainPanel.AddChild(container);

            container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 150) });

            _titleLabel = new Label { Text = "Lade..." };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 32);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(_titleLabel);

            var decorLine = new HSeparator();
            decorLine.CustomMinimumSize = new Vector2(300, 2);
            decorLine.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            decorLine.AddThemeStyleboxOverride("separator", new StyleBoxFlat
            {
                BgColor = AccentColor,
                ContentMarginTop = 2, ContentMarginBottom = 4
            });
            container.AddChild(decorLine);

            _progressBar = new ProgressBar();
            _progressBar.CustomMinimumSize = new Vector2(420, 24);
            _progressBar.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _progressBar.MinValue = 0;
            _progressBar.MaxValue = 100;
            _progressBar.Value = 0;
            _progressBar.ShowPercentage = false;
            container.AddChild(_progressBar);

            _progressLabel = new Label { Text = "0%" };
            _progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _progressLabel.AddThemeFontSizeOverride("font_size", 14);
            _progressLabel.AddThemeColorOverride("font_color", DimColor);
            container.AddChild(_progressLabel);

            container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

            _tipLabel = new Label();
            _tipLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tipLabel.AddThemeFontSizeOverride("font_size", 15);
            _tipLabel.AddThemeColorOverride("font_color", TextColor);
            _tipLabel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _tipLabel.CustomMinimumSize = new Vector2(600, 40);
            _tipLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            container.AddChild(_tipLabel);
        }

        public void Show(string levelName)
        {
            _isActive = true;
            Visible = true;
            _titleLabel.Text = levelName;
            _progressBar.Value = 0;
            _progressLabel.Text = "0%";

            var rng = new RandomNumberGenerator();
            rng.Randomize();
            _tipLabel.Text = Tips[rng.RandiRange(0, Tips.Length - 1)];
        }

        public void SetProgress(float pct)
        {
            _progressBar.Value = pct;
            _progressLabel.Text = $"{(int)pct}%";
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;
    }
}
