using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Settings menu for volume, difficulty, and key bindings.
    /// </summary>
    public partial class SettingsUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _container;
        private bool _isActive = false;

        private HSlider _masterVolumeSlider;
        private HSlider _sfxVolumeSlider;
        private HSlider _musicVolumeSlider;
        private OptionButton _difficultyButton;
        private CheckBox _colorblindCheckBox;
        private CheckBox _largerFontCheckBox;

        public float MasterVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.5f;
        public int Difficulty { get; set; } = 1; // 0=easy, 1=normal, 2=hard
        public bool ColorblindMode { get; set; } = false;
        public bool LargerFont { get; set; } = false;

        public static SettingsUI Instance { get; private set; }

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.98f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);

        public override void _Ready()
        {
            Instance = this;
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;
            Visible = false;
            BuildUI();
        }

        private void BuildUI()
        {
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(500, 0);
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

            _container = new VBoxContainer();
            _container.AddThemeConstantOverride("separation", 12);
            _mainPanel.AddChild(_container);

            var title = new Label { Text = "⚙ EINSTELLUNGEN" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 24);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _container.AddChild(title);

            // Master volume
            AddSliderRow("Master-Lautstaerke", out _masterVolumeSlider, MasterVolume, (v) => { MasterVolume = v; ApplyVolumes(); });
            AddSliderRow("Effekt-Lautstaerke", out _sfxVolumeSlider, SfxVolume, (v) => { SfxVolume = v; ApplyVolumes(); });
            AddSliderRow("Musik-Lautstaerke", out _musicVolumeSlider, MusicVolume, (v) => { MusicVolume = v; ApplyVolumes(); });

            // Difficulty
            var diffLabel = new Label { Text = "Schwierigkeitsgrad:" };
            diffLabel.AddThemeColorOverride("font_color", TextColor);
            diffLabel.AddThemeFontSizeOverride("font_size", 14);
            _container.AddChild(diffLabel);

            _difficultyButton = new OptionButton();
            _difficultyButton.AddItem("Leicht", 0);
            _difficultyButton.AddItem("Normal", 1);
            _difficultyButton.AddItem("Schwer", 2);
            _difficultyButton.Selected = Difficulty;
            _difficultyButton.ItemSelected += (idx) => Difficulty = (int)idx;
            _container.AddChild(_difficultyButton);

            // Accessibility
            _colorblindCheckBox = new CheckBox { Text = "Farbenblind-Modus" };
            _colorblindCheckBox.ButtonPressed = ColorblindMode;
            _colorblindCheckBox.Toggled += (on) => ColorblindMode = on;
            _container.AddChild(_colorblindCheckBox);

            _largerFontCheckBox = new CheckBox { Text = "Grosse Schrift" };
            _largerFontCheckBox.ButtonPressed = LargerFont;
            _largerFontCheckBox.Toggled += (on) => LargerFont = on;
            _container.AddChild(_largerFontCheckBox);

            // Close button
            var closeBtn = new Button { Text = "Schliessen" };
            closeBtn.CustomMinimumSize = new Vector2(0, 40);
            closeBtn.Pressed += Close;
            _container.AddChild(closeBtn);
        }

        private void AddSliderRow(string label, out HSlider slider, float value, System.Action<float> onChanged)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);

            var lbl = new Label { Text = label, CustomMinimumSize = new Vector2(180, 0) };
            lbl.AddThemeColorOverride("font_color", TextColor);
            lbl.AddThemeFontSizeOverride("font_size", 14);
            row.AddChild(lbl);

            slider = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.05f, Value = value };
            slider.CustomMinimumSize = new Vector2(200, 0);
            slider.ValueChanged += (v) => onChanged((float)v);
            row.AddChild(slider);

            _container.AddChild(row);
        }

        private void ApplyVolumes()
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb(MasterVolume));
            // SFX and Music buses would be set here if they exist
        }

        public void Toggle()
        {
            _isActive = !_isActive;
            Visible = _isActive;
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;

        public void Save()
        {
            var config = new ConfigFile();
            config.SetValue("audio", "master", MasterVolume);
            config.SetValue("audio", "sfx", SfxVolume);
            config.SetValue("audio", "music", MusicVolume);
            config.SetValue("game", "difficulty", Difficulty);
            config.SetValue("access", "colorblind", ColorblindMode);
            config.SetValue("access", "larger_font", LargerFont);
            config.Save("user://settings.cfg");
        }

        public void Load()
        {
            var config = new ConfigFile();
            if (config.Load("user://settings.cfg") == Error.Ok)
            {
                MasterVolume = (float)config.GetValue("audio", "master", 1.0f);
                SfxVolume = (float)config.GetValue("audio", "sfx", 0.8f);
                MusicVolume = (float)config.GetValue("audio", "music", 0.5f);
                Difficulty = (int)config.GetValue("game", "difficulty", 1);
                ColorblindMode = (bool)config.GetValue("access", "colorblind", false);
                LargerFont = (bool)config.GetValue("access", "larger_font", false);
            }
        }
    }
}
