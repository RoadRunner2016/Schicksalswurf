using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Settings menu with tabbed sections for audio, gameplay, and accessibility.
    /// </summary>
    public partial class SettingsUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _container;
        private TabContainer _tabs;
        private bool _isActive = false;

        private HSlider _masterVolumeSlider;
        private HSlider _sfxVolumeSlider;
        private HSlider _musicVolumeSlider;
        private OptionButton _difficultyButton;
        private CheckBox _colorblindCheckBox;
        private CheckBox _largerFontCheckBox;
        private CheckBox _autoSaveCheckBox;
        private CheckBox _combatCameraCheckBox;
        private CheckBox _showDamageNumbersCheckBox;
        private OptionButton _languageButton;

        public float MasterVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.5f;
        public int Difficulty { get; set; } = 1;
        public bool ColorblindMode { get; set; } = false;
        public bool LargerFont { get; set; } = false;
        public bool AutoSave { get; set; } = true;
        public bool CombatCamera { get; set; } = true;
        public bool ShowDamageNumbers { get; set; } = true;
        public int Language { get; set; } = 0;

        public static SettingsUI Instance { get; private set; }

        private static readonly Color PanelBg = new(0.04f, 0.04f, 0.08f, 0.98f);
        private static readonly Color BorderColor = new(0.45f, 0.35f, 0.12f, 0.9f);
        private static readonly Color HeaderColor = new(0.95f, 0.78f, 0.28f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.5f, 0.48f, 0.4f);
        private static readonly Color TabBg = new(0.08f, 0.07f, 0.05f, 0.9f);
        private static readonly Color AccentColor = new(0.6f, 0.45f, 0.15f);

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
            _mainPanel.CustomMinimumSize = new Vector2(560, 0);
            _mainPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = PanelBg,
                BorderWidthLeft = 3, BorderWidthRight = 3,
                BorderWidthTop = 3, BorderWidthBottom = 3,
                BorderColor = BorderColor,
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 24, ContentMarginRight = 24,
                ContentMarginTop = 20, ContentMarginBottom = 20
            });
            AddChild(_mainPanel);

            _container = new VBoxContainer();
            _container.AddThemeConstantOverride("separation", 14);
            _mainPanel.AddChild(_container);

            var title = new Label { Text = "Einstellungen" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 26);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _container.AddChild(title);

            _tabs = new TabContainer();
            _tabs.CustomMinimumSize = new Vector2(0, 320);
            _tabs.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = TabBg,
                BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2,
                BorderColor = AccentColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 16, ContentMarginRight = 16,
                ContentMarginTop = 12, ContentMarginBottom = 12
            });
            _container.AddChild(_tabs);

            BuildAudioTab();
            BuildGameplayTab();
            BuildAccessibilityTab();

            var buttonRow = new HBoxContainer();
            buttonRow.AddThemeConstantOverride("separation", 10);
            buttonRow.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            _container.AddChild(buttonRow);

            var saveBtn = new Button { Text = "Speichern" };
            saveBtn.AddThemeFontSizeOverride("font_size", 16);
            saveBtn.CustomMinimumSize = new Vector2(140, 40);
            saveBtn.Pressed += () => { Save(); Close(); };
            buttonRow.AddChild(saveBtn);

            var closeBtn = new Button { Text = "Schliessen" };
            closeBtn.AddThemeFontSizeOverride("font_size", 16);
            closeBtn.CustomMinimumSize = new Vector2(140, 40);
            closeBtn.Pressed += Close;
            buttonRow.AddChild(closeBtn);
        }

        private void BuildAudioTab()
        {
            var tab = new VBoxContainer();
            tab.AddThemeConstantOverride("separation", 12);

            var header = new Label { Text = "Audio" };
            header.AddThemeFontSizeOverride("font_size", 18);
            header.AddThemeColorOverride("font_color", HeaderColor);
            tab.AddChild(header);

            AddSliderRow(tab, "Master-Lautstaerke", out _masterVolumeSlider, MasterVolume, (v) => { MasterVolume = v; ApplyVolumes(); });
            AddSliderRow(tab, "Effekt-Lautstaerke", out _sfxVolumeSlider, SfxVolume, (v) => { SfxVolume = v; ApplyVolumes(); });
            AddSliderRow(tab, "Musik-Lautstaerke", out _musicVolumeSlider, MusicVolume, (v) => { MusicVolume = v; ApplyVolumes(); });

            _tabs.AddChild(tab);
            _tabs.SetTabTitle(0, "Audio");
        }

        private void BuildGameplayTab()
        {
            var tab = new VBoxContainer();
            tab.AddThemeConstantOverride("separation", 12);

            var header = new Label { Text = "Gameplay" };
            header.AddThemeFontSizeOverride("font_size", 18);
            header.AddThemeColorOverride("font_color", HeaderColor);
            tab.AddChild(header);

            var diffLabel = new Label { Text = "Schwierigkeitsgrad:" };
            diffLabel.AddThemeColorOverride("font_color", TextColor);
            diffLabel.AddThemeFontSizeOverride("font_size", 14);
            tab.AddChild(diffLabel);

            _difficultyButton = new OptionButton();
            _difficultyButton.AddItem("Leicht", 0);
            _difficultyButton.AddItem("Normal", 1);
            _difficultyButton.AddItem("Schwer", 2);
            _difficultyButton.Selected = Difficulty;
            _difficultyButton.ItemSelected += (idx) => Difficulty = (int)idx;
            tab.AddChild(_difficultyButton);

            var langLabel = new Label { Text = "Sprache:" };
            langLabel.AddThemeColorOverride("font_color", TextColor);
            langLabel.AddThemeFontSizeOverride("font_size", 14);
            tab.AddChild(langLabel);

            _languageButton = new OptionButton();
            _languageButton.AddItem("Deutsch", 0);
            _languageButton.AddItem("English", 1);
            _languageButton.Selected = Language;
            _languageButton.ItemSelected += (idx) => Language = (int)idx;
            tab.AddChild(_languageButton);

            _autoSaveCheckBox = new CheckBox { Text = "Automatisches Speichern", ButtonPressed = AutoSave };
            _autoSaveCheckBox.Toggled += (on) => AutoSave = on;
            tab.AddChild(_autoSaveCheckBox);

            _combatCameraCheckBox = new CheckBox { Text = "Kampf-Kamera verfolgen", ButtonPressed = CombatCamera };
            _combatCameraCheckBox.Toggled += (on) => CombatCamera = on;
            tab.AddChild(_combatCameraCheckBox);

            _showDamageNumbersCheckBox = new CheckBox { Text = "Schadenszahlen anzeigen", ButtonPressed = ShowDamageNumbers };
            _showDamageNumbersCheckBox.Toggled += (on) => ShowDamageNumbers = on;
            tab.AddChild(_showDamageNumbersCheckBox);

            _tabs.AddChild(tab);
            _tabs.SetTabTitle(1, "Gameplay");
        }

        private void BuildAccessibilityTab()
        {
            var tab = new VBoxContainer();
            tab.AddThemeConstantOverride("separation", 12);

            var header = new Label { Text = "Barrierefreiheit" };
            header.AddThemeFontSizeOverride("font_size", 18);
            header.AddThemeColorOverride("font_color", HeaderColor);
            tab.AddChild(header);

            _colorblindCheckBox = new CheckBox { Text = "Farbenblind-Modus" };
            _colorblindCheckBox.ButtonPressed = ColorblindMode;
            _colorblindCheckBox.Toggled += (on) => ColorblindMode = on;
            tab.AddChild(_colorblindCheckBox);

            _largerFontCheckBox = new CheckBox { Text = "Grosse Schrift" };
            _largerFontCheckBox.ButtonPressed = LargerFont;
            _largerFontCheckBox.Toggled += (on) => LargerFont = on;
            tab.AddChild(_largerFontCheckBox);

            var infoLabel = new Label { Text = "Der Farbenblind-Modus passt\nFarben in Kampf-UI und Minimap an." };
            infoLabel.AddThemeColorOverride("font_color", DimColor);
            infoLabel.AddThemeFontSizeOverride("font_size", 12);
            tab.AddChild(infoLabel);

            _tabs.AddChild(tab);
            _tabs.SetTabTitle(2, "Barrierefreiheit");
        }

        private void AddSliderRow(Node parent, string label, out HSlider slider, float value, System.Action<float> onChanged)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);

            var lbl = new Label { Text = label, CustomMinimumSize = new Vector2(180, 0) };
            lbl.AddThemeColorOverride("font_color", TextColor);
            lbl.AddThemeFontSizeOverride("font_size", 14);
            row.AddChild(lbl);

            slider = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.05f, Value = value };
            slider.CustomMinimumSize = new Vector2(220, 0);
            slider.ValueChanged += (v) => onChanged((float)v);
            row.AddChild(slider);

            var pctLabel = new Label { Text = $"{(int)(value * 100)}%", CustomMinimumSize = new Vector2(40, 0) };
            pctLabel.AddThemeColorOverride("font_color", DimColor);
            pctLabel.AddThemeFontSizeOverride("font_size", 13);
            slider.ValueChanged += (v) => pctLabel.Text = $"{(int)((float)v * 100)}%";
            row.AddChild(pctLabel);

            parent.AddChild(row);
        }

        private void ApplyVolumes()
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb(MasterVolume));
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

        public bool IsActive => Visible;

        public void Save()
        {
            var config = new ConfigFile();
            config.SetValue("audio", "master", MasterVolume);
            config.SetValue("audio", "sfx", SfxVolume);
            config.SetValue("audio", "music", MusicVolume);
            config.SetValue("game", "difficulty", Difficulty);
            config.SetValue("game", "language", Language);
            config.SetValue("game", "autosave", AutoSave);
            config.SetValue("game", "combat_camera", CombatCamera);
            config.SetValue("game", "damage_numbers", ShowDamageNumbers);
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
                Language = (int)config.GetValue("game", "language", 0);
                AutoSave = (bool)config.GetValue("game", "autosave", true);
                CombatCamera = (bool)config.GetValue("game", "combat_camera", true);
                ShowDamageNumbers = (bool)config.GetValue("game", "damage_numbers", true);
                ColorblindMode = (bool)config.GetValue("access", "colorblind", false);
                LargerFont = (bool)config.GetValue("access", "larger_font", false);
            }
        }
    }
}
