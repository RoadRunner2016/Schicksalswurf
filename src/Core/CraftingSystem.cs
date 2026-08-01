using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Crafting/Alchemy system for brewing potions and creating items.
    /// </summary>
    public static class CraftingSystem
    {
        public class Recipe
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public Dictionary<string, int> Ingredients { get; set; } = new();
            public Item Result { get; set; }
            public int ResultCount { get; set; } = 1;
            public int RequiredLevel { get; set; } = 1;
            public string RequiredSkill { get; set; } = "alchimie";
            public int Difficulty { get; set; } = 12;
        }

        private static List<Recipe> _recipes = new();

        public static IReadOnlyList<Recipe> Recipes => _recipes;

        public static void Initialize()
        {
            _recipes.Clear();

            _recipes.Add(new Recipe
            {
                Id = "heiltrank_klein",
                Name = "Kleiner Heiltrank",
                Description = "Braut einen kleinen Heiltrank aus Heilwurzeln.",
                Ingredients = new() { { "heilwurzel", 2 } },
                Result = Item.HealthPotion,
                ResultCount = 1,
                RequiredLevel = 1,
                Difficulty = 10
            });

            _recipes.Add(new Recipe
            {
                Id = "heiltrank_gross",
                Name = "Grosser Heiltrank",
                Description = "Braut einen grossen Heiltrank.",
                Ingredients = new() { { "heilwurzel", 3 }, { "mohn", 1 } },
                Result = Item.GreaterHealingPotion,
                ResultCount = 1,
                RequiredLevel = 3,
                Difficulty = 15
            });

            _recipes.Add(new Recipe
            {
                Id = "manatrank",
                Name = "Manatrank",
                Description = "Braut einen Manatrank.",
                Ingredients = new() { { "manakristall", 2 }, { "mohn", 1 } },
                Result = Item.ManaPotion,
                ResultCount = 1,
                RequiredLevel = 2,
                Difficulty = 12
            });

            _recipes.Add(new Recipe
            {
                Id = "dietrich",
                Name = "Dietrich",
                Description = "Stellt einen Dietrich her.",
                Ingredients = new() { { "eisenbarren", 1 } },
                Result = Item.Lockpick,
                ResultCount = 2,
                RequiredLevel = 1,
                Difficulty = 8,
                RequiredSkill = "schleichen"
            });

            _recipes.Add(new Recipe
            {
                Id = "gifttrank",
                Name = "Gifttrank",
                Description = "Braut einen Gifttrank fuer Waffen.",
                Ingredients = new() { { "giftbeutel", 2 }, { "heilwurzel", 1 } },
                Result = Item.PoisonPotion,
                ResultCount = 1,
                RequiredLevel = 4,
                Difficulty = 18
            });
        }

        public static bool CanCraft(Character crafter, Recipe recipe)
        {
            if (crafter.Level < recipe.RequiredLevel) return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                int count = crafter.Inventory.FindAll(i => i.Id == ingredient.Key).Count;
                if (count < ingredient.Value) return false;
            }
            return true;
        }

        public static (bool success, string message) Craft(Character crafter, Recipe recipe)
        {
            if (!CanCraft(crafter, recipe))
            {
                bool hasLevel = crafter.Level >= recipe.RequiredLevel;
                return (false, hasLevel ? "Nicht genug Zutaten." : $"Benoetigt Level {recipe.RequiredLevel}.");
            }

            // Consume ingredients
            foreach (var ingredient in recipe.Ingredients)
            {
                int toRemove = ingredient.Value;
                while (toRemove > 0)
                {
                    var item = crafter.Inventory.Find(i => i.Id == ingredient.Key);
                    if (item != null)
                    {
                        crafter.Inventory.Remove(item);
                        toRemove--;
                    }
                    else break;
                }
            }

            // Skill check
            var result = crafter.PerformSkillCheck(recipe.RequiredSkill, recipe.Difficulty);
            if (result.Success)
            {
                for (int i = 0; i < recipe.ResultCount; i++)
                    crafter.Inventory.Add(recipe.Result);
                return (true, $"{crafter.Name} stellt {recipe.ResultCount}x {recipe.Name} her!");
            }
            else
            {
                return (false, $"{crafter.Name} scheitert beim Brauen von {recipe.Name}. Zutaten verloren.");
            }
        }
    }
}
