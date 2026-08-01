using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Loading screen shown during level transitions.
    /// </summary>
    public partial class LoadingScreenUI : Control
    {
        private PanelContainer _mainPanel;
        private Label _titleLabel;
        private Label _tipLabel;
        private ProgressBar _progressBar;
        private bool _isActive = false;

        private static readonly string[] Tips = {
            "Tipp: Raste regelmaessig, um dich zu heilen.",
            "Tipp: Fallen koennen mit Wahrnehmung erkannt werden.",
            "Tipp: Dietriche werden beim Entschärfen verbraucht.",
            "Tipp: Zauber kosten Mana - spare es fuer kaempfe.",
            "Tipp: Tiefere Ebenen haben staerkere Gegner, aber bessere Beute.",
            "Tipp: Boss-Gegner erscheinen alle 3 Ebenen.",
            "Tipp: Haendler im Dungeon verkaufen nuetzliche Items.",
            "Tipp: Heiler koennen deine Gruppe fuer Gold heilen.",
            "Tipp: Attribute beeinflussen Kampfwerte direkt.",
            "Tipp: F5 speichert das Spiel, F9 laedt es.",
            "Tipp: H oeffnet die Hilfe mit allen Steuerungen.",
            "Tipp: Quests geben Gold und Erfahrung als Belohnung."
        };

        private static readonly Color PanelBg = new(0.02f, 0.02f, 0.06f, 0.95f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.7f, 0.68f, 0.6f);
        private static readonly Color DimColor = new(0.45f, 0.43f, 0.38f);

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
                BgColor = PanelBg
            });
            AddChild(_mainPanel);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 20);
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            container.SizeFlagsVertical = SizeFlags.ExpandFill;
            _mainPanel.AddChild(container);

            // Spacer
            container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 150) });

            _titleLabel = new Label { Text = "Lade..." };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(_titleLabel);

            _progressBar = new ProgressBar();
            _progressBar.CustomMinimumSize = new Vector2(400, 20);
            _progressBar.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _progressBar.MinValue = 0;
            _progressBar.MaxValue = 100;
            _progressBar.Value = 0;
            container.AddChild(_progressBar);

            _tipLabel = new Label();
            _tipLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tipLabel.AddThemeFontSizeOverride("font_size", 15);
            _tipLabel.AddThemeColorOverride("font_color", TextColor);
            _tipLabel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _tipLabel.CustomMinimumSize = new Vector2(600, 40);
            _tipLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _tipLabel.HorizontalAlignment = HorizontalAlignment.Center;
            container.AddChild(_tipLabel);
        }

        public void Show(string levelName)
        {
            _isActive = true;
            Visible = true;
            _titleLabel.Text = levelName;
            _progressBar.Value = 0;

            var rng = new RandomNumberGenerator();
            rng.Randomize();
            _tipLabel.Text = Tips[rng.RandiRange(0, Tips.Length - 1)];
        }

        public void SetProgress(float pct)
        {
            _progressBar.Value = pct;
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;
    }
}
