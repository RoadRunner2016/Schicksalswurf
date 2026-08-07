using Godot;

namespace Schicksalswurf.UI
{
    /// <summary>
    /// Tutorial/help overlay showing controls and mechanics with styled sections.
    /// </summary>
    public partial class HelpUI : Control
    {
        private PanelContainer _mainPanel;
        private bool _isActive = false;

        private static readonly Color PanelBg = new(0.04f, 0.04f, 0.08f, 0.97f);
        private static readonly Color BorderColor = new(0.45f, 0.35f, 0.12f, 0.9f);
        private static readonly Color HeaderColor = new(0.95f, 0.78f, 0.28f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.5f, 0.48f, 0.4f);
        private static readonly Color SectionBg = new(0.08f, 0.07f, 0.05f, 0.85f);
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
                BorderColor = BorderColor,
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ContentMarginLeft = 40, ContentMarginRight = 40,
                ContentMarginTop = 30, ContentMarginBottom = 30
            });
            AddChild(_mainPanel);

            var scroll = new ScrollContainer();
            scroll.CustomMinimumSize = new Vector2(0, 600);
            _mainPanel.AddChild(scroll);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 10);
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            scroll.AddChild(container);

            var title = new Label { Text = "HILFE & STEUERUNG" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 28);
            title.AddThemeColorOverride("font_color", HeaderColor);
            container.AddChild(title);

            var decorLine = new HSeparator();
            decorLine.AddThemeStyleboxOverride("separator", new StyleBoxFlat
            {
                BgColor = AccentColor,
                ContentMarginTop = 2, ContentMarginBottom = 4
            });
            container.AddChild(decorLine);

            AddSection(container, "Bewegung (Dungeon)",
                "W / S     - Vorwaerts / Rueckwaerts bewegen\n" +
                "A / D     - Nach links / rechts drehen\n" +
                "Q / E     - Seitwaerts (strafe) links / rechts\n" +
                "F         - Mit Objekten interagieren (Tueren, Truhen)\n" +
                "R         - Rasten (Heilung)");

            AddSection(container, "UI & Menues",
                "C         - Charakterbogen oeffnen/schliessen\n" +
                "I         - Inventar oeffnen/schliessen\n" +
                "H         - Hilfe anzeigen/verbergen\n" +
                "Esc       - Zurueck / Menue schliessen\n" +
                "Tab       - Zwischen Charakteren wechseln\n" +
                "F5        - Spiel speichern\n" +
                "F9        - Spiel laden");

            AddSection(container, "Kampf",
                "F         - Angreifen\n" +
                "G         - Zauber wirken (Auswahl oeffnet sich)\n" +
                "V         - Verteidigen\n" +
                "B         - Fliehen\n" +
                "1-4       - Gegner auswaehlen\n" +
                "WASD/Pfeile- Auf dem Kampfgrid bewegen\n" +
                "Im Zauber-Menue: 1-9 = Zauber waehlen, F = wirken, Esc = abbrechen");

            AddSection(container, "Charakterentwicklung",
                "EP sammeln durch Kaempfe und Quests.\n" +
                "Bei Level-Aufstieg: +2 Attributspunkte, +3 Talentpunkte.\n" +
                "Punkte im Charakterbogen (C) mit + Buttons verteilen.\n" +
                "Attribute: Kraft, Gewandtheit, Konstitution, Intelligenz,\n" +
                "Willenskraft, Wahrnehmung, Charisma");

            AddSection(container, "Magie",
                "Zauber kosten Mana und erfordern eine 3W20-Probe.\n" +
                "Magier lernen Feuerball, Arkaner Blitz, Heilung und mehr.\n" +
                "Heiler lernen Heilung, Schild, Staerkung.\n" +
                "Neue Zauber koennen durch Level-Up gelernt werden.\n" +
                "Hochstufige Zauber: Feuersturm, Apokalypse, Gottesschild.");

            AddSection(container, "Dungeon",
                "Treppen nach unten (gruen) fuehren in tiefere Ebenen.\n" +
                "Treppen nach oben (blau) fuehren zurueck.\n" +
                "Truhen enthalten Gold, Traenke und Items.\n" +
                "Fallen koennen mit Wahrnehmung erkannt werden.\n" +
                "Dietriche (Schurke) koennen Fallen entschaerfen.\n" +
                "Haendler und Heiler koennen im Dungeon angetroffen werden.\n" +
                "Neue Themes ab Ebene 15: Hoellenfestung, Eisgruft, Tempel.");

            AddSection(container, "Tipps",
                "Raste (R) regelmaessig, um HP/Mana/Ausdauer aufzufuellen.\n" +
                "Behalte immer Heiltraenke im Inventar.\n" +
                "Tiefere Ebenen haben staerkere Gegner, aber bessere Beute.\n" +
                "Boss-Gegner warten auf Ebene 3 und tiefer.\n" +
                "Gegner skalieren mit Dungeon-Tiefe - sei vorsichtig!\n" +
                "Quests geben Gold und Erfahrung als Belohnung.");

            container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

            var hint = new Label { Text = "H: Hilfe schliessen" };
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            hint.AddThemeColorOverride("font_color", DimColor);
            hint.AddThemeFontSizeOverride("font_size", 13);
            container.AddChild(hint);
        }

        private void AddSection(Node parent, string title, string content)
        {
            var sectionPanel = new PanelContainer();
            sectionPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = SectionBg,
                BorderWidthLeft = 2, BorderWidthRight = 2,
                BorderWidthTop = 2, BorderWidthBottom = 2,
                BorderColor = AccentColor,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 14, ContentMarginRight = 14,
                ContentMarginTop = 10, ContentMarginBottom = 10
            });
            parent.AddChild(sectionPanel);

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 4);
            sectionPanel.AddChild(vbox);

            var titleLabel = new Label { Text = title };
            titleLabel.AddThemeColorOverride("font_color", HeaderColor);
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(titleLabel);

            var contentLabel = new Label { Text = content };
            contentLabel.AddThemeColorOverride("font_color", TextColor);
            contentLabel.AddThemeFontSizeOverride("font_size", 13);
            vbox.AddChild(contentLabel);
        }

        public void Toggle()
        {
            _isActive = !_isActive;
            Visible = _isActive;
        }

        public bool IsActive => Visible;
    }
}
