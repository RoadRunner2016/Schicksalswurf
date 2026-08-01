using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Tutorial/help overlay showing controls and mechanics.
    /// </summary>
    public partial class HelpUI : Control
    {
        private PanelContainer _mainPanel;
        private bool _isActive = false;

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.95f);
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
                ContentMarginLeft = 40, ContentMarginRight = 40,
                ContentMarginTop = 30, ContentMarginBottom = 30
            });
            AddChild(_mainPanel);

            var scroll = new ScrollContainer();
            scroll.CustomMinimumSize = new Vector2(0, 600);
            _mainPanel.AddChild(scroll);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 8);
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            scroll.AddChild(container);

            var title = new Label { Text = "📖 HILFE & STEUERUNG 📖" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 24);
            title.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(title);

            AddSection(container, "Bewegung",
                "W / S    - Vorwaerts / Rueckwaerts bewegen\n" +
                "A / D    - Nach links / rechts drehen\n" +
                "Q / E    - Seitwaerts (strafe) links / rechts\n" +
                "F        - Mit Objekten interagieren (Tueren, Truhen)");

            AddSection(container, "UI",
                "C        - Charakterbogen oeffnen/schliessen\n" +
                "I        - Inventar oeffnen/schliessen\n" +
                "R        - Rasten (Heilung)\n" +
                "H        - Hilfe anzeigen/verbergen\n" +
                "Tab      - Zwischen Charakteren wechseln (in UIs)");

            AddSection(container, "Kampf",
                "F        - Angreifen\n" +
                "E        - Zauber wirken (Auswahl oeffnet sich)\n" +
                "A        - Verteidigen\n" +
                "S        - Fliehen\n" +
                "1-4      - Gegner auswaehlen\n" +
                "Im Zauber-Menue: 1-9 = Zauber waehlen, F = wirken, Esc = abbrechen");

            AddSection(container, "Charakterentwicklung",
                "EP sammeln durch Kmpfe und Quests.\n" +
                "Bei Level-Aufstieg: +2 Attributspunkte, +3 Talentpunkte.\n" +
                "Punkte im Charakterbogen (C) mit + Buttons verteilen.\n" +
                "Attribute: Kraft, Gewandtheit, Konstitution, Intelligenz, Willenskraft, Wahrnehmung, Charisma");

            AddSection(container, "Magie",
                "Zauber kosten Mana und erfordern eine 3W20-Probe.\n" +
                "Magier lernen Feuerball, Arkaner Blitz, Heilung.\n" +
                "Heiler lernen Heilung, Schild, Staerkung.\n" +
                "Alle anderen lernen Basis-Heilung.\n" +
                "Neue Zauber koennen durch Level-Up gelernt werden.");

            AddSection(container, "Dungeon",
                "Treppen nach unten (gruen) fuehren in tiefere Ebenen.\n" +
                "Treppen nach oben (blau) fuehren zurueck.\n" +
                "Truhen enthalten Gold, Traenke und Items.\n" +
                "Fallen koennen mit Wahrnehmung erkannt werden.\n" +
                "Dietriche (Schurke) koennen Fallen entschaerfen.\n" +
                "Haendler und Heiler koennen im Dungeon angetroffen werden.");

            AddSection(container, "Tipps",
                "Raste (R) regelmaessig, um HP/Mana/Ausdauer aufzufuellen.\n" +
                "Behalte immer Heiltraenke im Inventar.\n" +
                "Tiefere Ebenen haben staerkere Gegner, aber bessere Beute.\n" +
                "Boss-Gegner warten auf Ebene 3 und tiefer.");

            var hint = new Label { Text = "H: Hilfe schliessen" };
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            hint.AddThemeColorOverride("font_color", DimColor);
            hint.AddThemeFontSizeOverride("font_size", 13);
            container.AddChild(hint);
        }

        private void AddSection(Node parent, string title, string content)
        {
            var titleLabel = new Label { Text = title };
            titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            parent.AddChild(titleLabel);

            var contentLabel = new Label { Text = content };
            contentLabel.AddThemeColorOverride("font_color", TextColor);
            contentLabel.AddThemeFontSizeOverride("font_size", 13);
            parent.AddChild(contentLabel);
        }

        public void Toggle()
        {
            _isActive = !_isActive;
            Visible = _isActive;
        }

        public bool IsActive => _isActive;
    }
}
