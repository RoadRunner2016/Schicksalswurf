using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.UI
{
    using Characters;

    /// <summary>
    /// Inventory and shop UI. Shows party inventory, allows using items,
    /// equipping/unequipping, and buying/selling from a merchant.
    /// </summary>
    public partial class InventoryUI : Control
    {
        private Party _party;
        private int _selectedMember = 0;
        private bool _isShopMode = false;
        private bool _isActive = false;

        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        private HBoxContainer _memberTabs;
        private VBoxContainer _itemList;
        private VBoxContainer _shopList;
        private Label _goldLabel;
        private Label _infoLabel;

        // Shop inventory
        private static readonly List<object> ShopStock = new()
        {
            Weapon.Dagger, Weapon.ShortSword, Weapon.LongSword, Weapon.BattleAxe,
            Weapon.ShortBow, Weapon.LongBow,
            Armor.Leather, Armor.Chain, Armor.Plate,
            Item.HealthPotion, Item.ManaPotion, Item.StaminaPotion,
            Item.Torch, Item.Lockpick, Item.Rope
        };

        private static readonly Color PanelBg = new(0.07f, 0.07f, 0.11f, 0.95f);
        private static readonly Color BorderColor = new(0.4f, 0.35f, 0.15f, 0.9f);
        private static readonly Color TextColor = new(0.88f, 0.85f, 0.72f);
        private static readonly Color HeaderColor = new(0.9f, 0.75f, 0.3f);
        private static readonly Color SelectedColor = new(1.0f, 0.85f, 0.2f);
        private static readonly Color DimColor = new(0.55f, 0.52f, 0.45f);
        private static readonly Color GoldColor = new(1.0f, 0.85f, 0.2f);

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
                ContentMarginLeft = 20, ContentMarginRight = 20,
                ContentMarginTop = 16, ContentMarginBottom = 16
            });
            AddChild(_mainPanel);

            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 10);
            _mainContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainPanel.AddChild(_mainContainer);

            // Title
            var title = new Label { Text = "🎒 INVENTAR 🎒" };
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 22);
            title.AddThemeColorOverride("font_color", HeaderColor);
            _mainContainer.AddChild(title);

            // Gold
            _goldLabel = new Label();
            _goldLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _goldLabel.AddThemeFontSizeOverride("font_size", 16);
            _goldLabel.AddThemeColorOverride("font_color", GoldColor);
            _mainContainer.AddChild(_goldLabel);

            // Member tabs
            _memberTabs = new HBoxContainer();
            _memberTabs.AddThemeConstantOverride("separation", 12);
            _memberTabs.Alignment = BoxContainer.AlignmentMode.Center;
            _mainContainer.AddChild(_memberTabs);

            // Two columns
            var columns = new HBoxContainer();
            columns.AddThemeConstantOverride("separation", 24);
            columns.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _mainContainer.AddChild(columns);

            // Left: inventory
            var leftCol = new VBoxContainer();
            leftCol.AddThemeConstantOverride("separation", 6);
            leftCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            columns.AddChild(leftCol);

            var invTitle = new Label { Text = "Inventar:" };
            invTitle.AddThemeColorOverride("font_color", HeaderColor);
            invTitle.AddThemeFontSizeOverride("font_size", 16);
            leftCol.AddChild(invTitle);

            _itemList = new VBoxContainer();
            _itemList.AddThemeConstantOverride("separation", 3);
            leftCol.AddChild(_itemList);

            // Right: shop (only visible in shop mode)
            var rightCol = new VBoxContainer();
            rightCol.AddThemeConstantOverride("separation", 6);
            rightCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            columns.AddChild(rightCol);

            var shopTitle = new Label { Text = "Haendler:" };
            shopTitle.AddThemeColorOverride("font_color", HeaderColor);
            shopTitle.AddThemeFontSizeOverride("font_size", 16);
            rightCol.AddChild(shopTitle);

            _shopList = new VBoxContainer();
            _shopList.AddThemeConstantOverride("separation", 3);
            rightCol.AddChild(_shopList);

            // Info label
            _infoLabel = new Label();
            _infoLabel.AddThemeColorOverride("font_color", TextColor);
            _infoLabel.AddThemeFontSizeOverride("font_size", 13);
            _infoLabel.CustomMinimumSize = new Vector2(0, 40);
            _mainContainer.AddChild(_infoLabel);

            // Hint
            var hint = new Label { Text = "Tab: Charakter wechseln  |  I: Schliessen" };
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            hint.AddThemeColorOverride("font_color", DimColor);
            hint.AddThemeFontSizeOverride("font_size", 12);
            _mainContainer.AddChild(hint);
        }

        public void SetParty(Party party) => _party = party;

        public void Toggle(bool shopMode = false)
        {
            if (_party == null || _party.Members.Count == 0) return;
            _isShopMode = shopMode;
            _isActive = !_isActive;
            Visible = _isActive;
            if (_isActive)
            {
                _selectedMember = 0;
                RefreshUI();
            }
        }

        public bool IsActive => Visible;

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!Visible) return;

            if (@event is InputEventKey key && key.Pressed)
            {
                if (key.Keycode == Key.Tab)
                {
                    _selectedMember = (_selectedMember + 1) % _party.Members.Count;
                    RefreshUI();
                    GetViewport().SetInputAsHandled();
                }
                else if (key.Keycode == Key.I)
                {
                    Toggle();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        private void RefreshUI()
        {
            if (_party == null) return;
            var member = _party.Members[_selectedMember];

            _goldLabel.Text = $"Gold: {_party.Gold}G";

            // Member tabs
            foreach (Node child in _memberTabs.GetChildren())
                child.QueueFree();

            for (int i = 0; i < _party.Members.Count; i++)
            {
                var m = _party.Members[i];
                bool selected = i == _selectedMember;
                var lbl = new Label();
                lbl.Text = selected ? $"▶ {m.Name}" : $"  {m.Name}  ";
                lbl.AddThemeColorOverride("font_color", selected ? SelectedColor : DimColor);
                lbl.AddThemeFontSizeOverride("font_size", 15);
                _memberTabs.AddChild(lbl);
            }

            // Inventory items
            foreach (Node child in _itemList.GetChildren())
                child.QueueFree();

            // Equipment
            AddItemRow(_itemList, "Waffe:", member.EquippedWeapon?.Name ?? "-", null);
            AddItemRow(_itemList, "Ruestung:", member.EquippedArmor?.Name ?? "-", null);

            // Personal inventory
            foreach (var item in member.Inventory)
            {
                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 8);

                var nameLbl = new Label { Text = $"• {item.Name}" };
                nameLbl.AddThemeColorOverride("font_color", TextColor);
                nameLbl.AddThemeFontSizeOverride("font_size", 13);
                row.AddChild(nameLbl);

                var priceLbl = new Label { Text = $"({item.Value}G)" };
                priceLbl.AddThemeColorOverride("font_color", DimColor);
                priceLbl.AddThemeFontSizeOverride("font_size", 12);
                row.AddChild(priceLbl);

                if (item.IsConsumable)
                {
                    var useBtn = new Button { Text = "Benutzen", CustomMinimumSize = new Vector2(90, 24) };
                    useBtn.AddThemeFontSizeOverride("font_size", 11);
                    useBtn.Pressed += () => UseItem(member, item);
                    row.AddChild(useBtn);
                }

                var sellBtn = new Button { Text = "Verkaufen", CustomMinimumSize = new Vector2(90, 24) };
                sellBtn.AddThemeFontSizeOverride("font_size", 11);
                sellBtn.Pressed += () => SellItem(member, item);
                row.AddChild(sellBtn);

                _itemList.AddChild(row);
            }

            // Shared inventory
            foreach (var item in _party.SharedInventory)
            {
                var row = new HBoxContainer();
                row.AddThemeConstantOverride("separation", 8);

                var nameLbl = new Label { Text = $"• {item.Name} [Gruppe]" };
                nameLbl.AddThemeColorOverride("font_color", TextColor);
                nameLbl.AddThemeFontSizeOverride("font_size", 13);
                row.AddChild(nameLbl);

                _itemList.AddChild(row);
            }

            if (member.Inventory.Count == 0 && _party.SharedInventory.Count == 0)
            {
                var empty = new Label { Text = "  (leer)" };
                empty.AddThemeColorOverride("font_color", DimColor);
                _itemList.AddChild(empty);
            }

            // Shop
            foreach (Node child in _shopList.GetChildren())
                child.QueueFree();

            if (_isShopMode)
            {
                foreach (var stockItem in ShopStock)
                {
                    string name;
                    int price;
                    bool isWeapon = stockItem is Weapon;
                    bool isArmor = stockItem is Armor;
                    bool isItem = stockItem is Item;

                    if (isWeapon) { var w = (Weapon)stockItem; name = w.Name; price = w.Value; }
                    else if (isArmor) { var a = (Armor)stockItem; name = a.Name; price = a.Value; }
                    else { var it = (Item)stockItem; name = it.Name; price = it.Value; }

                    var row = new HBoxContainer();
                    row.AddThemeConstantOverride("separation", 8);

                    var nameLbl = new Label { Text = $"• {name}" };
                    nameLbl.AddThemeColorOverride("font_color", TextColor);
                    nameLbl.AddThemeFontSizeOverride("font_size", 13);
                    row.AddChild(nameLbl);

                    var priceLbl = new Label { Text = $"{price}G" };
                    priceLbl.AddThemeColorOverride("font_color", GoldColor);
                    priceLbl.AddThemeFontSizeOverride("font_size", 13);
                    row.AddChild(priceLbl);

                    var buyBtn = new Button { Text = "Kaufen", CustomMinimumSize = new Vector2(80, 24) };
                    buyBtn.AddThemeFontSizeOverride("font_size", 11);
                    buyBtn.Pressed += () => BuyItem(stockItem, price);
                    row.AddChild(buyBtn);

                    _shopList.AddChild(row);
                }
            }
            else
            {
                var noShop = new Label { Text = "(Kein Haendler in der Naehe)" };
                noShop.AddThemeColorOverride("font_color", DimColor);
                _shopList.AddChild(noShop);
            }
        }

        private void AddItemRow(Node parent, string label, string value, object _)
        {
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 6);

            var lbl = new Label { Text = label };
            lbl.AddThemeColorOverride("font_color", DimColor);
            lbl.AddThemeFontSizeOverride("font_size", 13);
            hbox.AddChild(lbl);

            var val = new Label { Text = value };
            val.AddThemeColorOverride("font_color", TextColor);
            val.AddThemeFontSizeOverride("font_size", 13);
            hbox.AddChild(val);

            parent.AddChild(hbox);
        }

        private void UseItem(Character member, Item item)
        {
            bool used = false;
            if (item.Id == "health_potion")
                used = member.UseHealthPotion();
            else if (item.Id == "mana_potion")
                used = member.UseManaPotion();

            if (used)
                _infoLabel.Text = $"{member.Name} benutzt {item.Name}.";
            else
                _infoLabel.Text = $"Kann {item.Name} nicht benutzen.";

            RefreshUI();
        }

        private void SellItem(Character member, Item item)
        {
            int sellPrice = item.Value / 2;
            member.Inventory.Remove(item);
            _party.Gold += sellPrice;
            _infoLabel.Text = $"{item.Name} fuer {sellPrice}G verkauft.";
            RefreshUI();
        }

        private void BuyItem(object stockItem, int price)
        {
            if (_party.Gold < price)
            {
                _infoLabel.Text = "Nicht genug Gold!";
                return;
            }

            _party.Gold -= price;
            var member = _party.Members[_selectedMember];

            if (stockItem is Weapon w)
            {
                member.EquippedWeapon = w;
                _infoLabel.Text = $"{w.Name} gekauft und ausgeruestet.";
            }
            else if (stockItem is Armor a)
            {
                member.EquippedArmor = a;
                _infoLabel.Text = $"{a.Name} gekauft und ausgeruestet.";
            }
            else if (stockItem is Item it)
            {
                member.Inventory.Add(it);
                _infoLabel.Text = $"{it.Name} gekauft.";
            }

            RefreshUI();
        }
    }
}
