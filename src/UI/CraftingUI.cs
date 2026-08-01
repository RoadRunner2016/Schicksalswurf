using Godot;

namespace Schicksalswurf.UI
{
    using Core;
    using Characters;

    /// <summary>
    /// Crafting UI for brewing potions and creating items.
    /// </summary>
    public partial class CraftingUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _container;
        private ScrollContainer _scroll;
        private Label _infoLabel;
        private bool _isActive = false;
        private Character _crafter;
        private Party _party;

        private static readonly Color PanelBg = new(0.06f, 0.06f, 0.1f, 0.98f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color DimColor = new(0.5f, 0.48f, 0.42f);

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

            var title = new Label { Text = "⚗ HANDWERK ⚗" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 28);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _container.AddChild(title);

            _infoLabel = new Label();
            _infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _infoLabel.AddThemeFontSizeOverride("font_size", 14);
            _infoLabel.AddThemeColorOverride("font_color", TextColor);
            _container.AddChild(_infoLabel);

            _scroll = new ScrollContainer();
            _scroll.CustomMinimumSize = new Vector2(0, 350);
            _scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
            _container.AddChild(_scroll);

            var list = new VBoxContainer { Name = "RecipeList" };
            list.AddThemeConstantOverride("separation", 6);
            _scroll.AddChild(list);

            var closeBtn = new Button { Text = "Schliessen (Esc)" };
            closeBtn.CustomMinimumSize = new Vector2(0, 40);
            closeBtn.Pressed += Close;
            _container.AddChild(closeBtn);
        }

        public void Show(Party party)
        {
            _party = party;
            _crafter = party.Members[0];
            _isActive = true;
            Visible = true;
            RefreshUI();
        }

        public void Close()
        {
            _isActive = false;
            Visible = false;
        }

        public bool IsActive => _isActive;

        private void RefreshUI()
        {
            _infoLabel.Text = $"Handwerker: {_crafter.Name} (Lvl {_crafter.Level})";

            var list = _scroll.GetNode<VBoxContainer>("RecipeList");
            foreach (Node child in list.GetChildren())
                child.QueueFree();

            foreach (var recipe in CraftingSystem.Recipes)
            {
                var canCraft = CraftingSystem.CanCraft(_crafter, recipe);

                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 10);

                var info = new VBoxContainer();
                info.SizeFlagsHorizontal = SizeFlags.ExpandFill;

                var name = new Label { Text = recipe.Name };
                name.AddThemeFontSizeOverride("font_size", 16);
                name.AddThemeColorOverride("font_color", canCraft ? TextColor : DimColor);
                info.AddChild(name);

                var ingredients = string.Join(", ", System.Linq.Enumerable.Select(recipe.Ingredients, kv => $"{kv.Value}x {kv.Key}"));
                var desc = new Label { Text = $"{recipe.Description}\nZutaten: {ingredients}\nSchwierigkeit: {recipe.Difficulty}" };
                desc.AddThemeFontSizeOverride("font_size", 13);
                desc.AddThemeColorOverride("font_color", DimColor);
                info.AddChild(desc);

                row.AddChild(info);

                var craftBtn = new Button { Text = "Herstellen", Disabled = !canCraft };
                craftBtn.Pressed += () =>
                {
                    var (success, msg) = CraftingSystem.Craft(_crafter, recipe);
                    _infoLabel.Text = msg;
                    RefreshUI();
                };
                row.AddChild(craftBtn);

                list.AddChild(row);
            }
        }
    }
}
